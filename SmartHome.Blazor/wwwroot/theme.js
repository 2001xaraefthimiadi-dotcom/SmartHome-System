window.themeManager = {
    applyTheme: function () {
        const savedTheme = localStorage.getItem("darkMode");

        const isDark = savedTheme === null
            ? false
            : savedTheme === "true";

        if (isDark) {
            document.documentElement.classList.add("dark-mode");
            document.body.classList.add("dark-mode");
        } else {
            document.documentElement.classList.remove("dark-mode");
            document.body.classList.remove("dark-mode");
        }

        return isDark;
    },

    toggleDarkMode: function () {
        const isCurrentlyDark =
            document.documentElement.classList.contains("dark-mode");

        localStorage.setItem("darkMode", isCurrentlyDark ? "false" : "true");

        return window.themeManager.applyTheme();
    },

    loadTheme: function () {
        return window.themeManager.applyTheme();
    }
};

document.addEventListener("DOMContentLoaded", function () {
    window.themeManager.applyTheme();
});

document.addEventListener("enhancedload", function () {
    window.themeManager.applyTheme();
});