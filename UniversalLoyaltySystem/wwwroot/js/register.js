
"use strict"; import { api } from './common.js';
let role='Client'; const bC=document.getElementById('btnClient'), bP=document.getElementById('btnPartner'), org=document.getElementById('orgRow');
function setRole(r){role=r;bC.classList.toggle('active',r==='Client');bP.classList.toggle('active',r==='Partner');org.style.display=(r==='Partner')?'':'none';}
bC.onclick=()=>setRole('Client'); bP.onclick=()=>setRole('Partner'); setRole('Client');
document.getElementById('regForm').addEventListener('submit',async e=>{
  e.preventDefault(); const fd=new FormData(e.currentTarget);
  const payload={email:fd.get('email'),password:fd.get('password'),fullName:fd.get('fullName')||null};
  if(role==='Client') await api('/api/auth/register/client',{method:'POST',body:JSON.stringify(payload)});
  else { const orgName=fd.get('orgName')||'Мой магазин'; await api('/api/auth/register/partner?orgName='+encodeURIComponent(orgName),{method:'POST',body:JSON.stringify(payload)}); }
  const res=await api('/api/auth/login',{method:'POST',body:JSON.stringify({email:payload.email,password:payload.password})});
  localStorage.setItem('token',res.accessToken); location.href='/html/profile.html';
});
