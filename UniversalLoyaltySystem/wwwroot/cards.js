// ====== Config ======
const API = window.location.origin + "/api";
const toastEl = document.getElementById("toast");
const overlayEl = document.getElementById("overlay");

function showToast(msg, type = "info") {
    toastEl.textContent = msg;
    toastEl.className = "toast show";
    if (type === "ok") toastEl.style.borderColor = "#22c55e";
    if (type === "err") toastEl.style.borderColor = "#ef4444";
    setTimeout(() => (toastEl.className = "toast hidden"), 2200);
}
function showOverlay(on = true) {
    overlayEl.classList.toggle("hidden", !on);
}

function getToken() {
    const t = localStorage.getItem("accessToken");
    if (!t) {
        window.location.href = "/login";
        return null;
    }
    return t;
}
function authHeaders() {
    return { Authorization: `Bearer ${getToken()}` };
}

// ====== UI Elements ======
const createBtn = document.getElementById("createCardBtn");
const logoutBtn = document.getElementById("logoutBtn");
const listEl = document.getElementById("cardsList");
const emptyEl = document.getElementById("cardsEmpty");

// ====== Helpers ======
function compat(obj, name) {
    // поддержка PascalCase и camelCase одновременно
    const camel = name[0].toLowerCase() + name.slice(1);
    return obj[camel] ?? obj[name];
}

function renderCards(cards) {
    if (!cards || cards.length === 0) {
        emptyEl.classList.remove("hidden");
        listEl.innerHTML = "";
        return;
    }
    emptyEl.classList.add("hidden");
    listEl.innerHTML = cards
        .map((c) => {
            const number = compat(c, "CardNumber");
            const balance = compat(c, "Balance") ?? 0;

            return `
        <div class="card-row">
          <div>
            <div class="card-id">${number}</div>
            <div class="badge">Баланс: ${balance} баллов</div>
          </div>
          <div class="quick">
            <input type="number" min="0" step="0.01" placeholder="Сумма ₽" data-card="${number}" class="earn-input" />
            <button class="btn success earn-btn" data-card="${number}">+</button>
            <input type="number" min="1" step="1" placeholder="Списать" data-card="${number}" class="redeem-input" />
            <button class="btn danger redeem-btn" data-card="${number}">−</button>
            <button class="btn ghost copy-btn" data-card="${number}">Копир.</button>
          </div>
        </div>`;
        })
        .join("");

    // навесим обработчики
    listEl.querySelectorAll(".earn-btn").forEach((b) =>
        b.addEventListener("click", async (e) => {
            const num = e.currentTarget.dataset.card;
            const input = listEl.querySelector(`.earn-input[data-card="${num}"]`);
            const amount = parseFloat(input.value);
            if (isNaN(amount) || amount <= 0) return showToast("Укажи сумму покупки", "err");
            await earn(num, amount);
        })
    );
    listEl.querySelectorAll(".redeem-btn").forEach((b) =>
        b.addEventListener("click", async (e) => {
            const num = e.currentTarget.dataset.card;
            const input = listEl.querySelector(`.redeem-input[data-card="${num}"]`);
            const pts = parseInt(input.value, 10);
            if (isNaN(pts) || pts <= 0) return showToast("Сколько списать?", "err");
            await redeem(num, pts);
        })
    );
    listEl.querySelectorAll(".copy-btn").forEach((b) =>
        b.addEventListener("click", async (e) => {
            const num = e.currentTarget.dataset.card;
            await navigator.clipboard.writeText(num);
            showToast("Номер карты скопирован", "ok");
        })
    );
}


async function createCard() {
    try {
        showOverlay(true);
        const r = await fetch(`${API}/cards`, { method: "POST", headers: authHeaders() });
        if (!r.ok) throw new Error("Ошибка создания");
        const d = await r.json();
        showToast(`Карта создана: ${compat(d, "CardNumber")}`, "ok");
        fetchCards();
    } catch (e) {
        console.error(e);
        showToast("Не удалось создать карту", "err");
    } finally {
        showOverlay(false);
    }
}

