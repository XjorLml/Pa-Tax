// Node 22+ (built-in WebSocket), installed Chromium browser; no npm packages.
import { spawn } from 'node:child_process';
import { mkdir, readFile, writeFile, mkdtemp, rm } from 'node:fs/promises';
import { resolve, join, sep } from 'node:path';
import { pathToFileURL, fileURLToPath } from 'node:url';
import { createHash } from 'node:crypto';

const browser = process.argv[2];
if (!browser) throw new Error('Usage: node tools/check_prototype.mjs <browser-executable> [artifact-directory]');
const root = fileURLToPath(new URL('../', import.meta.url));
const out = resolve(process.argv[3] || join(root, 'artifacts/brand-refresh'));
await mkdir(out, { recursive: true });
const profile = await mkdtemp(join(out, 'browser-profile-'));
const child = spawn(browser, ['--headless=new', '--disable-gpu', '--no-first-run', '--no-default-browser-check',
  '--disable-background-networking', '--disable-extensions', '--allow-file-access-from-files',
  '--remote-debugging-port=0', `--user-data-dir=${profile}`, 'about:blank'], { windowsHide: true, stdio: 'ignore' });
let ws, id = 0;
const pending = new Map(), results = [], errors = [];
const delay = ms => new Promise(r => setTimeout(r, ms));
async function until(fn, ms = 8000) {
  const end = Date.now() + ms;
  do { const v = await fn(); if (v) return v; await delay(80); } while (Date.now() < end);
  throw new Error('Timed out waiting for expected browser state');
}
function cdp(method, params = {}) {
  return new Promise((resolve, reject) => {
    const n = ++id;
    const timeout = setTimeout(() => { pending.delete(n); reject(new Error(`CDP timeout: ${method}`)); }, 15000);
    pending.set(n, { resolve: x => { clearTimeout(timeout); resolve(x); }, reject: e => { clearTimeout(timeout); reject(e); } });
    ws.send(JSON.stringify({ id: n, method, params }));
  });
}
async function js(expression) {
  const r = await cdp('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
  if (r.exceptionDetails) throw new Error(r.exceptionDetails.text + ': ' + r.exceptionDetails.exception?.description);
  return r.result.value;
}
async function check(name, expression) {
  const ok = await js(expression);
  results.push({ name, pass: !!ok });
  if (!ok) throw new Error('Check failed: ' + name);
}
const click = selector => js(`(() => { const el=document.querySelector(${JSON.stringify(selector)}); if(!el) throw Error('Missing '+${JSON.stringify(selector)}); el.click(); })()`);
const set = (selector, value, event = 'change') => js(`(() => { const el=document.querySelector(${JSON.stringify(selector)}); if(!el) throw Error('Missing field'); el.value=${JSON.stringify(value)}; el.dispatchEvent(new Event(${JSON.stringify(event)},{bubbles:true})); })()`);
const textHas = text => `document.querySelector('#app').textContent.includes(${JSON.stringify(text)})`;
async function route(hash) { await js(`location.hash=${JSON.stringify(hash)}`); await delay(100); }
async function open(query = '', hash = '#/directory', width = 1440, height = 1100) {
  await cdp('Emulation.setDeviceMetricsOverride', { width, height, deviceScaleFactor: 1, mobile: false });
  await cdp('Page.navigate', { url: pathToFileURL(join(root, 'prototypes/index.html')).href + '?' + query + hash });
  await until(() => js(`!!document.querySelector('#app .hero, #app #screen-title')`));
  await delay(850);
}
async function screenshot(name) {
  const r = await cdp('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
  await writeFile(join(out, name + '.png'), Buffer.from(r.data, 'base64'));
}
async function layout(name) {
  await check(name + ': no horizontal overflow', `(() => {
    const selectors=['html','#device','.content','.page','.main','.single','.pane-detail'];
    return selectors.every(s=>Array.from(document.querySelectorAll(s)).every(e=>e.clientWidth===0 || e.scrollWidth<=e.clientWidth+2));
  })()`);
  await check(name + ': local image assets', `Array.from(document.querySelectorAll('#app img')).every(i=>i.complete && i.naturalWidth>0)`);
}

let version;
try {
  child.on('error', e => errors.push(e.message));
  const port = await until(async () => {
    try { return (await readFile(join(profile, 'DevToolsActivePort'), 'utf8')).split('\n')[0]; } catch { return null; }
  }, 15000);
  version = await (await fetch(`http://127.0.0.1:${port}/json/version`)).json();
  const target = await (await fetch(`http://127.0.0.1:${port}/json/new?about:blank`, { method: 'PUT' })).json();
  ws = new WebSocket(target.webSocketDebuggerUrl);
  await new Promise((r, reject) => { ws.onopen = r; ws.onerror = reject; });
  ws.onmessage = e => {
    const msg = JSON.parse(e.data);
    if (process.env.PROTOTYPE_CDP_DEBUG) console.log(JSON.stringify(msg).slice(0, 700));
    if (msg.id && pending.has(msg.id)) {
      const p = pending.get(msg.id); pending.delete(msg.id);
      if (msg.error) p.reject(new Error(msg.error.message)); else p.resolve(msg.result);
    }
    if (msg.method === 'Runtime.exceptionThrown') errors.push(msg.params.exceptionDetails.exception?.description || msg.params.exceptionDetails.text);
  };
  ws.onclose = () => { for (const p of pending.values()) p.reject(new Error('Browser connection closed')); pending.clear(); };
  await cdp('Page.enable'); await cdp('Runtime.enable');
  await open('theme=light&notes=0');
  await check('Desktop default and Tax Buddy name', `document.querySelector('#device').classList.contains('desktop') && document.title.includes('Tax Buddy') && document.querySelector('#device').dataset.look==='taxbuddy'`);
  await layout('Desktop directory'); await screenshot('directory-desktop-light');
  await js(`document.querySelector('.page').scrollTop=500`);
  await screenshot('directory-cards-desktop-light');
  await js(`document.querySelector('.page').scrollTop=0`);
  await check('Six initial cards', `document.querySelectorAll('.results .card').length===6`);
  await click('[data-action="more"]');
  await check('Paging adds remaining cards', `document.querySelectorAll('.results .card').length===8`);
  await set('#q', 'zzzz-no-match', 'input');
  await until(() => js(textHas('No matches')));
  await check('No match has recovery', `!!document.querySelector('[data-fkey="clear-all"]')`);
  await click('[data-fkey="clear-all"]');
  await click('[data-action="service"][data-val="Payroll"]');
  await check('Service filter is selected', `document.querySelector('[data-val="Payroll"]').getAttribute('aria-pressed')==='true' && [...document.querySelectorAll('.results .card')].every(c=>c.innerText.includes('Payroll'))`);
  await set('#sel-dir', 'empty'); await delay(800);
  await check('Empty directory distinct from no-match', textHas('No accountants listed yet'));
  await set('#sel-dir', 'error'); await delay(800);
  await check('Load failure distinct from empty', `!!document.querySelector('[data-action="retry-dir"]')`);
  await click('[data-action="retry-dir"]'); await delay(800);
  await check('Directory retry recovers', `document.querySelectorAll('.results .card').length>0`);
  await route('#/profile/a1'); await layout('Desktop profile'); await screenshot('profile-desktop-light');
  await check('Profile contains portfolio, reviews and contact action', `${textHas('Portfolio highlights')} && ${textHas('Reviews')} && !!document.querySelector('[data-fkey="msg-btn"]')`);
  await route('#/profile/a9'); await check('Removed profile state', textHas('no longer available'));

  await open('device=phone&theme=light&notes=0', '#/compose/a3', 390, 1100);
  await set('#composer', 'Synthetic browser check: bookkeeping enquiry.', 'input');
  await click('[data-action="send"]'); await delay(100);
  await check('Visitor draft shown at sign-in', textHas('Synthetic browser check: bookkeeping enquiry.'));
  await click('[data-provider="Google"]'); await delay(1500);
  await check('Agreement required before role choice', `document.querySelector('[data-role="client"]').disabled`);
  await click('#agree'); await click('[data-role="client"]'); await delay(150);
  await check('Draft retained after sign-in', `document.querySelector('#composer').value==='Synthetic browser check: bookkeeping enquiry.'`);
  await set('#sel-send', 'fail'); await click('[data-action="send"]'); await delay(1300);
  await check('Failed send offers retry', `!!document.querySelector('[data-action="retry-msg"]')`);
  await click('[data-action="retry-msg"]'); await delay(800);
  await check('Retry avoids duplicate message', `Array.from(document.querySelectorAll('.bubble')).filter(e=>e.textContent==='Synthetic browser check: bookkeeping enquiry.').length===1 && !document.querySelector('[data-action="retry-msg"]')`);
  await set('#composer', 'Draft during lost connection', 'input'); await click('#net-off');
  await check('Offline notice and draft', `${textHas('Connection lost.')} && document.querySelector('#composer').value==='Draft during lost connection'`);
  await click('#net-on');
  await route('#/me'); await click('[data-action="toggle-email"]');
  await check('Email preference toggles', textHas('Email notifications: Off'));

  await open('device=desktop&role=client&theme=light&notes=0', '#/review/a1');
  await click('[data-action="save-review"]');
  await check('Review requires stars', textHas('Choose a rating'));
  await click('[data-n="5"]'); await set('#rv-text', 'Synthetic review for prototype verification.', 'input');
  await click('[data-action="save-review"]'); await delay(100);
  await check('Review shown under abbreviated name', `${textHas('Synthetic review for prototype verification.')} && ${textHas('Maria S.')}`);
  await set('#sel-role', 'accountant');
  await click('[data-action="reply-review"]'); await set('#reply-text', 'Thank you for your feedback.', 'input'); await click('[data-action="save-reply"]');
  await check('Accountant reply is displayed', textHas('Thank you for your feedback.'));
  await route('#/thread/c1'); await click('[data-action="block"]');
  await check('Block requires confirmation', `!!document.querySelector('[data-action="block-confirm"]') && !!document.querySelector('[data-action="dlg-keep"]')`);
  await click('[data-action="block-confirm"]');
  await check('Blocked chat hides composer', `!document.querySelector('#composer') && !!document.querySelector('[data-action="unblock"]')`);
  await click('[data-action="unblock"]');
  await check('Unblock restores composer', `!!document.querySelector('#composer')`);
  await route('#/me/profile'); await set('#f-summary', 'Synthetic unsaved profile edit.', 'input'); await route('#/directory');
  await check('Unsaved profile guard', `!!document.querySelector('[data-action="dlg-keep"]')`);
  await click('[data-action="dlg-discard"]');

  for (const [device, width, theme] of [['desktop',1440,'dark'],['phone',390,'light'],['phone',390,'dark'],['phone',320,'light'],['desktop',700,'light']]) {
    const name = `${device}-${width}-${theme}`;
    await open(`device=${device}&theme=${theme}&notes=0`, '#/directory', width, 1100);
    await layout(name + ' directory'); await screenshot('directory-' + name);
    await route('#/profile/a1'); await layout(name + ' profile');
    if (device === 'phone' && width === 390) await screenshot('profile-' + name);
    await open(`device=${device}&theme=${theme}&notes=0&role=client`, '#/thread/c1', width, 1100);
    await layout(name + ' chat');
    if (width === 390 || width === 1440) await screenshot('chat-' + name);
  }
  for (const look of ['taxbuddy','modern','ledger','neighbourly','clear']) {
    await open(`device=desktop&theme=light&look=${look}&notes=0`);
    await check(`${look} comparison direction`, `document.querySelectorAll('.results .card').length===6`);
  }
  await open('device=desktop&theme=light&notes=0');
  await cdp('Emulation.setEmulatedMedia', { features: [{ name:'prefers-reduced-motion', value:'reduce' }] });
  await check('Reduced motion removes transitions', `getComputedStyle(document.querySelector('.card')).transitionDuration==='0s'`);
  await cdp('Emulation.setEmulatedMedia', { features: [] });
  for (const hash of ['#/about','#/help','#/for-accountants','#/privacy','#/terms','#/no-such-page','#/error']) {
    await route(hash); await check(`${hash} renders`, `!!document.querySelector('#screen-title') && document.querySelector('#app').innerText.length>100`);
  }
  await check('No stale earlier names in visible page', `!/AccApp|Kuwenta/i.test(document.body.innerText)`);
  const contrastJS = `(() => {
      const style=getComputedStyle(document.querySelector('#device'));
      const lum=h=>{const v=[1,3,5].map(i=>parseInt(h.slice(i,i+2),16)/255).map(x=>x<=.04045?x/12.92:((x+.055)/1.055)**2.4);return v[0]*.2126+v[1]*.7152+v[2]*.0722;};
      const pairs=[['text','surface',4.5],['text','app-bg',4.5],['muted','surface',4.5],['muted','app-bg',4.5],['muted','hero',4.5],['muted','hero-art',4.5],['link','surface',4.5],['on-primary','primary',4.5],['on-primary','primary-strong',4.5],['on-soft','primary-soft',4.5],['danger','danger-soft',4.5],['warn','warn-soft',4.5],['ctl','surface',3],['ctl','app-bg',3],['ctl','hero',3],['focus','surface',3],['focus','hero',3]];
      return pairs.map(([fg,bg,minimum])=>{const a=lum(style.getPropertyValue('--'+fg).trim()),b=lum(style.getPropertyValue('--'+bg).trim());const ratio=(Math.max(a,b)+.05)/(Math.min(a,b)+.05);return {fg,bg,ratio,minimum,pass:ratio>=minimum};});
    })()`;
  for (const theme of ['light','dark']) {
    await open(`device=desktop&theme=${theme}&notes=0`);
    const contrast = await js(contrastJS);
    await writeFile(join(out, `contrast-${theme}.json`), JSON.stringify(contrast,null,2));
    results.push({name:`${theme}: 17 semantic contrast pairs`,pass:contrast.every(r=>r.pass)});
    if (contrast.some(r=>!r.pass)) throw new Error('Contrast failure: '+theme);
  }

  // BD08: palette options for the Tax Buddy look. Paper & Forest stays the default.
  const token = name => `getComputedStyle(document.querySelector('#device')).getPropertyValue('--${name}').trim()`;
  await open('device=desktop&theme=light&notes=0&palette=not-a-palette');
  await check('Default palette is Paper & Forest with file logos', `document.querySelector('#device').dataset.palette==='forest' && document.querySelector('#sel-palette').value==='forest' && !!document.querySelector('.sitebar img.brand-logo') && !document.querySelector('#app .brand-inline')`);
  const forestPrimary = await js(token('primary'));
  const paletteContrast = {};
  for (const palette of ['ink','lagoon','plum','marigold']) {
    for (const theme of ['light','dark']) {
      await open(`device=desktop&theme=${theme}&notes=0&palette=${palette}`);
      await check(`${palette} ${theme}: palette applied with token-coloured logo`, `document.querySelector('#device').dataset.palette==='${palette}' && document.querySelector('#sel-palette').value==='${palette}' && !!document.querySelector('.sitebar svg.brand-logo.brand-inline') && !document.querySelector('.sitebar img.brand-logo') && getComputedStyle(document.querySelector('.sitebar .lg-bg')).fill!=='none' && document.querySelectorAll('.results .card').length===6`);
      const contrast = await js(contrastJS);
      paletteContrast[`${palette}-${theme}`] = contrast;
      results.push({name:`${palette} ${theme}: 17 semantic contrast pairs`,pass:contrast.every(r=>r.pass)});
      if (contrast.some(r=>!r.pass)) throw new Error(`Contrast failure: ${palette} ${theme}`);
      await screenshot(`palette-${palette}-${theme}`);
    }
    await check(`${palette}: primary differs from the current palette`, `${token('primary')}!==${JSON.stringify(forestPrimary)}`);
    await open(`device=phone&theme=light&notes=0&palette=${palette}`, '#/directory', 390, 1100);
    await layout(`${palette} phone directory`);
    await check(`${palette}: phone header uses the token-coloured icon`, `!!document.querySelector('.appbar .brand svg.brand-inline')`);
    await screenshot(`palette-${palette}-phone-390-light`);
  }
  await writeFile(join(out, 'contrast-palettes.json'), JSON.stringify(paletteContrast,null,2));
  await open('device=desktop&theme=light&notes=0&palette=ink');
  await set('#sel-palette', 'plum');
  await check('Palette control switches the palette in place', `document.querySelector('#device').dataset.palette==='plum' && !!document.querySelector('.sitebar svg.brand-inline')`);
  await set('#sel-palette', 'forest');
  await check('Palette control returns to the current palette', `${token('primary')}===${JSON.stringify(forestPrimary)} && !!document.querySelector('.sitebar img.brand-logo')`);
  await open('device=desktop&theme=light&notes=0&look=modern&palette=ink');
  await check('Palette options do not affect earlier looks', `document.querySelector('#sel-palette').disabled && !document.querySelector('#app .brand-inline') && document.querySelectorAll('.results .card').length===6`);
  await open('device=phone&theme=light&notes=0', '#/directory', 390, 1100);
  await check('Phone simulation controls start collapsed', `!document.querySelector('#prototype-controls').open`);
  await cdp('Input.dispatchKeyEvent', {type:'keyDown',key:'Tab',code:'Tab',windowsVirtualKeyCode:9});
  await cdp('Input.dispatchKeyEvent', {type:'keyUp',key:'Tab',code:'Tab',windowsVirtualKeyCode:9});
  await check('Keyboard reaches visible skip link', `document.activeElement.classList.contains('skip-link') && getComputedStyle(document.activeElement).top==='8px'`);
  await click('.skip-link');
  await check('Skip link preserves route and focuses prototype', `document.activeElement.id==='device' && location.hash==='#/directory'`);
  await check('Phone controls meet 44px target', `[...document.querySelectorAll('#app .btn,#app .chip,#app .tabbar a,#app .tabbar button,#app .sortctl select,#app .infolinks a')].every(e=>e.getBoundingClientRect().height>=44)`);

  // F03: private lists, request lifecycle, progress and generated service files.
  await open('device=desktop&notes=0&theme=light', '#/workspace');
  await check('CS05 visitor workspace is private', `${textHas('Sign in to see your workspace')} && !document.querySelector('.work-person')`);
  await open('device=desktop&notes=0&theme=light&role=accountant', '#/workspace');
  await check('CS01 accountant groups three scoped clients', `document.querySelectorAll('.work-person').length===3 && !!document.querySelector('a[href="#/clients/u-maria"]')`);
  await layout('Accountant client list'); await screenshot('clients-desktop-light');
  await set('#sel-work-data','loading'); await check('Workspace loading state', `!!document.querySelector('[aria-busy="true"]')`);
  await set('#sel-work-data','empty'); await check('Workspace empty state', textHas('No clients yet'));
  await set('#sel-work-data','error'); await click('[data-action="ws-retry-load"]');
  await check('Workspace load error retries', `document.querySelectorAll('.work-person').length===3`);
  await route('#/clients/u-maria');
  await check('Client detail shows only scoped services', `${textHas('Bookkeeping')} && !${textHas('Tax advisory')} && !!document.querySelector('[data-action="ws-add-service"]')`);
  await set('[data-scope="client:u-maria"][data-field="service"]','Financial statements'); await click('[data-action="ws-add-service"]'); await delay(100);
  const assignedHash = await js('location.hash');
  await check('Accountant assigns service to an accepted client', `${textHas('Financial statements')} && document.querySelectorAll('.work-tasks li').length===3`);
  await route('#/clients/u-maria'); await click('[data-action="ws-add-service"]'); await delay(100);
  await check('Repeated accountant service assignment reuses workspace', `location.hash===${JSON.stringify(assignedHash)}`);
  await route('#/clients/u-unknown'); await check('Unknown client unavailable', textHas('This workspace is not available'));

  await open('device=desktop&notes=0&theme=light&role=client', '#/workspace');
  await check('CS02 client has two added accountants', `document.querySelectorAll('.work-accountants>section').length===2 && ${textHas('Andrea Villanueva')} && ${textHas('Elaine Navarro')} && !${textHas('Jose Reyes')}`);
  await route('#/profile/a2'); await click('[data-action="ws-save"]'); await route('#/workspace');
  await check('Adding accountant populates private list', `document.querySelectorAll('.work-accountants>section').length===3 && ${textHas('Benjie Ramos')}`);
  await route('#/profile/a2'); await check('Added accountant has no duplicate add control', `!document.querySelector('[data-action="ws-save"]')`);
  await route('#/workspace'); await click('[data-action="ws-unsave"][data-id="a2"]');
  await check('Removing saved entry keeps service work', `document.querySelectorAll('.work-accountants>section').length===2 && !!document.querySelector('a[href="#/work/e1"]')`);
  await route('#/clients/u-maria'); await check('Client cannot open accountant client list', textHas('This workspace is not available'));
  await route('#/request/a1'); await click('[data-action="ws-request"]');
  await check('Request validates selected offered service', textHas('Choose a service offered'));
  await set('[data-scope="request:a1"][data-field="service"]','Tax filing');
  await click('[data-action="ws-request"]'); await delay(150);
  const requestedHash = await js('location.hash');
  await check('CS03 request awaits acceptance without edit controls', `${textHas('Awaiting acceptance')} && !document.querySelector('[data-action="ws-save-tasks"]') && !document.querySelector('[data-action="ws-upload"]')`);
  await route('#/request/a1'); await set('[data-scope="request:a1"][data-field="service"]','Tax filing'); await click('[data-action="ws-request"]'); await delay(100);
  await check('Repeated service request reuses workspace', `location.hash===${JSON.stringify(requestedHash)}`);
  await set('#sel-role','accountant'); await click('[data-action="ws-accept"]');
  await check('Acceptance creates starting checklist', `document.querySelectorAll('.work-tasks li').length===3 && !document.querySelector('[data-action="ws-accept"]')`);
  await set('#sel-role','client'); await check('Client sees accepted read-only checklist', `document.querySelectorAll('.work-tasks li').length===3 && !document.querySelector('[data-bind="work-task"]')`);
  await set('#sel-role','accountant'); await route('#/work/e2'); await click('[data-action="ws-decline"]');
  await check('Decline distinct from active work', `${textHas('Declined')} && !document.querySelector('[data-action="ws-upload"]')`);

  await route('#/work/e1');
  await set('.work-tasks li:nth-child(2) select','Completed'); await set('#sel-work-save','fail'); await click('[data-action="ws-save-tasks"]');
  await check('CS04 failed progress save preserves draft and saved total', `document.querySelector('.work-tasks li:nth-child(2) select').value==='Completed' && ${textHas('1 of 4 tasks complete')} && ${textHas('could not be saved')}`);
  await set('#sel-work-save','ok'); await click('#net-off'); await click('[data-action="ws-save-tasks"]');
  await check('Offline progress never queues', textHas('1 of 4 tasks complete'));
  await click('#net-on'); await click('[data-action="ws-save-tasks"]');
  await check('Saved progress recomputes completed ratio', textHas('2 of 4 tasks complete'));
  await set('[data-scope="e1"][data-field="newTask"]','Confirm final totals','input'); await click('[data-action="ws-add-task"]');
  await check('Adding a task updates denominator', textHas('2 of 5 tasks complete'));
  await set('[data-scope="e1"][data-field="note"]','Synthetic shared progress update.','input');
  await set('#sel-work-save','fail'); await click('[data-action="ws-post-note"]');
  await check('Failed shared note retains text', `document.querySelector('[data-field="note"]').value==='Synthetic shared progress update.'`);
  await set('#sel-work-save','ok'); await click('[data-action="ws-post-note"]');
  await check('Shared note has author and timestamp', `document.querySelector('.work-timeline').textContent.includes('Synthetic shared progress update.') && !!document.querySelector('.work-timeline time[datetime]')`);
  await click('.work-tasks li:first-child input'); await click('[data-action="ws-save-tasks"]');
  await check('Checklist checkbox can reopen a completed task', textHas('1 of 5 tasks complete'));
  await click('.work-tasks li:first-child input'); await click('[data-action="ws-save-tasks"]');
  await check('Checklist checkbox saves completion', textHas('2 of 5 tasks complete'));
  await screenshot('service-accountant-desktop-light');
  await set('#sel-role','client');
  await check('Client sees saved progress and note without write controls', `${textHas('2 of 5 tasks complete')} && ${textHas('Synthetic shared progress update.')} && !document.querySelector('[data-field="note"]') && !document.querySelector('[data-bind="work-task"]')`);
  await set('#sel-role','accountant');

  const fileCount = () => js(`document.querySelectorAll('.work-files>li').length`);
  const initialFiles = await fileCount();
  await set('[data-field="sample"]','oversized'); await click('[data-action="ws-upload"]');
  await check('CS07 oversize rejected', `${textHas('exceeds the proposed 5 MiB limit')} && document.querySelectorAll('.work-files>li').length===${initialFiles}`);
  await set('[data-field="sample"]','unsupported'); await click('[data-action="ws-upload"]');
  await check('Unsupported file type rejected', textHas('sample type is not allowed'));
  await set('[data-field="sample"]','receipts'); await set('[data-field="fileTask"]','2'); await set('#sel-work-upload','fail');
  await click('[data-action="ws-upload"]');
  await check('Upload exposes busy progress and disables repetition', `document.querySelector('[data-action="ws-upload"]').disabled && !!document.querySelector('progress[aria-label="Sample upload progress"]')`);
  await delay(950);
  await check('Upload failure keeps sample/task selection', `${textHas('Sample upload failed')} && document.querySelector('[data-field="sample"]').value==='receipts' && document.querySelector('[data-field="fileTask"]').value==='2' && document.querySelectorAll('.work-files>li').length===${initialFiles}`);
  await js(`document.querySelector('.work-upload').scrollIntoView({block:'center'})`); await screenshot('service-upload-failure');
  await set('#sel-work-upload','ok'); await click('[data-action="ws-upload"]'); await delay(950);
  await check('CS06 retry adds one task-associated file', `document.querySelectorAll('.work-files>li').length===${initialFiles+1} && document.querySelector('.work-files>li:last-child').textContent.includes('Prepare monthly statements')`);
  const addedFile = await js(`document.querySelector('.work-files>li:last-child [data-action="ws-download"]').dataset.file`);
  await set('#sel-role','client');
  await check('Counterpart downloads but cannot remove uploaded file', `!!document.querySelector('[data-action="ws-download"][data-file="${addedFile}"]') && !document.querySelector('[data-action="ws-remove-file"][data-file="${addedFile}"]')`);
  // Intercept only the browser download handoff; inspect generated Blob bytes, no real files.
  await js(`window.__originalObjectURL=URL.createObjectURL;window.__originalAnchorClick=HTMLAnchorElement.prototype.click;URL.createObjectURL=b=>{window.__fixtureBlob=b;return window.__originalObjectURL(b)};HTMLAnchorElement.prototype.click=function(){window.__fixtureFilename=this.download;};`);
  await click(`[data-action="ws-download"][data-file="${addedFile}"]`);
  await check('Generated PDF download is a synthetic fixture', `(async()=>window.__fixtureFilename==='receipts-sample.pdf' && window.__fixtureBlob.type==='application/pdf' && (await window.__fixtureBlob.text()).startsWith('%PDF-1.4') && (await window.__fixtureBlob.text()).includes('Demo only'))()`);
  await js(`URL.createObjectURL=window.__originalObjectURL;HTMLAnchorElement.prototype.click=window.__originalAnchorClick;`);
  await set('#sel-role','accountant');
  await click('[data-action="ws-upload"]'); await click('#net-off'); await delay(950);
  await check('Loss of connection during upload preserves file count', `${textHas('Connection lost during upload')} && document.querySelectorAll('.work-files>li').length===${initialFiles+1}`);
  await click('#net-on'); await click('[data-action="ws-upload"]'); await set('#sel-role','client'); await delay(950);
  await check('Account switch prevents late upload insertion', `document.querySelectorAll('.work-files>li').length===${initialFiles+1}`);
  await set('#sel-role','accountant');
  await check('Interrupted uploader gets explicit retry state', textHas('Upload interrupted by an account change'));
  await click(`[data-action="ws-remove-file"][data-file="${addedFile}"]`);
  await check('CS08 document removal requires confirmation', `!!document.querySelector('[data-action="ws-confirm-remove"]')`);
  await click('[data-action="dlg-keep"]'); await check('Cancel removal preserves file', `document.querySelectorAll('.work-files>li').length===${initialFiles+1}`);
  await click(`[data-action="ws-remove-file"][data-file="${addedFile}"]`); await click('[data-action="ws-confirm-remove"]');
  await check('Confirmed own-file removal updates workspace', `document.querySelectorAll('.work-files>li').length===${initialFiles}`);
  await set('#sel-role','newAccountant');
  await check('CS05 unrelated accountant cannot inspect service or files', `${textHas('This workspace is not available')} && !${textHas('monthly-sales.csv')} && !${textHas('Maria Santos')}`);
  await route('#/workspace'); await check('Unrelated accountant has empty scoped list', textHas('No clients yet'));

  for (const [device,width,theme,role] of [['desktop',1440,'light','accountant'],['phone',390,'dark','client'],['phone',320,'light','accountant'],['desktop',700,'dark','client']]) {
    const name=`work-${device}-${width}-${theme}-${role}`;
    await open(`device=${device}&theme=${theme}&role=${role}&notes=0`,'#/workspace',width,1100);
    await layout(name+' list'); await screenshot(name+'-list');
    await route('#/work/e1'); await layout(name+' service'); await screenshot(name+'-service');
    if(device==='phone') await check(name+' service touch targets', `[...document.querySelectorAll('.work-panel .btn,.work-field input,.work-field select,.work-field textarea,.work-task-status select,.work-task-name input')].every(e=>e.getBoundingClientRect().height>=44)`);
  }
  if (errors.length) throw new Error(errors.join('\n'));
  console.log(`PASS: ${results.length} prototype checks; ${version.Browser}`);
} catch (e) {
  errors.push(e.stack || e.message); console.error(e.message); process.exitCode = 1;
  if (ws?.readyState === WebSocket.OPEN) await screenshot('failure').catch(() => {});
} finally {
  const sourceHashes = {};
  for (const file of ['prototypes/index.html','prototypes/taxbuddy.css','prototypes/service-workspace.js','tools/check_prototype.mjs',
    ...['taxbuddy-logo-light.svg','taxbuddy-logo-dark.svg','taxbuddy-icon.svg','taxbuddy-icon-dark.svg','favicon.svg'].map(f=>'prototypes/assets/'+f)]) {
    sourceHashes[file] = createHash('sha256').update(await readFile(join(root,file))).digest('hex');
  }
  await writeFile(join(out, 'results.json'), JSON.stringify({ browser: version?.Browser, node: process.version, observedAt: new Date().toISOString(), sourceHashes, results, errors }, null, 2));
  // Close only the browser started above; the isolated profile never touches real browser data.
  if (ws?.readyState === WebSocket.OPEN) { await cdp('Browser.close').catch(() => {}); ws.close(); }
  if (child.exitCode === null) { await Promise.race([new Promise(r => child.once('exit', r)), delay(3000)]); if (child.exitCode === null) child.kill(); }
  if (!resolve(profile).startsWith(out + sep)) throw new Error('Refusing profile cleanup outside the selected artifact directory');
  await rm(profile, { recursive: true, force: true, maxRetries: 5, retryDelay: 300 }).catch(e => console.error('Profile cleanup pending:', e.message));
}
