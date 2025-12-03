/* ============================================================
   GLOBAL FUNCTIONS FOR THEME COLOR PICKER
   ============================================================ */

function setColor(input) {
    const lightness = getLightnessFromHex(input.value);
    document.body.setAttribute(
        "style",
        `--color-bg: ${input.value};
         --color-text: ${lightness > 60 ? 'black' : 'white'}`
    );
}

function getLightnessFromHex(hex) {
    hex = hex.replace(/^#/, "");

    const r = parseInt(hex.substr(0, 2), 16);
    const g = parseInt(hex.substr(2, 2), 16);
    const b = parseInt(hex.substr(4, 2), 16);

    const brightness = (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255;

    return +(brightness * 100).toFixed(2);
}


/* ============================================================
   PAGE INTERACTIONS
   ============================================================ */

document.addEventListener("DOMContentLoaded", () => {

    /* ============================================================
       1. NAVBAR RIGHT
       ============================================================ */

    /* SEARCH */

    const searchContainer = document.querySelector(".search-container");
    const searchToggle = document.querySelector(".search-toggle");
    const searchInput = document.querySelector(".search-input");

    if (!searchContainer || !searchToggle || !searchInput) return;

    searchToggle.addEventListener("click", () => {
        searchContainer.classList.toggle("active");

        if (searchContainer.classList.contains("active")) {
            searchInput.focus();
        } else {
            searchInput.blur();
        }
    });

    // collapse on click-outside
    document.addEventListener("click", (e) => {
        if (!searchContainer.contains(e.target)) {
            searchContainer.classList.remove("active");
            searchInput.blur();
        }
    });

    /* DROPDOWN */

    document.addEventListener("click", (e) => {
        const dropdownButton = e.target.closest("[data-dropdown-button]");
        const dropdown = e.target.closest("[data-dropdown]");

        if (!dropdownButton && dropdown) return;

        let currentDropdown;

        if (dropdownButton) {
            currentDropdown = dropdownButton.closest("[data-dropdown]");
            currentDropdown.classList.toggle("active");
        }

        document.querySelectorAll("[data-dropdown].active").forEach(d => {
            if (d !== currentDropdown) d.classList.remove("active");
        });
    });

    document.addEventListener("click", e => {
        const dropdowns = document.querySelectorAll('[data-dropdown]');
        dropdowns.forEach(dropdown => {
            if (!dropdown.contains(e.target)) {
                dropdown.classList.remove('active');
            }
        });
    });

    // DEBUG
    document.addEventListener("click", (e) => {
        const dropdownButton = e.target.closest("[data-dropdown-button]");
        if (dropdownButton) {
            console.log('Dropdown button clicked');
            console.log('Current active state:',
                dropdownButton.closest("[data-dropdown]").classList.contains('active'));
        }
    });

    // FORCE HIGH Z-INDEX
    document.querySelectorAll('.dropdown-menu').forEach(menu => {
        menu.style.zIndex = '9999';
    });

});
