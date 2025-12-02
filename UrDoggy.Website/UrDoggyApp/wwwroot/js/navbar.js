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

    // THÊM PHẦN DEBUG
    document.addEventListener("click", (e) => {
        const dropdownButton = e.target.closest("[data-dropdown-button]");
        if (dropdownButton) {
            console.log('Dropdown button clicked');
            console.log('Current active state:',
                dropdownButton.closest("[data-dropdown]").classList.contains('active'));
        }
    });

    // THÊM RESET Z-INDEX
    document.querySelectorAll('.dropdown-menu').forEach(menu => {
        menu.style.zIndex = '9999';
    });
});