async function fetchCards() {
    try {
        showOverlay(true);
        const r = await fetch(`${API}/cards/my`, { headers: authHeaders() });
        console.log("[API] GET /api/cards/my:", r.status);
        if (r.status === 401) return toLogin();
        const data = await r.json();
        console.log("[API] cards:", data);
        renderCards(data);
    } catch (e) {
        console.error("[API] cards error:", e);
        showToast("Не удалось загрузить карты", "err");
    } finally {
        showOverlay(false);
    }
}

async function earn(cardNumber, purchaseAmount) {
    try {
        showOverlay(true);
        console.log("[API] POST /api/cards/earn", { cardNumber, purchaseAmount });
        const r = await fetch(`${API}/cards/earn`, {
            method: "POST",
            headers: { ...authHeaders(), "Content-Type": "application/json" },
            body: JSON.stringify({ cardNumber, purchaseAmount })
        });

        const raw = await r.text();
        console.log("[API] earn status:", r.status, "raw:", raw);

        if (!r.ok) throw new Error(raw || "Ошибка начисления");

        // поддержка camelCase / PascalCase
        const d = raw ? JSON.parse(raw) : {};
        const added = d.added ?? d.Added ?? 0;

        if (added === 0) {
            showToast("Начислено 0 (сумма < 10 ₽?)", "err");
        } else {
            showToast(`Начислено: ${added} балл(ов)`, "ok");
        }
        await fetchCards(); // ← обязательно ждём обновление списка
    } catch (e) {
        console.error("[API] earn error:", e);
        showToast(e.message || "Не удалось начислить", "err");
    } finally {
        showOverlay(false);
    }
}

async function redeem(cardNumber, points) {
    try {
        showOverlay(true);
        const r = await fetch(`${API}/cards/redeem`, {
            method: "POST",
            headers: { ...authHeaders(), "Content-Type": "application/json" },
            body: JSON.stringify({ cardNumber, points })
        });
        if (!r.ok) {
            const t = await r.text();
            throw new Error(t || "Ошибка списания");
        }
        const d = await r.json();
        showToast(`Списано: ${compat(d, "Redeemed")} балл(ов)`, "ok");
        fetchCards();
    } catch (e) {
        console.error(e);
        showToast(e.message.includes("Недостаточно") ? "Недостаточно баллов" : "Не удалось списать", "err");
    } finally {
        showOverlay(false);
    }
}





function parseMoney(input) {
    if (typeof input === "string") input = input.replace(",", ".").trim();
    const v = Number.parseFloat(input);
    return Number.isFinite(v) ? v : NaN;
}

(function initForms() {
    const earnForm = document.getElementById("earnForm");
    const redeemForm = document.getElementById("redeemForm");

    // Начисление из правой формы
    earnForm?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const numEl = document.getElementById("earnCard");
        const amtEl = document.getElementById("earnAmount");
        const cardNumber = numEl?.value?.trim();
        const amount = parseMoney(amtEl?.value ?? "");
        if (!cardNumber) return showToast("Укажи номер карты", "err");
        if (isNaN(amount) || amount <= 0) return showToast("Сумма должна быть > 0", "err");

        await earn(cardNumber, amount);      // ← уже существующая функция
        amtEl.value = "";                    // очистим поле
    });

    // Списание из правой формы
    redeemForm?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const numEl = document.getElementById("redeemCard");
        const ptsEl = document.getElementById("redeemPoints");
        const cardNumber = numEl?.value?.trim();
        const points = parseInt(ptsEl?.value ?? "", 10);
        if (!cardNumber) return showToast("Укажи номер карты", "err");
        if (!Number.isInteger(points) || points <= 0) return showToast("Баллы должны быть > 0", "err");

        await redeem(cardNumber, points);    // ← уже существующая функция
        ptsEl.value = "";
    });
})();




function toLogin() {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    window.location.href = "/login";
}

// ====== Events ======
createBtn?.addEventListener("click", createCard);
logoutBtn?.addEventListener("click", toLogin);

// ====== Init ======
if (!getToken()) {
    // redirect in getToken
} else {
    fetchCards();
}


