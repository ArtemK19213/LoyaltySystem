
"use strict"; import { api, fmt } from './common.js';
(async()=>{
  const me=await api('/api/auth/me');
  document.getElementById('heroName').textContent=me.fullName?.trim()||'Профиль';
  document.getElementById('heroEmail').textContent=me.email||'—';
  document.getElementById('heroRole').textContent=(me.role==='Admin'?'Админ':me.role==='Partner'?'Партнёр':'Клиент');
  document.getElementById('outEmail').textContent=me.email||'—';
  document.getElementById('outFullName').textContent=me.fullName?.trim()||'—';
  document.getElementById('outCreated').textContent=fmt(me.createdAt);
  const row=document.getElementById('rowOrg');
  if(me.organizationId){document.getElementById('outOrg').textContent=(me.organizationName&&me.organizationName.trim())?me.organizationName.trim():('Орг. #'+me.organizationId);row.style.display='';} else row.style.display='none';
})();
