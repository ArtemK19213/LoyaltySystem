
export const token=localStorage.getItem('token')||null;
const ensureToast=(()=>{let el=document.getElementById('toast');if(!el){el=document.createElement('div');el.id='toast';document.body.appendChild(el);}return el;})();
export function toast(msg, ok=true){const d=document.createElement('div');d.className='toast'+(ok?'':' err');d.textContent=msg;ensureToast.appendChild(d);setTimeout(()=>d.remove(),3600);}
export async function api(url,opt={}){
  const h=Object.assign({},opt.headers||{});
  if(!h['Content-Type']&&!(opt.body instanceof FormData))h['Content-Type']='application/json';
  if(localStorage.getItem('token'))h['Authorization']='Bearer '+localStorage.getItem('token');
  const r=await fetch(url,Object.assign({},opt,{headers:h}));
  if(r.status===401){localStorage.removeItem('token');if(!location.pathname.endsWith('/login.html')&&!location.pathname.endsWith('/register.html'))location.href='/html/login.html';throw new Error('401');}
  const ct=r.headers.get('content-type')||'';const data=ct.includes('application/json')?await r.json():await r.text();
  if(!r.ok){toast((data&&(data.error||data.message))||'Ошибка',false);throw new Error('api');}
  return data;
}
export const fmt=(dt)=>new Intl.DateTimeFormat('ru-RU',{dateStyle:'medium',timeStyle:'short'}).format(new Date(dt));
export function ulid(){const t=Date.now().toString(36);let r='';for(let i=0;i<16;i++)r+=((Math.random()*36)|0).toString(36);return (t+r).slice(0,26);}
