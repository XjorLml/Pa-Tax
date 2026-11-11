/* F03: disposable, in-memory prototype. Never reads or transmits a real file.
   Relationship/edit/file policies demonstrated here are proposals (PRD D24–D28). */
window.createServiceWorkspace = function (ctx) {
  'use strict';
  const { S, PROFILES, CONVS, esc, initials, tone, fmtShort, fmtTime, nid, ago, render, toast, go, icon, getListed, signinBtn, stateHTML, announce } = ctx;
  const states = ['Not started', 'In progress', 'Waiting for client', 'Completed'];
  const clients = { 'u-maria':'Maria Santos', 'u-jose':'Jose Reyes', 'u-liza':'Liza Fernandez' };
  const saved = { 'u-maria':['a1','a5'] };
  const tasks = (names, statuses = []) => names.map((name,i) => ({ id:nid('task'), name, status:statuses[i] || states[0] }));
  const work = [
    {id:'e1', clientId:'u-maria', accountantId:'a1', service:'Bookkeeping', phase:'active',
      tasks:tasks(['Collect monthly records','Reconcile bank entries','Prepare monthly statements','Review and hand over'],[states[3],states[1],states[2]]),
      updates:[{id:'h1',name:'Andrea Villanueva',at:ago(120),text:'Bank reconciliation is underway. Please attach the missing receipt summary to Prepare monthly statements.'}],
      files:[{id:'f1',name:'monthly-sales.csv',sample:'sales',taskIndex:0,owner:'client:u-maria',by:'Maria Santos',at:ago(150)}]},
    {id:'e2', clientId:'u-jose', accountantId:'a1', service:'Tax filing', phase:'requested', tasks:[],files:[],updates:[{id:'h2',name:'Jose Reyes',at:ago(80),text:'Requested Tax filing. Please confirm whether you can help.'}]},
    {id:'e3', clientId:'u-maria', accountantId:'a5', service:'Tax advisory', phase:'active',tasks:tasks(['Review business details','Prepare recommendations','Discuss next steps'],[states[3],states[1]]),files:[],updates:[{id:'h3',name:'Elaine Navarro',at:ago(200),text:'I have reviewed your business details and am preparing recommendations.'}]},
    {id:'e4', clientId:'u-liza', accountantId:'a1', service:'Financial statements', phase:'active',tasks:tasks(['Prepare statements','Hand over statements'],[states[3],states[3]]),files:[],updates:[{id:'h4',name:'Andrea Villanueva',at:ago(500),text:'Statements prepared and handed over.'}]}
  ];
  const samples = {
    receipts:{name:'receipts-sample.pdf',label:'Sample receipts (PDF)',mime:'application/pdf'},
    sales:{name:'monthly-sales.csv',label:'Monthly sales (CSV)',mime:'text/csv'},
    summary:{name:'service-summary.txt',label:'Service summary (TXT)',mime:'text/plain'},
    oversized:{name:'oversized-sample.pdf',label:'Oversized sample (6 MiB, rejected)',mime:'application/pdf',invalid:'The sample exceeds the proposed 5 MiB limit.'},
    unsupported:{name:'unsupported-sample.exe',label:'Unsupported sample (rejected)',mime:'application/octet-stream',invalid:'This sample type is not allowed. Use PDF, CSV or TXT in this demo.'}
  };
  const drafts = new Map(), uploads = new Map(), errors = new Map();
  const api = { saveScenario:'ok', uploadScenario:'ok', dataMode:'normal' };
  const actor = () => !S.me ? '' : S.me.role + ':' + (S.me.role === 'client' ? S.me.clientId : S.me.accountantId);
  const key = scope => actor() + '|' + scope;
  const draft = scope => { const k=key(scope); if(!drafts.has(k)) drafts.set(k,{}); return drafts.get(k); };
  const accessible = e => !!(e && S.me && (S.me.role === 'client' ? e.clientId === S.me.clientId : e.accountantId === S.me.accountantId));
  const ownWork = () => work.filter(accessible);
  const find = id => { const e=work.find(x=>x.id===id); return accessible(e) ? e : null; };
  const owns = e => accessible(e) && S.me.role === 'accountant';
  const editable = e => owns(e) && e.phase === 'active';
  const event = (e,text) => e.updates.unshift({id:nid('event'),name:S.me.name,at:new Date().toISOString(),text});
  const error = (scope,text) => { errors.set(key(scope),text); render(); announce(text); };
  const errorHTML = scope => errors.has(key(scope)) ? '<p class="work-error" role="alert">'+esc(errors.get(key(scope)))+'</p>' : '';
  const canWrite = scope => {
    if (!S.online) { error(scope,'Connection lost. Your changes are kept. Retry when you are online; nothing is queued.'); return false; }
    if (api.saveScenario === 'fail') { error(scope,'The update could not be saved. Your changes are kept; try again.'); return false; }
    errors.delete(key(scope)); return true;
  };
  const unavailable = () => stateHTML('lock','This workspace is not available','It may not exist, or it belongs to another client and accountant.','<a class="btn" href="#/workspace">Back to my workspace</a>');
  const status = e => e.phase === 'requested' ? 'Awaiting acceptance' : e.phase === 'declined' ? 'Declined' : !e.tasks.length ? 'Not started' : e.tasks.every(t=>t.status===states[3]) ? states[3] : e.tasks.some(t=>t.status===states[2]) ? states[2] : e.tasks.some(t=>t.status!==states[0]) ? states[1] : states[0];
  const progress = e => ({done:e.tasks.filter(t=>t.status===states[3]).length,total:e.tasks.length});
  const badge = e => '<span class="work-status '+(status(e)===states[3]?'done':'')+'">'+esc(status(e))+'</span>';
  const meter = e => { const p=progress(e); return '<div class="work-progress"><progress max="'+(p.total||1)+'" value="'+p.done+'" aria-label="Completed service tasks"></progress><span>'+p.done+' of '+p.total+' tasks complete'+(p.total?' · '+Math.round(p.done/p.total*100)+'%':' · checklist not set')+'</span></div>'; };
  const field = (scope,name,label,value='',extra='') => '<label class="work-field">'+esc(label)+'<input data-bind="work-field" data-scope="'+esc(scope)+'" data-field="'+name+'" data-fkey="'+esc(scope+'-'+name)+'" value="'+esc(value)+'" '+extra+'></label>';
  const summaryCard = (title,value,detail) => '<div class="work-stat"><span>'+title+'</span><strong>'+value+'</strong><small>'+detail+'</small></div>';
  const heading = (title,sub) => '<div class="work-intro"><span class="eyebrow">Your private workspace</span><h2>'+esc(title)+'</h2><p>'+esc(sub)+'</p></div>';
  const workCard = e => '<li class="work-service"><div class="work-row"><h3><a href="#/work/'+e.id+'">'+esc(e.service)+'</a></h3>'+badge(e)+'</div>'+meter(e)+'<div class="work-row small"><span class="muted">'+e.files.length+' service document'+(e.files.length===1?'':'s')+'</span><a href="#/work/'+e.id+'">Open service '+icon('chevron',14)+'</a></div></li>';
  const chatLink = (clientId,accId) => { const c=CONVS.find(c=>c.clientId===clientId&&c.accountantId===accId); return c ? '<a class="btn sm" href="#/thread/'+c.id+'">'+icon('chat',16)+'Open chat</a>' : S.me.role==='client'&&getListed(accId) ? '<a class="btn sm" href="#/compose/'+accId+'">Message accountant</a>' : ''; };

  function listHTML() {
    const all=api.dataMode==='empty'?[]:ownWork(), isAcc=S.me.role==='accountant';
    if (isAcc) {
      const ids=[...new Set(all.map(e=>e.clientId))];
      return heading('A clear view of your client work.','Keep services, next steps and shared documents together. Client details stay private.')+
        '<div class="work-stats">'+summaryCard('Clients',ids.length,'With requests or service work')+summaryCard('In progress',all.filter(e=>e.phase==='active'&&status(e)!==states[3]).length,'Services to move forward')+summaryCard('New requests',all.filter(e=>e.phase==='requested').length,'Waiting for your response')+'</div>'+
        (ids.length?'<ul class="work-people">'+ids.map(id=>{const es=all.filter(e=>e.clientId===id),name=clients[id]||'Client';return '<li><a class="work-person" href="#/clients/'+id+'"><span class="avatar '+tone(name)+'" aria-hidden="true">'+initials(name)+'</span><div><h3>'+esc(name)+'</h3><p>'+es.length+' service'+(es.length===1?'':'s')+' · '+es.filter(e=>e.phase==='requested').length+' awaiting acceptance</p><span>'+esc(es.map(e=>e.service).join(' · '))+'</span></div>'+icon('chevron',18)+'</a></li>';}).join('')+'</ul>':stateHTML('user','No clients yet','A client appears here after they request one of your services. Saving your profile alone does not share their details.',''));
    }
    const savedIds=api.dataMode==='empty'?[]:(saved[S.me.clientId]||[]), ids=[...new Set([...savedIds,...all.map(e=>e.accountantId)])];
    return heading('Your people. Your next steps.','Accountants you have added, with your service progress in one place.')+
      '<div class="work-stats">'+summaryCard('Added accountants',savedIds.length,'Your private saved list')+summaryCard('Active services',all.filter(e=>e.phase==='active'&&status(e)!==states[3]).length,'Shared with each accountant')+summaryCard('Completed',all.filter(e=>status(e)===states[3]).length,'All checklist tasks complete')+'</div>'+
      '<p class="muted small">Adding is private. A shared client relationship starts when an accountant accepts a service request (confirmed workflow).</p>'+
      (ids.length?'<div class="work-accountants">'+ids.map(id=>{const p=getListed(id),es=all.filter(e=>e.accountantId===id);return '<section class="work-panel"><div class="work-row"><div><h3>'+(p?'<a href="#/profile/'+id+'">'+esc(p.name)+'</a>':'Accountant no longer listed')+'</h3><p class="muted small">'+(p?esc(p.title):'Existing service work stays private; account-deletion policy remains open.')+'</p></div>'+(savedIds.includes(id)?'<button class="btn ghost sm" data-action="ws-unsave" data-id="'+id+'">Remove saved<span class="sr-only"> accountant '+esc(p?p.name:'')+'</span></button>':'<span class="work-status">Service history</span>')+'</div>'+
        (es.length?'<ul class="work-services">'+es.map(workCard).join('')+'</ul>':'<p class="work-quiet">No services requested yet. You can browse their profile or request a service.</p>')+
        '<div class="actions">'+(p?'<a class="btn primary sm" href="#/request/'+id+'">Request a service</a>':'')+chatLink(S.me.clientId,id)+'</div></section>';}).join('')+'</div>':stateHTML('user','No accountants added yet','Explore the directory and add an accountant from their profile.','<a class="btn primary" href="#/directory">Find an accountant</a>'))+errorHTML('list');
  }
  function clientHTML(id) {
    if (S.me.role!=='accountant') return unavailable();
    const es=ownWork().filter(e=>e.clientId===id); if(!es.length)return unavailable();
    const p=PROFILES.find(p=>p.id===S.me.accountantId), d=draft('client:'+id), active=es.some(e=>e.phase==='active');
    return heading(clients[id]||'Client','Services shared with you. Only this client and you can see the work below.')+
      '<div class="actions">'+chatLink(id,S.me.accountantId)+'</div><ul class="work-services">'+es.map(workCard).join('')+'</ul>'+
      (active&&p?'<section class="work-panel"><h3>Add a service for this client</h3><p class="muted small">Proposed: accountants may add work for an existing accepted client relationship. No prices, payment or booking.</p>'+serviceSelect('client:'+id,p,d.service)+'<button class="btn primary" data-action="ws-add-service" data-client="'+id+'">Add service</button>'+errorHTML('client:'+id)+'</section>':'');
  }
  function serviceSelect(scope,p,value) {
    return '<label class="work-field">Service<select data-bind="work-field" data-scope="'+esc(scope)+'" data-field="service" data-fkey="'+esc(scope+'-service')+'"><option value="">Choose a service</option>'+p.services.map(s=>'<option'+(value===s.name?' selected':'')+'>'+esc(s.name)+'</option>').join('')+'</select></label>';
  }
  function requestHTML(id) {
    const p=getListed(id); if(!p||S.me.role!=='client')return unavailable();
    const scope='request:'+id,d=draft(scope);
    return heading('Request a service',p.name+' will review your request before work starts.')+'<section class="work-panel work-form">'+serviceSelect(scope,p,d.service)+
      '<p class="note"><span>This shares your account name and requested service with '+esc(p.name)+'. It does not make a payment, booking or guarantee of service. The shared client relationship starts after accountant acceptance.</span></p><button class="btn primary" data-action="ws-request" data-id="'+id+'">Send service request</button>'+errorHTML(scope)+'</section>';
  }
  function checklistHTML(e) {
    const d=draft(e.id), edit=editable(e);
    return '<section class="work-panel"><div class="work-row"><h3>Service checklist</h3><span class="muted small">'+(edit?'Changes are shared when saved':'Updated by your accountant')+'</span></div>'+meter(e)+
      (e.tasks.length?'<ol class="work-tasks">'+e.tasks.map(t=>{const value=d['task:'+t.id]||t.status;return '<li><div class="work-task-name">'+(edit?'<input type="checkbox" data-bind="work-check" data-id="'+e.id+'" data-task="'+t.id+'" data-fkey="check-'+t.id+'" aria-label="Mark '+esc(t.name)+' completed"'+(value===states[3]?' checked':'')+'>':'<span aria-hidden="true">'+(t.status===states[3]?'✓':'○')+'</span>')+'<span>'+esc(t.name)+'</span></div>'+(edit?'<label class="work-task-status"><span class="sr-only">Status for '+esc(t.name)+'</span><select data-bind="work-task" data-id="'+e.id+'" data-task="'+t.id+'" data-fkey="status-'+t.id+'">'+states.map(s=>'<option'+(s===value?' selected':'')+'>'+s+'</option>').join('')+'</select></label>':'<span class="work-status">'+esc(t.status)+'</span>')+'</li>';}).join('')+'</ol>':'<p class="work-quiet">No tasks yet. Progress starts after the accountant adds a checklist.</p>')+
      (edit?'<div class="actions"><button class="btn primary" data-action="ws-save-tasks" data-id="'+e.id+'">Save checklist changes</button></div>'+field(e.id,'newTask','New checklist task',d.newTask||'','maxlength="120"')+'<button class="btn" data-action="ws-add-task" data-id="'+e.id+'">Add task</button>':'')+errorHTML(e.id)+'</section>';
  }
  function filesHTML(e) {
    const d=draft(e.id),u=uploads.get(key(e.id)),busy=u&&u.state==='uploading',sample=d.sample||'receipts';
    return '<section class="work-panel"><h3>Service documents</h3><p class="muted small">Private to '+esc(clients[e.clientId]||'the client')+' and '+esc(PROFILES.find(p=>p.id===e.accountantId)?.name||'the accountant')+'. These are service documents, not chat attachments.</p>'+
      '<ul class="work-files">'+e.files.map(f=>'<li><div>'+icon('file',20)+'<div><strong>'+esc(f.name)+'</strong><small>Available · '+esc(f.by)+' · '+esc(fmtShort(f.at))+' · '+(f.taskIndex===null?'Whole service':esc(e.tasks[f.taskIndex]?.name||'Service task'))+'</small></div></div><div class="actions"><button class="btn sm" data-action="ws-download" data-id="'+e.id+'" data-file="'+f.id+'">Download<span class="sr-only"> '+esc(f.name)+'</span></button>'+(f.owner===actor()?'<button class="btn ghost sm" data-action="ws-remove-file" data-id="'+e.id+'" data-file="'+f.id+'">Remove<span class="sr-only"> '+esc(f.name)+'</span></button>':'')+'</div></li>').join('')+'</ul>'+
      (!e.files.length?'<p class="work-quiet">No documents attached yet.</p>':'')+
      (e.phase==='active'?'<div class="work-upload"><p class="work-demo">Prototype upload: choose a generated sample only. No actual document is read or sent. Demo policy: PDF, CSV, TXT; 5 MiB maximum (proposed).</p><label class="work-field">Sample document<select data-bind="work-field" data-scope="'+e.id+'" data-field="sample" data-fkey="sample-'+e.id+'"'+(busy?' disabled':'')+'>'+Object.entries(samples).map(([k,s])=>'<option value="'+k+'"'+(k===sample?' selected':'')+'>'+esc(s.label)+'</option>').join('')+'</select></label>'+
        '<label class="work-field">Attach to<select data-bind="work-field" data-scope="'+e.id+'" data-field="fileTask" data-fkey="file-task-'+e.id+'"'+(busy?' disabled':'')+'><option value="">Whole service</option>'+e.tasks.map((t,i)=>'<option value="'+i+'"'+(d.fileTask===String(i)?' selected':'')+'>'+esc(t.name)+'</option>').join('')+'</select></label>'+
        (busy?'<div role="status"><progress max="100" value="'+u.percent+'" aria-label="Sample upload progress"></progress><p>Uploading sample… '+u.percent+'%</p></div>':'')+
        (u&&u.state==='failed'?'<p class="work-error" role="alert">'+esc(u.error)+'</p>':'')+
        '<button class="btn primary" data-action="ws-upload" data-id="'+e.id+'"'+(busy?' disabled':'')+'>'+(busy?'Uploading…':u&&u.state==='failed'?'Retry sample upload':'Upload sample document')+'</button></div>':'<p class="muted small">Documents can be added after acceptance.</p>')+'</section>';
  }
  function detailHTML(id) {
    const e=find(id);if(!e)return unavailable();
    const p=PROFILES.find(p=>p.id===e.accountantId),d=draft(id),isAcc=owns(e);
    return heading(e.service,(clients[e.clientId]||'Client')+' · '+(p?.name||'Accountant'))+
      '<div class="work-row work-summary">'+badge(e)+chatLink(e.clientId,e.accountantId)+'</div>'+
      (e.phase==='requested'?'<section class="work-panel work-request"><h3>Service request</h3><p>Work has not started. '+(isAcc?'Review this request, then accept or decline.':'Your accountant has not accepted this request yet.')+'</p>'+(isAcc?'<div class="actions"><button class="btn primary" data-action="ws-accept" data-id="'+id+'">Accept request</button><button class="btn" data-action="ws-decline" data-id="'+id+'">Decline request</button></div>':'')+errorHTML(id)+'</section>':'')+
      '<div class="work-grid"><div>'+checklistHTML(e)+filesHTML(e)+'</div><aside class="work-panel work-history"><h3>Progress updates</h3><p class="muted small">Shared notes and activity, newest first. Times are shown in Asia/Manila.</p>'+
      (editable(e)?'<label class="work-field">Shared progress note<textarea rows="4" maxlength="1000" data-bind="work-field" data-scope="'+id+'" data-field="note" data-fkey="note-'+id+'">'+esc(d.note||'')+'</textarea></label><button class="btn" data-action="ws-post-note" data-id="'+id+'">Post update</button>':'')+
      '<ol class="work-timeline">'+e.updates.map(h=>'<li><strong>'+esc(h.name)+'</strong><time datetime="'+esc(h.at)+'">'+esc(fmtShort(h.at))+(fmtShort(h.at).includes(':')?'':' · '+esc(fmtTime(h.at)))+'</time><p>'+esc(h.text)+'</p></li>').join('')+'</ol></aside></div>';
  }
  api.page = r => {
    const title=r.name==='workspace'?(S.me?.role==='accountant'?'Clients':'My accountants'):r.name==='request'?'Request a service':r.name==='client'?'Client services':'Service progress';
    if(!S.me)return {title,back:'#/directory',body:stateHTML('lock','Sign in to see your workspace','Client lists, service progress and documents are private.',signinBtn(location.hash))};
    if(api.dataMode==='error')return {title,back:'#/workspace',body:stateHTML('alert','Could not load your workspace','Try again. Your work has not been changed.','<button class="btn primary" data-action="ws-retry-load">Try again</button>')};
    if(api.dataMode==='loading')return {title,body:'<div class="state" aria-busy="true">Loading your workspace…</div>'};
    return {title,back:r.name==='workspace'?null:'#/workspace',tabs:r.name==='workspace',body:r.name==='workspace'?listHTML():r.name==='client'?clientHTML(r.id):r.name==='request'?requestHTML(r.accId):detailHTML(r.id)};
  };
  api.profileAction = id => {
    if(!S.me||S.me.role!=='client')return '';
    const added=(saved[S.me.clientId]||[]).includes(id);
    return '<div class="work-profile-actions">'+(added?'<a class="btn block" href="#/workspace">Added · My accountants</a>':'<button class="btn block" data-action="ws-save" data-id="'+id+'">Add accountant</button>')+'<a class="btn ghost block" href="#/request/'+id+'">Request a service</a><p class="cta-hint">Adding is private. Request a service when you are ready.</p></div>';
  };
  function newWork(clientId,accId,service,phase) {
    const existing=work.find(e=>e.clientId===clientId&&e.accountantId===accId&&e.service===service&&e.phase!=='declined');
    if(existing){go('#/work/'+existing.id);toast('This service already has a workspace.');return;}
    const e={id:nid('engagement'),clientId,accountantId:accId,service,phase,tasks:phase==='active'?tasks(['Agree the service requirements','Prepare the work','Review and hand over']):[],files:[],updates:[]};
    clients[clientId] ||= S.me.role==='client'?S.me.name:'Client';
    work.push(e);event(e,phase==='active'?'Added '+service+' to this client relationship.':'Requested '+service+'. Awaiting accountant acceptance.');go('#/work/'+e.id);toast(phase==='active'?'Service added.':'Service request sent.');
  }
  function syntheticBytes(sample) {
    if(sample==='receipts') {
      const stream='BT /F1 14 Tf 40 180 Td (Kuwenta synthetic service document) Tj 0 -24 Td (Demo only - no client records.) Tj ET';
      const objects=['<< /Type /Catalog /Pages 2 0 R >>','<< /Type /Pages /Kids [3 0 R] /Count 1 >>','<< /Type /Page /Parent 2 0 R /MediaBox [0 0 400 240] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>','<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>','<< /Length '+stream.length+' >>\nstream\n'+stream+'\nendstream'];
      let pdf='%PDF-1.4\n',offsets=[0];objects.forEach((o,i)=>{offsets.push(pdf.length);pdf+=(i+1)+' 0 obj\n'+o+'\nendobj\n';});
      const start=pdf.length;pdf+='xref\n0 6\n0000000000 65535 f \n'+offsets.slice(1).map(n=>String(n).padStart(10,'0')+' 00000 n \n').join('')+'trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n'+start+'\n%%EOF\n';return pdf;
    }
    return sample==='sales'?'record,note\nDEMO-001,Synthetic sample only - no client records\n':'Kuwenta synthetic service document\nDemo only. No client records.\n';
  }
  api.actions = {
    'ws-retry-load':()=>{api.dataMode='normal';render();},
    'ws-save':el=>{if(!S.me||S.me.role!=='client'||!getListed(el.dataset.id))return;if(!canWrite('list'))return;const ids=saved[S.me.clientId]||=[];if(!ids.includes(el.dataset.id))ids.push(el.dataset.id);toast('Accountant added to your private list.');},
    'ws-unsave':el=>{if(!S.me||S.me.role!=='client'||!canWrite('list'))return;saved[S.me.clientId]=(saved[S.me.clientId]||[]).filter(id=>id!==el.dataset.id);toast('Removed from your saved list. Existing service work remains accessible.');},
    'ws-request':el=>{const p=getListed(el.dataset.id);if(!p||S.me?.role!=='client')return;const scope='request:'+p.id,service=draft(scope).service;if(!p.services.some(s=>s.name===service)){error(scope,'Choose a service offered by this accountant.');return;}if(!canWrite(scope))return;const ids=saved[S.me.clientId]||=[];if(!ids.includes(p.id))ids.push(p.id);newWork(S.me.clientId,p.id,service,'requested');},
    'ws-add-service':el=>{if(S.me?.role!=='accountant'||!ownWork().some(e=>e.clientId===el.dataset.client&&e.phase==='active'))return;const p=PROFILES.find(p=>p.id===S.me.accountantId),scope='client:'+el.dataset.client,service=draft(scope).service;if(!p?.services.some(s=>s.name===service)){error(scope,'Choose one of your offered services.');return;}if(canWrite(scope))newWork(el.dataset.client,S.me.accountantId,service,'active');},
    'ws-accept':el=>{const e=find(el.dataset.id);if(!owns(e)||e.phase!=='requested'||!canWrite(e.id))return;e.phase='active';e.tasks=tasks(['Agree the service requirements','Prepare the work','Review and hand over']);event(e,'Accepted the service request and created a starting checklist.');toast('Request accepted.');},
    'ws-decline':el=>{const e=find(el.dataset.id);if(!owns(e)||e.phase!=='requested'||!canWrite(e.id))return;e.phase='declined';event(e,'Declined the service request.');toast('Request declined.');},
    'ws-save-tasks':el=>{const e=find(el.dataset.id);if(!editable(e)||!canWrite(e.id))return;const d=draft(e.id),changed=[];e.tasks.forEach(t=>{const v=d['task:'+t.id];if(states.includes(v)&&v!==t.status){t.status=v;changed.push(t.name+': '+v);}delete d['task:'+t.id];});if(changed.length)event(e,changed.join('; '));toast(changed.length?'Checklist progress saved.':'No checklist changes to save.');},
    'ws-add-task':el=>{const e=find(el.dataset.id);if(!editable(e))return;const d=draft(e.id),name=(d.newTask||'').trim();if(!name||name.length>120){error(e.id,'Enter a task name of 1 to 120 characters.');return;}if(!canWrite(e.id))return;e.tasks.push({id:nid('task'),name,status:states[0]});d.newTask='';event(e,'Added checklist task: '+name);toast('Task added.');},
    'ws-post-note':el=>{const e=find(el.dataset.id);if(!editable(e))return;const d=draft(e.id),text=(d.note||'').trim();if(!text||text.length>1000){error(e.id,'Write a shared update of 1 to 1,000 characters.');return;}if(!canWrite(e.id))return;event(e,text);d.note='';toast('Progress update shared.');},
    'ws-upload':el=>{
      const e=find(el.dataset.id);if(!e||e.phase!=='active')return;const k=key(e.id),previous=uploads.get(k);if(previous?.state==='uploading')return;
      const d=draft(e.id),sample=d.sample||'receipts',s=samples[sample];
      const fail=message=>{uploads.set(k,{state:'failed',error:message});render();announce(message);};
      if(!s||s.invalid){fail(s?.invalid||'Choose a sample document.');return;}
      if(!S.online){fail('Connection lost. Your sample selection is kept. Retry when online; nothing is queued.');return;}
      const index=d.fileTask===''||d.fileTask===undefined?null:Number(d.fileTask);
      if(index!==null&&(!Number.isInteger(index)||!e.tasks[index])){fail('Choose a valid service task.');return;}
      const owner=actor(),by=S.me.name,u={state:'uploading',percent:15};uploads.set(k,u);render();announce('Sample upload started.');
      setTimeout(()=>{if(uploads.get(k)!==u||u.state!=='uploading')return;u.percent=60;if(actor()===owner)render();},250);
      setTimeout(()=>{
        if(uploads.get(k)!==u||u.state!=='uploading')return;
        if(actor()!==owner||!find(e.id)){u.state='failed';u.error='Upload interrupted by an account change. Retry when signed in.';return;}
        if(!S.online||api.uploadScenario==='fail'){fail(!S.online?'Connection lost during upload. Selection kept; retry when online.':'Sample upload failed. Your selection is kept; try again.');return;}
        e.files.push({id:nid('file'),name:s.name,sample,taskIndex:index,owner,by,at:new Date().toISOString()});uploads.delete(k);event(e,'Attached '+s.name+(index===null?' to the service.':' to '+e.tasks[index].name+'.'));toast('Sample document attached.');announce('Sample document attached.');
      },800);
    },
    'ws-download':el=>{const e=find(el.dataset.id),f=e?.files.find(f=>f.id===el.dataset.file);if(!f||!S.online){if(e)toast('Connect to download the sample document.');return;}const sample=samples[f.sample],blob=new Blob([syntheticBytes(f.sample)],{type:sample.mime}),url=URL.createObjectURL(blob),a=document.createElement('a');a.href=url;a.download=f.name;a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);},
    'ws-remove-file':el=>{const e=find(el.dataset.id),f=e?.files.find(f=>f.id===el.dataset.file);if(!f||f.owner!==actor())return;S.dialog={id:'remove-service-file',title:'Remove '+f.name+'?',body:'This removes the sample from the shared service workspace. Production retention and deletion policies remain open.',actions:[{label:'Cancel',action:'dlg-keep'},{label:'Remove document',action:'ws-confirm-remove',kind:'danger'}]};api.removeTarget={id:e.id,file:f.id,owner:actor()};render();},
    'ws-confirm-remove':()=>{const target=api.removeTarget;S.dialog=null;api.removeTarget=null;const e=target&&find(target.id),f=e?.files.find(f=>f.id===target.file);if(!f||target.owner!==actor()||f.owner!==actor()){render();return;}if(!canWrite(e.id))return;e.files=e.files.filter(x=>x.id!==f.id);event(e,'Removed '+f.name+' from this service.');toast('Sample document removed.');}
  };
  api.input = el => {
    if(el.dataset.bind!=='work-field')return false;
    draft(el.dataset.scope)[el.dataset.field]=el.value;return true;
  };
  api.change = el => {
    const b=el.dataset.bind;
    if(b==='work-field')return api.input(el);
    if(b!=='work-task'&&b!=='work-check')return false;
    const e=find(el.dataset.id);if(!editable(e)||!e.tasks.some(t=>t.id===el.dataset.task))return true;
    draft(e.id)['task:'+el.dataset.task]=b==='work-check'?(el.checked?states[3]:states[0]):el.value;render();return true;
  };
  api.actorChanged = () => {
    for(const u of uploads.values())if(u.state==='uploading'){u.state='failed';u.error='Upload interrupted by an account change. Retry when signed in.';}
    api.removeTarget=null;
    if(S.dialog?.id==='remove-service-file')S.dialog=null;
  };
  return api;
};
