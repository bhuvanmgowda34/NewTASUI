/* =========================================================
   1. SIDEBAR MENU TOGGLE (Parent Menu Open/Close)
   ========================================================= */
document.addEventListener("DOMContentLoaded", function () {

    const menuItems = document.querySelectorAll(".menu-item.has-children");


    menuItems.forEach(item => {
        item.querySelector(".menu-title").addEventListener("click", function () {

            const isOpen = item.classList.contains("is-open");

            // Close other menus ONLY
            document.querySelectorAll(".menu-item")
                .forEach(i => {
                    if (i !== item) i.classList.remove("is-open");
                });

            // Toggle current menu
            if (!isOpen) {
                item.classList.add("is-open");
            } else {
                item.classList.remove("is-open");
            }

        });
    });

});


/* =========================================================
   2. ALARM PANEL TOGGLE (Top Right Switch)
   ========================================================= */
const alarmToggle = document.getElementById("alarmToggle");
const alarmPanel = document.getElementById("alarmPanel");
const alarmPin = document.getElementById("alarmPin");

let isPinned = false;


document.addEventListener("DOMContentLoaded", function () {

    const savedState = localStorage.getItem("alarmToggle");

    if (savedState === "true") {
        alarmToggle.checked = true;
        // transition:none is already on the element via HTML attribute.
        // Just add open — no animation fires.
        alarmPanel.classList.add("open");
        // Re-enable transition after 50ms for user-triggered toggles
        window.setTimeout(function () {
            alarmPanel.style.transition = "";
        }, 50);
    } else {
        alarmToggle.checked = false;
        // Re-enable transition immediately — panel is already closed
        alarmPanel.style.transition = "";
    }

});

/* EXISTING CODE */
alarmToggle.addEventListener("change", function () {

    const isChecked = alarmToggle.checked;

    localStorage.setItem("alarmToggle", isChecked);

    if (isChecked) {
        alarmPanel.classList.add("open");
    } else {
        alarmPanel.classList.remove("open");
    }

});


/* =========================================================
   3. TOPBAR TICKER ANIMATION (Scrolling Text)
   ========================================================= */
//document.addEventListener("DOMContentLoaded", function () {

//    const ticker = document.getElementById("ticker");
//    if (!ticker) return;

//    const speed = 50;
//    const firstText = ticker.children[0];
//    let startTime = null;

//    function animate(time) {
//        if (!startTime) startTime = time;
//        const elapsed = (time - startTime) / 1000;
//        const distance = (elapsed * speed) % firstText.offsetWidth;
//        ticker.style.transform = `translateX(${-distance}px)`;
//        requestAnimationFrame(animate);
//    }

//    requestAnimationFrame(animate);
//});


/* =========================================================
   4. BAY TAB INDICATOR (Red Sliding Line)
   ========================================================= */
document.addEventListener("DOMContentLoaded", function () {

    const tabs = document.querySelectorAll(".bay-tab");
    const indicator = document.querySelector(".bay-indicator");

    if (!tabs.length) return;

    function moveIndicator(tab) {
        const rect = tab.getBoundingClientRect();
        const parentRect = tab.parentElement.getBoundingClientRect();
        indicator.style.width = rect.width + "px";
        indicator.style.transform = `translateX(${rect.left - parentRect.left}px)`;
    }

    tabs.forEach(tab => {
        tab.addEventListener("click", () => {
            tabs.forEach(t => t.classList.remove("active"));
            tab.classList.add("active");
            moveIndicator(tab);
        });
    });

    const activeTab = document.querySelector(".bay-tab.active");
    if (activeTab) moveIndicator(activeTab);
});


/* =========================================================
   5. CUSTOM DROPDOWN (Glass Select)
   ========================================================= */
document.addEventListener("DOMContentLoaded", function () {

    document.querySelectorAll(".custom-select").forEach(select => {
        const trigger = select.querySelector(".select-trigger");
        const options = select.querySelectorAll(".option");
        const text = trigger.querySelector("span");

        trigger.addEventListener("click", (e) => {
            e.stopPropagation();

            // CLOSE all others
            document.querySelectorAll(".custom-select.open").forEach(s => {
                if (s !== select) s.classList.remove("open");
            });

            // TOGGLE ONLY if exact trigger clicked
            if (e.target.closest(".select-trigger")) {
                select.classList.toggle("open");
            }
        });

        select.addEventListener("click", function (e) {

            // 🔥 IMPORTANT: ignore checkbox clicks
            if (e.target.closest("input[type='checkbox']")) return;

            const option = e.target.closest(".option");

            if (option) {
                const select = option.closest(".custom-select");

                if (!select.classList.contains("open")) return;

                e.stopPropagation();

                const text = select.querySelector(".select-trigger span");

                text.textContent = option.textContent;
                select.setAttribute("data-selected-id", option.dataset.id || option.textContent);

                select.classList.remove("open");
            }
        });
    });

    document.addEventListener("click", () => {
        document.querySelectorAll(".custom-select.open")
            .forEach(s => s.classList.remove("open"));
    });

});


