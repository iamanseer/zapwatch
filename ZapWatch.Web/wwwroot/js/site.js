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
