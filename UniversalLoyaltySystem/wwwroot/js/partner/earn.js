
"use strict"; import { api } from '../common.js';
const rnd=()=> (crypto.randomUUID ? crypto.randomUUID() : (Date.now().toString(36)+Math.random().toString(36).slice(2)));
const earn=document.getElementById('earnForm'), red=document.getElementById('redeemForm');
earn.addEventListener('submit',async e=>{e.preventDefault(); const fd=new FormData(earn); const payload=Object.fromEntries(fd.entries()); payload.OrderAmount=parseFloat(payload.OrderAmount); await api('/api/partner/earn',{method:'POST',headers:{'Idempotency-Key':rnd()},body:JSON.stringify(payload)}); earn.reset();});
red.addEventListener('submit',async e=>{e.preventDefault(); const fd=new FormData(red); const payload=Object.fromEntries(fd.entries()); payload.Points=parseInt(payload.Points,10); await api('/api/partner/redeem',{method:'POST',headers:{'Idempotency-Key':rnd()},body:JSON.stringify(payload)}); red.reset();});
