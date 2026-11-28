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
// Choose a file in a file input, as a person would from the device's file chooser.
async function chooseFile(selector, path) {
  const r = await cdp('Runtime.evaluate', { expression: `document.querySelector(${JSON.stringify(selector)})` });
  if (!r.result || !r.result.objectId) throw Error('Missing ' + selector);
  await cdp('DOM.setFileInputFiles', { files: [path], objectId: r.result.objectId });
  await delay(400);
}
// A synthetic 2 x 2 PNG for the profile picture checks; written to the artifact directory.
const samplePicture = join(out, 'sample-picture.png');
await writeFile(samplePicture, Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAFklEQVR4nGNkWPqfgYGBiQEMGBkY/gMACR0CA1rvOfsAAAAASUVORK5CYII=', 'base64'));
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
  // C03 (resolved 2026-11-12): the directory behaves like the planned static page with a plain GET form.
  const address = `document.querySelector('[data-address]').textContent`;
  await check('C03 directory is a submitted search form', `!!document.querySelector('form[data-dir-form][role="search"] button[type="submit"][data-action="dir-submit"]') && !document.querySelector('[data-action="more"], .sortctl select')`);
  const firstPage = await js(`[...document.querySelectorAll('.results .card')].map(c=>c.dataset.fkey)`);
  await click('[data-action="page"][data-val="2"]');
  await check('Page link shows the remaining cards', `document.querySelectorAll('.results .card').length===2 && ${address}.endsWith('/?page=2')`);
  await check('Pages never repeat or skip an accountant', `(() => { const all=${JSON.stringify(firstPage)}.concat([...document.querySelectorAll('.results .card')].map(c=>c.dataset.fkey)); return all.length===8 && new Set(all).size===8; })()`);
  await js(`document.querySelector('.page').scrollTop=1e5`); await screenshot('directory-page2-desktop-light'); await js(`document.querySelector('.page').scrollTop=0`);
  await click('[data-action="page"][data-val="1"]');
  await set('#q', 'zzzz-no-match', 'input');
  await check('C03 typing alone does not filter', `document.querySelectorAll('.results .card').length===6 && !${textHas('No matches')}`);
  await click('[data-action="dir-submit"]');
  await until(() => js(textHas('No matches')));
  await check('Submitted search is in the address', `${address}.includes('?q=zzzz-no-match')`);
  await check('No match has recovery', `!!document.querySelector('[data-fkey="clear-all"]')`);
  await click('[data-fkey="clear-all"]');
  await click('[data-action="service"][data-val="Payroll"]');
  await check('Service filter is selected', `document.querySelector('[data-val="Payroll"]').getAttribute('aria-pressed')==='true' && [...document.querySelectorAll('.results .card')].every(c=>c.innerText.includes('Payroll'))`);
  await click('[data-action="sort"][data-val="rating"]');
  await check('Sort link keeps the filter and is in the address', `document.querySelector('[data-action="sort"][data-val="rating"]').getAttribute('aria-current')==='true' && ${address}.endsWith('/?service=Payroll&sort=rating')`);
  await click('[data-action="sort"][data-val="name"]');
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
  // Profile picture (R24; F06 PP10): E is the current design. PP09's panel (A) is what the site shows without script.
  await check('Picture design control starts on the current design', `document.querySelector('#sel-picture').value==='e' && document.querySelectorAll('#sel-picture option').length===5`);
  await set('#sel-picture', 'a');
  await check('Account avatar has a pencil and the picture panel starts closed', `!!document.querySelector('.me-card .avatar-edit .pencil') && document.querySelector('.me-card .avatar-edit').getAttribute('aria-label')==='Change profile picture' && !document.querySelector('#picture')`);
  await click('[data-action="pic-open"]');
  await check('Pencil opens the picture panel', `!!document.querySelector('#picture #picture-file') && ${textHas('Upload picture')} && !document.querySelector('[data-action="pic-remove"]')`);
  await click('[data-action="pic-upload"]');
  await check('Upload without a file asks for a photo', `${textHas('Choose a photo first.')} && document.querySelector('#picture .banner').getAttribute('role')==='alert'`);
  await screenshot('account-picture-panel');
  await click('[data-action="pic-close"]');
  await check('Close hides the picture panel', `!document.querySelector('#picture') && !!document.querySelector('.me-card .avatar-edit')`);
  // The designs not chosen (B, C, D), kept for comparison, and the current design E with a real file.
  await set('#sel-picture', 'c'); await click('[data-action="pic-open"]');
  await check('Option C: the pencil opens a small menu', `!!document.querySelector('.me-card .pic-menu [data-action="pic-pick"]') && document.querySelector('.avatar-edit').getAttribute('aria-expanded')==='true' && !document.querySelector('#picture')`);
  await screenshot('account-picture-option-c');
  await set('#sel-picture', 'd'); await click('[data-action="pic-open"]'); await delay(150);
  await check('Option D: the pencil opens a separate picture page', `location.hash==='#/me/picture' && !!document.querySelector('.picture-page .avatar.xl') && ${textHas('Save picture')}`);
  await screenshot('account-picture-option-d');
  await set('#sel-picture', 'e');
  await check('Option E: back on Account, with the hidden picker and no panel', `location.hash==='#/me' && !!document.querySelector('#pic-direct') && !document.querySelector('#picture') && !document.querySelector('.pic-menu')`);
  await chooseFile('#pic-direct', samplePicture);
  await check('Option E: a chosen photo is previewed and waits for Save', `!!document.querySelector('.me-card .avatar.has-picture img') && ${textHas('Preview. Not saved yet.')} && !!document.querySelector('[data-action="pic-save"]') && !document.querySelector('.sitebar .avatar.has-picture, .userlink .avatar.has-picture')`);
  await screenshot('account-picture-option-e');
  await click('[data-action="pic-save"]');
  await check('Option E: Save keeps the picture and offers Remove', `${textHas('Picture updated.')} && !!document.querySelector('.me-card [data-action="pic-remove"]') && !document.querySelector('[data-action="pic-save"]')`);
  await click('[data-action="pic-remove"]');
  await check('Option E: Remove brings back the initials', `${textHas('Picture removed. Your initials are shown instead.')} && !document.querySelector('.me-card .avatar.has-picture')`);
  await set('#sel-picture', 'b');
  await check('Option B: the pencil with the hidden picker', `!!document.querySelector('#pic-direct') && !!document.querySelector('.me-card .avatar-edit')`);
  await chooseFile('#pic-direct', samplePicture);
  await check('Option B: a chosen photo is saved at once', `${textHas('Picture updated.')} && !!document.querySelector('.me-card .avatar.has-picture img') && !!document.querySelector('.me-card [data-action="pic-remove"]')`);
  await screenshot('account-picture-option-b');
  await click('[data-action="pic-remove"]');
  await set('#sel-picture', 'a');
  await check('Design A, the panel used without script, is still selectable', `document.querySelector('#sel-picture').value==='a' && !document.querySelector('#pic-direct') && !document.querySelector('#picture')`);
  await click('[data-action="pic-open"]'); await chooseFile('#picture-file', samplePicture); await click('[data-action="pic-upload"]'); await delay(400);
  await check('Current design A: upload shows the picture, the result and Remove', `${textHas('Picture updated.')} && !!document.querySelector('.me-card .avatar.has-picture img') && !!document.querySelector('#picture [data-action="pic-remove"]')`);
  await screenshot('account-picture-option-a');
  await click('[data-action="pic-remove"]'); await click('[data-action="pic-close"]');
  await set('#sel-picture', 'e');
  await check('Picture design returns to the current design E', `document.querySelector('#sel-picture').value==='e' && !!document.querySelector('#pic-direct') && !document.querySelector('#picture')`);

  await open('device=desktop&role=client&theme=light&notes=0');
  await check('C03 directory cards carry no per-user tag', `document.querySelectorAll('.results .card').length===6 && !document.querySelector('.results .tag.strong') && !document.querySelector('.results').textContent.includes('Chatting')`);
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
  await check('Phone controls meet 44px target', `[...document.querySelectorAll('#app .btn,#app .chip,#app .tabbar a,#app .tabbar button,#app .sortlink,#app .pager a,#app .infolinks a')].every(e=>e.getBoundingClientRect().height>=44)`);
  await set('#svc', 'Payroll');
  await check('C03 phone service menu waits for Search', `document.querySelectorAll('.results .card').length===6 && !document.querySelector('[data-address]').textContent.includes('service=')`);
  await click('.hero-search>[data-action="dir-submit"]');
  await check('Phone Search applies the service menu', `document.querySelector('[data-address]').textContent.endsWith('/?service=Payroll') && [...document.querySelectorAll('.results .card')].every(c=>c.innerText.includes('Payroll'))`);

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
  // Client details on Account (the user's choice B of 2026-11-25; C05, not in the PRD): view, edit in place, a refused
  // phone, save; the form on a phone; the accountant's page for the client shows them.
  await open('device=desktop&role=client&theme=light&notes=0', '#/me');
  await check('Client details: Account shows Your details with Edit details', `!!document.querySelector('.opt-cd-card [data-action="cd-edit"]')`);
  await click('[data-action="cd-edit"]'); await delay(300);
  await check('Client details: Edit details opens the form in place and moves focus to it', `!!document.querySelector('.opt-cd-card.editing #cd-name') && document.activeElement.id==='cd-h'`);
  await set('#cd-phone', 'call me', 'input'); await click('[data-action="cd-save"]'); await delay(200);
  await check('Client details: a phone with letters is refused', textHas('Enter a phone number using digits, spaces, + or -.'));
  await set('#cd-phone', '+63 900 000 0299', 'input'); await click('[data-action="cd-save"]'); await delay(300);
  await check('Client details: Save shows the new phone on the card', `!document.querySelector('.opt-cd-card.editing') && document.querySelector('.opt-cd-card').textContent.includes('+63 900 000 0299')`);
  await layout('Client details on Account');
  await open('device=phone&role=client&theme=dark&notes=0', '#/me/edit', 390, 900);
  await layout('Client details form on a phone');
  await check('Client details: form touch targets on a phone', `[...document.querySelectorAll('.opt-cd-card input,.opt-cd-card select,.opt-cd-card .btn')].every(e=>e.getBoundingClientRect().height>=44)`);
  await open('device=desktop&role=accountant&theme=light&notes=0', '#/clients/u-maria');
  await check('Client details: the accountant\'s page for Maria shows About Maria', `!!document.querySelector('.opt-about-client')`);
  // My profile: the preview beside the form (the user's choice B of 2026-11-25). The directory card as last saved sits
  // beside the form on wide desktop pages, and in a closed disclosure above the form on phones and narrow desktop pages.
  await open('device=desktop&role=accountant&theme=light&notes=0', '#/me/profile');
  await check('My profile: the directory card sits beside the form', `(() => { const a=document.querySelector('.editor-grid>aside.editor-preview'), f=document.querySelector('.editor-grid>.form');
    return !!a && !!a.querySelector('.card') && getComputedStyle(a).display!=='none' && a.getBoundingClientRect().left>=f.getBoundingClientRect().right && getComputedStyle(document.querySelector('.editor-preview-mini')).display==='none'; })()`);
  await set('#f-city', 'Synthetic Preview City', 'input');
  await check('My profile: the card keeps what was last saved while typing', `!document.querySelector('.editor-preview .card').textContent.includes('Synthetic Preview City')`);
  await click('[data-action="save-profile"]');
  await until(() => js(`!!document.querySelector('.editor-preview .card') && document.querySelector('.editor-preview .card').textContent.includes('Synthetic Preview City')`));
  await check('My profile: Save shows the new city on the card', `document.querySelector('.editor-preview .card').textContent.includes('Synthetic Preview City') && ${textHas('Saved. Your profile is listed in the directory.')}`);
  await layout('My profile with the preview');
  await open('device=desktop&role=accountant&theme=dark&notes=0', '#/me/profile', 700, 1000);
  await check('My profile: on a narrow desktop page the card is a closed disclosure above the form', `(() => { const m=document.querySelector('.editor-preview-mini');
    return getComputedStyle(m).display!=='none' && !m.open && getComputedStyle(document.querySelector('.editor-preview')).display==='none' && m.getBoundingClientRect().bottom<=document.querySelector('.editor-grid').getBoundingClientRect().top; })()`);
  await layout('My profile on a 700 px desktop page');
  await open('device=phone&role=newAccountant&theme=light&notes=0', '#/me/profile', 390, 900);
  await click('.editor-preview-mini summary'); await delay(100);
  await check('My profile: before listing, the phone preview says where the card will appear', `(() => { const m=document.querySelector('.editor-preview-mini');
    return m.open && m.textContent.includes('Not listed yet') && !m.querySelector('.card') && !document.querySelector('.editor-preview') && m.querySelector('summary').getBoundingClientRect().height>=44; })()`);
  await layout('My profile on a phone, not listed');
  await open('device=desktop&role=accountant&theme=light&notes=0&opt=editor-current', '#/me/profile');
  await check('My profile: opt=editor-current shows the one long form without a preview', `!document.querySelector('.editor-preview, .editor-preview-mini') && !!document.querySelector('.single-inner>.form')`);
  // Accountant dashboard options (2026-11-26; archived the same day, kept working): A "Needs you" and B tiles at /dashboard, C an
  // overview on Clients. The default has no dashboard. States: Andrea, busy (Elaine), new (Sam), listed with no clients
  // (Francis) and a failed load; phones at 320 px with 44 px targets.
  const dashTargets = sel => `[...document.querySelectorAll(${JSON.stringify(sel)})].every(e=>e.getBoundingClientRect().height>=44)`;
  const navFirst = `document.querySelector('.sitenav a')`;
  await open('device=desktop&role=accountant&theme=light&notes=0', '#/workspace');
  await check('Dashboard: the default has no Dashboard item, and Clients is current', `!document.querySelector('.sitenav a[href="#/dashboard"]') && document.querySelector('.sitenav a[aria-current="page"]').getAttribute('href')==='#/workspace' && !document.querySelector('.dash-split')`);
  await route('#/dashboard');
  await check('Dashboard: without an option, /dashboard is not a page', textHas('We couldn\'t find that page'));
  await open('device=desktop&role=accountant&theme=light&notes=0&opt=dash-a', '#/dashboard');
  await check('Dashboard A: Dashboard is first in the site bar and current', `${navFirst}.getAttribute('href')==='#/dashboard' && ${navFirst}.getAttribute('aria-current')==='page'`);
  await check('Dashboard A: Andrea\'s listing line and four things that need her', `${textHas('Listed in the directory')} && document.querySelectorAll('.dash-needs li').length===4 && !!document.querySelector('.dash-needs [data-action="ws-accept"][data-id="e2"]') && ${textHas('Jose Reyes is waiting for your reply')} && ${textHas('Maria Santos is waiting for your reply')} && ${textHas('Noel B. left 4 stars. No reply yet')}`);
  await click('.dash-needs [data-action="ws-accept"][data-id="e2"]'); await delay(300);
  await check('Dashboard A: an accepted request leaves the list and becomes work in progress', `document.querySelectorAll('.dash-needs li').length===3 && !document.querySelector('[data-action="ws-accept"][data-id="e2"]') && ${textHas('1 service in progress')}`);
  await layout('Dashboard A, desktop');
  await open('device=desktop&role=listedAccountant&theme=dark&notes=0&opt=dash-a', '#/dashboard');
  await check('Dashboard A: listed with no clients is all caught up', `${textHas('You\'re all caught up. Nothing needs you right now. Clients find you in the directory')} && ${textHas('No services yet.')} && !document.querySelector('.dash-needs li')`);
  await layout('Dashboard A, listed with no clients, dark');
  await open('device=phone&role=newAccountant&theme=dark&notes=0&opt=dash-a', '#/dashboard', 320, 900);
  await check('Dashboard A, phone: Dashboard takes the Accountants tab\'s place and is current', `(() => { const t=[...document.querySelectorAll('.tabbar a')]; return t.length===4 && t[0].getAttribute('href')==='#/dashboard' && t[0].getAttribute('aria-current')==='page' && !t.some(a=>a.getAttribute('href')==='#/directory'); })()`);
  await check('Dashboard A, new accountant: creating the profile comes first', `(() => { const li=document.querySelector('.dash-needs li'); return li.textContent.includes('Your listing') && li.textContent.includes('Create your profile') && document.querySelectorAll('.dash-needs li').length===1 && ${textHas('1 of 6 details')}; })()`);
  await check('Dashboard A, phone: 44 px touch targets', dashTargets('.dash .btn, .dash-links a, .tabbar a'));
  await layout('Dashboard A, new accountant, 320 px');
  await open('device=phone&role=busyAccountant&theme=light&notes=0&opt=dash-b', '#/dashboard', 320, 900);
  await check('Dashboard B, busy: the tiles count 3 requests, 3 unread, 2 active services and a 4.0 rating', `(() => { const t=[...document.querySelectorAll('.dash-tile strong')].map(s=>s.textContent); return t.join(',')==='3,3,2,4.0'; })()`);
  await check('Dashboard B, busy: five sections, three requests to answer and two reviews without a reply', `document.querySelectorAll('.dash-section').length===5 && document.querySelectorAll('.dash-requests [data-action="ws-accept"]').length===3 && ${textHas('Carl U., 5 stars')} && ${textHas('Bea R., 3 stars')}`);
  await click('.dash-tile[data-target="dash-rev"]'); await delay(200);
  await check('Dashboard B: a tile moves focus to its section', `document.activeElement && document.activeElement.id==='dash-rev'`);
  await check('Dashboard B, phone: 44 px touch targets', dashTargets('.dash .btn, .dash-links a, .tabbar a, .dash-tile, a.dash-row'));
  await layout('Dashboard B, busy, 320 px');
  await open('device=desktop&role=busyAccountant&theme=dark&notes=0&opt=dash-b', '#/dashboard');
  await check('Dashboard B, desktop: four tiles in one row', `(() => { const t=[...document.querySelectorAll('.dash-tile')].map(e=>Math.round(e.getBoundingClientRect().top)); return t.length===4 && new Set(t).size===1; })()`);
  await layout('Dashboard B, busy, desktop dark');
  await open('device=desktop&role=busyAccountant&theme=light&notes=0&opt=dash-c', '#/workspace');
  await check('Dashboard C: Clients stays current, with no Dashboard item', `!document.querySelector('.sitenav a[href="#/dashboard"]') && document.querySelector('.sitenav a[aria-current="page"]').getAttribute('href')==='#/workspace'`);
  await check('Dashboard C, busy: eight things need Elaine, beside her listing, services and reviews, then her clients', `document.querySelectorAll('.dash-split .dash-needs li').length===8 && document.querySelectorAll('.dash-side .dash-card').length===3 && !!document.querySelector('#dash-clients + .work-people') && !document.querySelector('.work-stats')`);
  await layout('Dashboard C, busy, desktop');
  await open('device=phone&role=busyAccountant&theme=dark&notes=0&opt=dash-c', '#/workspace', 320, 900);
  await check('Dashboard C, phone: "Needs you" comes first and the tabs are unchanged', `document.querySelector('.dash-needs').getBoundingClientRect().top < document.querySelector('.dash-side').getBoundingClientRect().top && [...document.querySelectorAll('.tabbar a')].some(a=>a.getAttribute('href')==='#/directory') && document.querySelector('.tabbar a[aria-current="page"]').getAttribute('href')==='#/workspace'`);
  await check('Dashboard C, phone: 44 px touch targets', dashTargets('.dash-split .btn, .tabbar a'));
  await layout('Dashboard C, busy, 320 px');
  await open('device=desktop&role=accountant&theme=light&notes=0&opt=dash-a&wsdata=error', '#/dashboard');
  await check('Dashboard: a failed load says so, with Try again', `${textHas('We couldn\'t load your dashboard')} && !!document.querySelector('[data-action="ws-retry-load"]') && !document.querySelector('.dash-needs')`);
  await click('[data-action="ws-retry-load"]'); await delay(200);
  await check('Dashboard: Try again loads the dashboard', `!!document.querySelector('.dash-needs')`);
  await open('device=phone&role=accountant&theme=light&notes=0&opt=dash-c&wsdata=error', '#/workspace', 320, 900);
  await check('Dashboard C: a failed load keeps the Clients page\'s message', textHas('Could not load your workspace'));
  // Page titles on the workspace pages (2026-11-26; proposals, not adopted): one title per page instead of a breadcrumb,
  // a page-type heading, an eyebrow and a second title. The default keeps today's header.
  const h1s = `[...document.querySelectorAll('#app h1')].map(h=>h.textContent.trim())`;
  const crumbs = `[...document.querySelectorAll('#app .crumbs a, #app .heads-trail nav a')].map(a=>a.textContent.trim())`;
  await open('device=desktop&role=accountant&theme=light&notes=0', '#/clients/u-maria');
  await check('Page titles: the default keeps the breadcrumb, "Client services" and the eyebrow', `${h1s}.join()==='Client services' && !!document.querySelector('.work-intro .eyebrow') && ${textHas('Client services')}`);
  await open('device=desktop&role=accountant&theme=light&notes=0&opt=heads-a', '#/workspace');
  await check('Page titles A: Clients has one title, no breadcrumb, eyebrow or slogan', `${h1s}.join()==='Clients' && !document.querySelector('.crumbs, .eyebrow, .work-intro') && !${textHas('A clear view of your client work.')} && !!document.querySelector('.heads-sub')`);
  await route('#/clients/u-maria');
  await check('Page titles A: a client\'s page is titled with the name, under "Clients"', `${h1s}.join()==='Maria Santos' && ${crumbs}.join('/')==='Clients' && document.querySelector('.heads-sub').textContent.includes('Only this client and you')`);
  await route('#/work/e1');
  await check('Page titles A: a service sits under "Clients / Maria Santos", and its sentence says who can see it', `${h1s}.join()==='Bookkeeping' && ${crumbs}.join('/')==='Clients/Maria Santos' && !document.querySelector('.work-intro') && document.querySelector('.heads-sub').textContent==='Only Maria and you can see this work.'`);
  await layout('Page titles A, service, desktop');
  await open('device=desktop&role=client&theme=dark&notes=0&opt=heads-a', '#/request/a1');
  await check('Page titles A: Request a service says its name once', `${h1s}.join()==='Request a service' && [...document.querySelectorAll('#app h1, #app h2')].filter(h=>h.textContent.includes('Request a service')).length===1`);
  await open('device=phone&role=accountant&theme=light&notes=0&opt=heads-a', '#/work/e1', 320, 900);
  await check('Page titles A, phone: the app bar names the service and Back goes to the client', `document.querySelector('#screen-title').textContent==='Bookkeeping' && document.querySelector('.appbar a[aria-label="Back"]').getAttribute('href')==='#/clients/u-maria' && !document.querySelector('.work-intro, .eyebrow')`);
  await layout('Page titles A, service, 320 px');
  await open('device=desktop&role=accountant&theme=dark&notes=0&opt=heads-b', '#/work/e1');
  await check('Page titles B: Back to Maria Santos, the name and a privacy pill in one bar', `(() => { const b=document.querySelector('.heads-bar'); return !!b && b.querySelector('.heads-back').textContent.trim()==='Maria Santos' && b.querySelector('h1').textContent==='Bookkeeping' && b.querySelector('.heads-private').textContent.includes('Private to you and Maria') && !document.querySelector('.heads-sub, .work-intro, .crumbs'); })()`);
  await layout('Page titles B, service, desktop dark');
  await open('device=phone&role=client&theme=dark&notes=0&opt=heads-b', '#/work/e1', 320, 900);
  await check('Page titles B, phone: the pill names the accountant', `document.querySelector('#screen-title').textContent==='Bookkeeping' && document.querySelector('.heads-row .heads-private').textContent.includes('Private to you and Andrea')`);
  await layout('Page titles B, client service, 320 px');
  await open('device=desktop&role=accountant&theme=light&notes=0&opt=heads-c', '#/work/e1');
  await check('Page titles C: "Clients / Maria Santos / Bookkeeping" on one line, the name the h1', `(() => { const t=document.querySelector('.heads-trail'); const tops=[...t.querySelectorAll('nav a, h1')].map(e=>Math.round(e.getBoundingClientRect().bottom)); return ${crumbs}.join('/')==='Clients/Maria Santos' && t.querySelector('h1').textContent==='Bookkeeping' && Math.max(...tops)-Math.min(...tops)<=6; })()`);
  await open('device=phone&role=accountant&theme=light&notes=0&opt=heads-c', '#/work/e1', 320, 900);
  await check('Page titles C, phone: the pages above are 44 px links at the top', `[...document.querySelectorAll('.heads-phone-trail a')].map(a=>a.textContent).join('/')==='Clients/Maria Santos' && [...document.querySelectorAll('.heads-phone-trail a')].every(a=>a.getBoundingClientRect().height>=44)`);
  await layout('Page titles C, service, 320 px');
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
