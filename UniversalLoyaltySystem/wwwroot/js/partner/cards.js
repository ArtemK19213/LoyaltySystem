import { api, toast } from '/js/common.js';

const programSel = document.getElementById('program');
const contactInp = document.getElementById('contact');
const issueBtn = document.getElementById('issue');
const issuedDiv = document.getElementById('issued');
const qInp = document.getElementById('q');
const findBtn = document.getElementById('find');
const resDiv = document.getElementById('res');

async function loadPrograms() {
    const list = await api('/api/partner/programs');
    programSel.innerHTML = list.length
        ? list.map(p => `<option value="${p.id}">${p.name} — ${p.programType}</option>`).join('')
        : '<option disabled>Сначала создайте программу</option>';
}
issueBtn.onclick = async () => {
    const programId = Number(programSel.value);
    if (!programId) { toast.err('Выберите программу'); return; }
    const body = { programId, customerContact: contactInp.value?.trim() || null };
    const card = await api('/api/partner/cards', { method: 'POST', body: JSON.stringify(body) });
    toast('Карта выпущена');
    issuedDiv.innerHTML = `<div class="card-visual"><div class="num">№ ${card.number}</div></div>
    <div class="small">QR-токен: ${card.qrToken}</div>`;
    contactInp.value = '';
};

findBtn.onclick = async () => {
    const q = qInp.value.trim();
    if (!q) { resDiv.textContent = 'Введите запрос.'; return; }
    const items = await api(`/api/partner/cards/search?q=${encodeURIComponent(q)}`);
    if (!items.length) { resDiv.textContent = 'Ничего не найдено.'; return; }
    resDiv.innerHTML = items.map(c => `<div class="surface" style="padding:12px">
    <b>№ ${c.number}</b> — ${c.status} • ${c.programName}
    <div class="small">${c.customerEmail ?? 'Без владельца'}</div>
  </div>`).join('');
};

loadPrograms();