/* =========================================================
   6. BAY TAB CONTENT SWITCH (Form vs Material)
   ========================================================= */
document.addEventListener("DOMContentLoaded", function () {

    const tabs = document.querySelectorAll(".bay-tab");
    const bayContent = document.querySelector(".bay-content");
    const materialContent = document.querySelector(".bay-material-container");

    if (!bayContent || !materialContent) return;

    tabs.forEach((tab, index) => {
        tab.addEventListener("click", () => {
            tabs.forEach(t => t.classList.remove("active"));
            tab.classList.add("active");

            if (index === 0) {
                bayContent.classList.remove("hidden");
                materialContent.classList.add("hidden");
            } else {
                bayContent.classList.add("hidden");
                materialContent.classList.remove("hidden");
            }
        });
    });

});


/* =========================================================
   7. SUBMENU CLICK + PAGE ACTIVE STATE
   =========================================================
   Philosophy:
   - NO localStorage. Active state is driven by the URL only.
   - On every page load, JS reads window.location and marks
     the matching submenu item active + opens its parent.
   - Clicking an active item closes the page and dropdown.
   - Clicking a different item navigates normally.
   ========================================================= */
document.addEventListener("DOMContentLoaded", function () {

    /* ── Step A: Mark active item based on current URL ── */
    const currentPath = window.location.pathname.toLowerCase();

    document.querySelectorAll(".submenu-item").forEach(item => {

        // Get the href — works for both <a> tags and plain divs
        const href = (item.getAttribute("href") || "").toLowerCase();

        // Match if the current URL contains this item's href
        // e.g. /masters/bay matches href="/Masters/Bay"
        /*const isActive = href && currentPath.includes(href);*/
        const isActive = href && currentPath === href;

        if (isActive) {
            item.classList.add("active");

            // Open parent dropdown — no transition during this restore
            const parent = item.closest(".menu-item");
            const submenu = parent && parent.querySelector(".submenu");

            if (parent && submenu) {
                submenu.style.transition = "none";
                parent.classList.add("is-open");
                // Re-arm transition after browser has painted the open state
                window.setTimeout(function () {
                    submenu.style.transition = "";
                }, 50);
            }
        }
    });

    /* ── Step B: Handle clicks ── */
    document.querySelectorAll(".submenu-item").forEach(item => {
        item.addEventListener("click", function (e) {

            const isActive = this.classList.contains("active");

            if (isActive) {
                // Already on this page — close everything and go home
                e.preventDefault();

                document.querySelectorAll(".submenu-item")
                    .forEach(i => i.classList.remove("active"));

                const parent = this.closest(".menu-item");
                if (parent) parent.classList.remove("is-open");

                const wrapper = document.querySelector(".main-content .content-wrapper");
                if (wrapper) wrapper.innerHTML = "";
            }
            // If not active — let the <a> href navigate normally, no extra code needed
        });
    });

});




