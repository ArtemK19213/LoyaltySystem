"use strict";

// auth guard
const token = localStorage.getItem('token');
if (!token) location.href = '/html/login.html';

// toast
const toastRoot = (() => {
  let el = document.getElementById('toast');
  if (!el) { el = document.createElement('div'); el.id = 'toast'; document.body.appendChild(el); }
  return el;
})();
function toast(msg, ok=true){
  const div = document.createElement('div');
  div.className = 'toast ' + (ok ? 'ok' : 'err');
  div.textContent = msg;
  toastRoot.appendChild(div);
  setTimeout(()=>div.remove(), 3500);
}

async function api(url, options={}){
  options.headers = Object.assign({}, options.headers || {}, { Authorization: 'Bearer ' + token, 'Content-Type':'application/json' });
  const r = await fetch(url, options);
  if (r.status === 401){ localStorage.removeItem('token'); location.href='/html/login.html'; throw new Error('401'); }
  const ct = r.headers.get('content-type') || '';
  const data = ct.includes('application/json') ? await r.json() : await r.text();
  if (!r.ok){ const msg = (data && data.error) ? data.error : (data && data.message) ? data.message : 'Ошибка'; toast(msg, false); throw new Error(msg); }
  return data;
}

function fmtDate(dtStr){
  try{ const d = new Date(dtStr); return new Intl.DateTimeFormat('ru-RU', { dateStyle:'medium', timeStyle:'short' }).format(d); } catch { return '' }
}
