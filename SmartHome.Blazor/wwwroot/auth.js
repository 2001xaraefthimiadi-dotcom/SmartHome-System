window.authStorage = {
    setToken: function (token) {
        localStorage.setItem("smartHomeToken", token);
    },

    getToken: function () {
        return localStorage.getItem("smartHomeToken");
    },

    removeToken: function () {
        localStorage.removeItem("smartHomeToken");
    }
};