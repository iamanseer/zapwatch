// Light/dark theme toggle. The actual theme is set pre-paint by the inline
// script in _Layout.cshtml (avoids a flash of the wrong theme) - this just
// handles the button click and keeps the saved preference in sync.
(function () {
    var THEME_COLORS = { light: "#ffffff", dark: "#0b0d12" };

    function updateToggleLabel(theme) {
        var button = document.getElementById("zwThemeToggle");
        if (!button) {
            return;
        }
        button.setAttribute("aria-label", theme === "dark" ? "Switch to light theme" : "Switch to dark theme");
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute("data-theme", theme);
        document.documentElement.setAttribute("data-bs-theme", theme);
        localStorage.setItem("zw-theme", theme);
        updateToggleLabel(theme);

        var meta = document.querySelector('meta[name="theme-color"]');
        if (meta) {
            meta.setAttribute("content", THEME_COLORS[theme] || THEME_COLORS.light);
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        updateToggleLabel(document.documentElement.getAttribute("data-theme") || "light");
    });

    document.addEventListener("click", function (event) {
        var button = event.target.closest("#zwThemeToggle");
        if (!button) {
            return;
        }
        var current = document.documentElement.getAttribute("data-theme") || "light";
        applyTheme(current === "dark" ? "light" : "dark");
    });
})();

// Copy-to-clipboard for ping URLs on the automations dashboard.
document.addEventListener("click", function (event) {
    var button = event.target.closest("[data-copy-target]");
    if (!button) {
        return;
    }

    var wrapper = button.closest("[data-ping-url]");
    var value = wrapper ? wrapper.getAttribute("data-ping-url") : null;
    if (!value) {
        return;
    }

    navigator.clipboard.writeText(value).then(function () {
        var original = button.textContent;
        button.textContent = "Copied";
        setTimeout(function () {
            button.textContent = original;
        }, 1200);
    });
});
