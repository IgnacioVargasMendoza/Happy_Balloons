(function () {
    var THEME_KEY = 'htb-theme';
    var FONT_KEY = 'htb-font';

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem(THEME_KEY, theme);
        var btn = document.getElementById('btn-theme-toggle');
        if (btn) {
            var icon = btn.querySelector('i');
            if (icon) {
                icon.className = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-fill';
            }
            btn.title = theme === 'dark' ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro';
        }
    }

    function applyFont(size) {
        document.documentElement.classList.remove('htb-font-sm', 'htb-font-md', 'htb-font-lg');
        document.documentElement.classList.add('htb-font-' + size);
        localStorage.setItem(FONT_KEY, size);
        ['sm', 'md', 'lg'].forEach(function (s) {
            var btn = document.getElementById('btn-font-' + s);
            if (btn) {
                btn.classList.toggle('active', s === size);
            }
        });
    }

    window.htbToggleTheme = function () {
        var current = document.documentElement.getAttribute('data-bs-theme') || 'light';
        applyTheme(current === 'dark' ? 'light' : 'dark');
    };

    window.htbSetFont = function (size) {
        applyFont(size);
    };

    document.addEventListener('DOMContentLoaded', function () {
        var theme = localStorage.getItem(THEME_KEY) || 'light';
        var font = localStorage.getItem(FONT_KEY) || 'md';

        var themeBtn = document.getElementById('btn-theme-toggle');
        if (themeBtn) {
            var icon = themeBtn.querySelector('i');
            if (icon) {
                icon.className = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-fill';
            }
            themeBtn.title = theme === 'dark' ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro';
        }

        ['sm', 'md', 'lg'].forEach(function (s) {
            var btn = document.getElementById('btn-font-' + s);
            if (btn) {
                btn.classList.toggle('active', s === font);
            }
        });
    });
}());
