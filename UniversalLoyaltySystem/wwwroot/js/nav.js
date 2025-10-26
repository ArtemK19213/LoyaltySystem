
import {api,token} from './common.js';
(async function(){
  const h=document.createElement('header');h.className='topbar';
  h.innerHTML=`<div class="container"><div class="bar">
    <a class="brand" href="/html/profile.html">Universal Loyalty</a>
    <nav class="nav" id="tabs"></nav>
    <div class="actions" id="navActions"></div>
  </div></div>`;
  document.body.prepend(h);
  let me=null;if(token){try{me=await api('/api/auth/me');}catch{}}
  const tabs=[];
  if(me&&(me.role==='Partner'||me.role==='Admin')){
    tabs.push({href:'/html/partner/programs.html',text:'Программы'},
              {href:'/html/partner/cards.html',text:'Карты'},
              {href:'/html/partner/earn.html',text:'Начислить/Списать'},
              {href:'/html/partner/campaigns.html',text:'Акции'},
              {href:'/html/partner/ledger.html',text:'Транзакции'});
  }else{
    tabs.push({href:'/html/client/cards.html',text:'Мои карты'},
              {href:'/html/client/history.html',text:'История'},
              {href:'/html/client/inbox.html',text:'Сообщения'});
  }
  tabs.push({href:'/html/profile.html',text:'Профиль'});
  const host=h.querySelector('#tabs');
  const active=(p)=>location.pathname.endsWith(p.replace('/html',''));
  host.innerHTML=tabs.map(t=>`<a class="tab ${active(t.href)?'active':''}" href="${t.href}">${t.text}</a>`).join('');
  const act=h.querySelector('#navActions');
  if(me){act.innerHTML=`<button class="btn ghost" id="logout">Выйти</button>`;act.querySelector('#logout').onclick=()=>{localStorage.removeItem('token');location.href='/html/login.html';};}
  else{act.innerHTML=`<a class="tab" href="/html/login.html">Войти</a><a class="tab" href="/html/register.html">Регистрация</a>`;}
})();
