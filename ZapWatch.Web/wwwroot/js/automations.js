// Live dashboard updates for the Automations list via SignalR.
// One hub connection per page load, scoped server-side to this user's own automations
// (see AutomationStatusHub) - every event received here belongs to this account only.
(function () {
    var rows = document.querySelectorAll("[data-automation-id]");
    if (rows.length === 0) {
        return;
    }

    function describeStatus(status, lastSeenAtIso) {
        if (status === "Paused") {
            return { cssClass: "zw-status--paused", label: "Paused" };
        }
        if (status === "Overdue") {
            return { cssClass: "zw-status--overdue", label: "Overdue" };
        }
        if (!lastSeenAtIso) {
            return { cssClass: "zw-status--waiting", label: "Awaiting first ping" };
        }
        return { cssClass: "zw-status--ok", label: "Live" };
    }

    function formatLastSeen(lastSeenAtIso) {
        if (!lastSeenAtIso) {
            return "Never";
        }
        var d = new Date(lastSeenAtIso);
        var pad = function (n) { return String(n).padStart(2, "0"); };
        return d.getFullYear() + "-" + pad(d.getMonth() + 1) + "-" + pad(d.getDate()) +
            " " + pad(d.getHours()) + ":" + pad(d.getMinutes());
    }

    function applyUpdate(automationId, status, lastSeenAtIso) {
        var row = document.querySelector('[data-automation-id="' + automationId + '"]');
        if (!row) {
            return;
        }

        var statusEl = row.querySelector('[data-role="status"]');
        var labelEl = row.querySelector('[data-role="status-label"]');
        var lastSeenEl = row.querySelector('[data-role="last-seen"]');
        var described = describeStatus(status, lastSeenAtIso);

        if (statusEl) {
            statusEl.className = "zw-status " + described.cssClass;
        }
        if (labelEl) {
            labelEl.textContent = described.label;
        }
        if (lastSeenEl) {
            lastSeenEl.textContent = formatLastSeen(lastSeenAtIso);
        }
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/automation-status")
        .withAutomaticReconnect()
        .build();

    connection.on("AutomationStatusChanged", function (automationId, status, lastSeenAtIso) {
        applyUpdate(automationId, status, lastSeenAtIso);
    });

    connection.start().catch(function (err) {
        console.error("AutomationStatusHub connection failed:", err);
    });
})();
