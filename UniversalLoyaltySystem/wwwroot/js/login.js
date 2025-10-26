
"use strict"; import { api } from './common.js';
document.getElementById('loginForm').addEventListener('submit',async e=>{
  e.preventDefault(); const fd=new FormData(e.currentTarget);
  const res=await api('/api/auth/login',{method:'POST',body:JSON.stringify({email:fd.get('email'),password:fd.get('password')})});
  localStorage.setItem('token',res.accessToken); location.href='/html/profile.html';
});