/* =========================================================
   9. REPORT TABLE LOADER
   ========================================================= */
    function loadReport() {

        const body = document.getElementById("reportBody");
        const empty = document.getElementById("reportEmpty");

        const fromDate = document.getElementById("fromDate").value;
        const toDate = document.getElementById("toDate").value;
        const bayId = document.getElementById("baySelect").getAttribute("data-selected-id");

        fetch('/Reports/GetReportData', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                fromDate: fromDate,
                toDate: toDate,
                bay: bayId
            })
        })
            .then(res => res.json())
            .then(data => {

                const selectedBay = document.querySelector("#baySelect .select-trigger span").innerText;

                console.log("Selected:", selectedBay);
                console.log("Row Bay:", data[0]?.bayNo);

                //  FILTER HERE
                if (selectedBay !== "All Bays") {

                    const selectedNumber = selectedBay.split(" ")[1].trim();

                    data = data.filter(row => {
                        return String(row.bayNo).includes(selectedNumber);
                    });
                }
                let html = "";

                if (!data || data.length === 0) {
                    body.innerHTML = "";
                    empty.style.display = "block";
                    return;
                }

                data.forEach(row => {
                    html += `
            <div class="report-row">
                <div>${row.dateAndTime}</div>
                <div>${row.gantry}</div>
                <div>${row.bayNo}</div>
                <div>${row.fanNumber}</div>
                <div>${row.truckRegNo}</div>
                <div>${row.preset}</div>
                <div>${row.baseQty}</div>
                <div>${row.blendQty}</div>
                <div>${row.add1Qty}</div>
                <div>${row.add2Qty}</div>
                <div>${row.qtyFilled}</div>
                <div>${row.topUpQty}</div>
                <div>${row.decantQty}</div>
                <div>${row.effectiveQty}</div>
            </div>`;
                });

                body.innerHTML = html;
                empty.style.display = "none";

            })
            .catch(err => console.error(err));
    }


        fetch('/Reports/GetBays')
            .then(res => res.json())
            .then(data => {

                const dropdown = document.getElementById("bayDropdown");

                let html = `<div class="option">All Bays</div>`;

                data.forEach(bay => {
                    html += `<div class="option" data-id="${bay.id}">${bay.name}</div>`;
                });

                dropdown.innerHTML = html;

                //  ADD THIS PART (IMPORTANT)
                dropdown.querySelectorAll(".option").forEach(option => {
                    option.addEventListener("click", function (e) {

                        e.stopPropagation();

                        document.querySelector("#baySelect .select-trigger span").textContent = this.textContent;

                        // STORE SELECTED BAY ID
                        document.getElementById("baySelect").setAttribute("data-selected-id", this.dataset.id);

                        document.getElementById("baySelect").classList.remove("open");
                    });
                });

            });
// LICENSE API

document.addEventListener('DOMContentLoaded', () => {
    loadLicenseStatus();

    setInterval(loadLicenseStatus, 1000);
});
function loadLicenseStatus() {
    const el = document.getElementById('licenseStatus');

    if (!el) return;

    fetch('/Home/GetLicenseStatus')
        .then(res => res.json())
        .then(data => {

            el.innerHTML = 'xxx';

            if (data.success && data.lmStatus) {
                el.classList.remove('warn');
                el.classList.add('ok');
            } else {
                el.classList.remove('ok');
                el.classList.add('warn');
            }
        })
        .catch(() => {
            el.innerHTML = 'xxx';
            el.classList.remove('ok');
            el.classList.add('warn');
        });
}

// SERVER STATUS API
document.addEventListener('DOMContentLoaded', () => {
    loadServerStatus();
    setInterval(loadServerStatus, 1000); 
});
function loadServerStatus() {
    fetch('/Home/GetServerStatus')
        .then(res => res.json())
        .then(data => {

            const primaryEl = document.getElementById('primaryStatus');
            const secondaryEl = document.getElementById('secondaryStatus');

            if (!primaryEl || !secondaryEl) return;

            primaryEl.innerHTML = 'xxx';
            secondaryEl.innerHTML = 'xxx';

            if (!data.success) {
                primaryEl.className = 'status-value warn';
                secondaryEl.className = 'status-value warn';
                return;
            }

            primaryEl.className = 'status-value ' + data.primary;
            secondaryEl.className = 'status-value ' + data.secondary;
        })
        .catch(() => {
            const primaryEl = document.getElementById('primaryStatus');
            const secondaryEl = document.getElementById('secondaryStatus');

            if (!primaryEl || !secondaryEl) return;

            primaryEl.className = 'status-value warn';
            secondaryEl.className = 'status-value warn';
        });
}
// Top bar header API
document.addEventListener('DOMContentLoaded', () => {
    loadCompanyHeader();
    setInterval(loadCompanyHeader, 1000);
});
function loadCompanyHeader() {
    fetch('/Home/GetCompanyInfo')
        .then(res => res.json())
        .then(data => {
            const el = document.getElementById('companyHeader');
            if (!el) return;

            if (data.success) {
                el.innerText = data.text;
            } else {
                el.innerText = "—";
            }
        })
        .catch(() => {
            const el = document.getElementById('companyHeader');
            if (!el) return;
            el.innerText = "—";
        });
}