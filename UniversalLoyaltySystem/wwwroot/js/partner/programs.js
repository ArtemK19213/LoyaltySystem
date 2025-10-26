
"use strict"; import { api } from '../common.js';
const form=document.getElementById('createForm'), sel=document.getElementById('prgType'), bonus=document.getElementById('bonusFields'), disc=document.getElementById('discountFields'), list=document.getElementById('programsList');
function toggle(){const b=sel.value==='Bonus'; bonus.style.display=b?'grid':'none'; disc.style.display=b?'none':'grid';} sel.onchange=toggle; toggle();
async function load(){list.textContent='Загрузка…'; const items=await api('/api/partner/programs'); if(!items.length){list.textContent='Пока нет программ'; return;}
  const wrap=document.createElement('div'); wrap.className='grid'; items.forEach(p=>{const r=document.createElement('div'); r.className='kv'; r.innerHTML=`<span>${p.programType==='Bonus'?'Бонусная':'Дисконтная'} — <b>${p.name}</b></span><strong>${new Intl.DateTimeFormat('ru-RU',{dateStyle:'medium',timeStyle:'short'}).format(new Date(p.createdAt))}</strong>`; wrap.appendChild(r);}); list.innerHTML=''; list.appendChild(wrap); }
form.addEventListener('submit',async e=>{e.preventDefault(); const fd=new FormData(form); const payload={}; for(const [k,v] of fd.entries()){ if(v!=='') payload[k]=v; } payload.ProgramType=sel.value; await api('/api/partner/programs',{method:'POST',body:JSON.stringify(payload)}); form.reset(); toggle(); load();});
load();
