(() => {
  "use strict";
  const STORAGE_KEY = "shopping_cart";
  let PACKAGES = {};

  const money = (value) => `${new Intl.NumberFormat("ar-EG").format(value)} ج.م`;

  const readCart = () => {
    try {
      const raw = JSON.parse(localStorage.getItem(STORAGE_KEY) || "[]");
      if (!Array.isArray(raw)) return [];
      return raw
        .filter(
          (x) =>
            x &&
            Number.isSafeInteger(x.quantity) &&
            x.quantity > 0 &&
            x.quantity <= 99
        )
        .map((x) => ({ id: String(x.id), quantity: x.quantity }));
    } catch {
      return [];
    }
  };

  let cart = readCart();

  const save = () => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(cart));
    } catch {}
  };

  const count = () => cart.reduce((sum, item) => sum + item.quantity, 0);

  const total = () =>
    cart.reduce((sum, item) => {
      const pkg = PACKAGES[item.id];
      return sum + (pkg ? pkg.price * item.quantity : 0);
    }, 0);

  const badge = document.getElementById("cartBadge");
  const drawer = document.getElementById("cartDrawer");
  const overlay = document.getElementById("cartOverlay");
  const itemsEl = document.getElementById("cartItems");
  const totalEl = document.getElementById("cartTotal");

  function render() {
    if (badge) {
      const c = count();
      badge.textContent = c;
      badge.style.display = c === 0 ? "none" : "inline-flex";
    }

    if (!itemsEl) return;

    if (!cart.length) {
      itemsEl.innerHTML = `
        <div class="cart-empty text-center py-4">
          <i class="bi bi-bag-x fs-1 text-muted"></i>
          <h3 class="h5 mt-2">سلتك فارغة</h3>
          <p class="text-muted small">لم تقم بإضافة أي باقة حتى الآن.</p>
          <a class="btn btn-soft" href="/#packages" id="emptyCartShop">تصفح العبوات</a>
        </div>`;
      const footer = document.getElementById("cartFooter");
      if (footer) footer.style.display = "none";
    } else {
      const footer = document.getElementById("cartFooter");
      if (footer) footer.style.display = "block";

      itemsEl.innerHTML = cart
        .map((item) => {
          const p = PACKAGES[item.id] || {
            name: "عبوة أعشاب",
            duration: "",
            price: 0,
            image: "",
          };
          return `
            <article class="cart-item d-flex justify-content-between align-items-center py-3 border-bottom" data-id="${item.id}">
              <div class="d-flex align-items-center gap-3 flex-grow-1 min-w-0">
                ${p.image ? `<img src="${p.image}" alt="${p.name}" class="cart-item-img" loading="lazy">` : `<div class="cart-item-icon"><i class="bi bi-box-seam"></i></div>`}
                <div class="cart-item-info">
                  <h3 class="h6 mb-1 text-truncate">${p.name}</h3>
                  <small class="text-muted d-block">${p.duration ? p.duration + " · " : ""}${money(p.price)}</small>
                  <div class="quantity-control d-flex align-items-center gap-2 mt-2">
                    <button type="button" class="btn btn-sm btn-outline-secondary" data-action="decrease" aria-label="تقليل الكمية">−</button>
                    <span class="px-2 fw-bold">${item.quantity}</span>
                    <button type="button" class="btn btn-sm btn-outline-secondary" data-action="increase" aria-label="زيادة الكمية">+</button>
                  </div>
                </div>
              </div>
              <div class="cart-item-side text-end flex-shrink-0 ps-2">
                <strong class="d-block text-success mb-2">${money(p.price * item.quantity)}</strong>
                <button class="btn btn-sm text-danger remove-item p-0" type="button" data-action="remove">
                  <i class="bi bi-trash"></i> <span>حذف</span>
                </button>
              </div>
            </article>`;
        })
        .join("");
    }

    if (totalEl) totalEl.textContent = money(total());
  }

  function openCart() {
    if (!drawer) return;
    drawer.classList.add("is-open");
    drawer.setAttribute("aria-hidden", "false");
    if (overlay) overlay.hidden = false;
    document.body.classList.add("cart-locked");
    document.getElementById("cartToggle")?.setAttribute("aria-expanded", "true");
  }

  function closeCart() {
    if (!drawer) return;
    drawer.classList.remove("is-open");
    drawer.setAttribute("aria-hidden", "true");
    if (overlay) overlay.hidden = true;
    document.body.classList.remove("cart-locked");
    document.getElementById("cartToggle")?.setAttribute("aria-expanded", "false");
  }

  function persistAndRender() {
    save();
    render();
  }

  function addToCart(id, name, price, duration, image) {
    const strId = String(id);
    if (!PACKAGES[strId] && name) {
      PACKAGES[strId] = {
        id: strId,
        name: name,
        price: parseFloat(price) || 0,
        duration: duration || "",
        image: image || "",
      };
    } else if (PACKAGES[strId] && image && !PACKAGES[strId].image) {
      PACKAGES[strId].image = image;
    }

    const found = cart.find((x) => String(x.id) === strId);
    if (found) {
      found.quantity = Math.min(99, found.quantity + 1);
    } else {
      cart.push({ id: strId, quantity: 1 });
    }

    persistAndRender();
    openCart();
  }

  // Event Listeners
  document.addEventListener("click", (event) => {
    const add = event.target.closest(".order-package[data-package-id]");
    if (add) {
      event.preventDefault();
      const id = add.dataset.packageId;
      const name = add.dataset.package;
      const price = add.dataset.price;
      const duration = add.dataset.duration;
      const image = add.dataset.image;
      addToCart(id, name, price, duration, image);
      return;
    }

    if (event.target.closest("#cartToggle")) {
      openCart();
      return;
    }

    if (event.target.closest("#cartClose") || event.target === overlay) {
      closeCart();
      return;
    }

    const action = event.target.closest("[data-action]");
    if (action && action.closest(".cart-item")) {
      const id = String(action.closest(".cart-item").dataset.id);
      const item = cart.find((x) => String(x.id) === id);
      if (!item) return;

      if (action.dataset.action === "increase")
        item.quantity = Math.min(99, item.quantity + 1);
      if (action.dataset.action === "decrease")
        item.quantity = Math.max(1, item.quantity - 1);
      if (action.dataset.action === "remove")
        cart = cart.filter((x) => String(x.id) !== id);

      persistAndRender();
    }
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && drawer?.classList.contains("is-open"))
      closeCart();
  });

  window.addEventListener("storage", (event) => {
    if (event.key === STORAGE_KEY) {
      cart = readCart();
      render();
    }
  });

  // Load catalog on page init
  async function initCatalog() {
    try {
      const res = await fetch("/api/catalog");
      if (res.ok) {
        PACKAGES = await res.json();
      }
    } catch {}

    // Also harvest any package cards present in the DOM
    document.querySelectorAll(".order-package[data-package-id]").forEach((btn) => {
      const id = String(btn.dataset.packageId);
      if (!PACKAGES[id]) {
        PACKAGES[id] = {
          id: id,
          name: btn.dataset.package || "عبوة أعشاب",
          price: parseFloat(btn.dataset.price) || 0,
          duration: btn.dataset.duration || "",
          image: btn.dataset.image || "",
        };
      } else if (btn.dataset.image && !PACKAGES[id].image) {
        PACKAGES[id].image = btn.dataset.image;
      }
    });

    render();
  }

  initCatalog();
})();
