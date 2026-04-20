(function () {
    const usdInput    = document.getElementById("usdAmount");
    const rateInput   = document.getElementById("exchangeRate");
    const zarDisplay  = document.getElementById("zarDisplay");
    const zarHidden   = document.getElementById("zarHidden");
    const requestForm = usdInput?.closest("form");
    const submitBtn   = requestForm?.querySelector("button[type='submit']");
    const defaultText = "Save Service Request";

    if (!usdInput || !rateInput || !zarDisplay || !zarHidden) {
        return;
    }

    // ── ZAR preview ────────────────────────────────────────────────────────
    const updatePreview = () => {
        const usd  = Number.parseFloat((usdInput.value  || "0").replace(",", "."));
        const rate = Number.parseFloat((rateInput.value || "0").replace(",", "."));

        if (Number.isNaN(usd) || Number.isNaN(rate) || rate <= 0) {
            zarDisplay.value = "";
            zarHidden.value  = "0";
            return;
        }

        const zar = (usd * rate).toFixed(2);
        zarDisplay.value = zar;   // display input (shown to user)
        zarHidden.value  = zar;   // hidden input  (posted to server)
    };

    usdInput.addEventListener("input", updatePreview);
    updatePreview();

    // ── Button state reset on every page show ──────────────────────────────
    // Covers: initial load, server returning validation errors, bfcache.
    const resetButton = () => {
        if (submitBtn) {
            submitBtn.disabled    = false;
            submitBtn.textContent = defaultText;
        }
    };

    resetButton();
    window.addEventListener("pageshow", resetButton);

    // ── Submit: only disable AFTER validation passes ───────────────────────
    // jQuery Unobtrusive Validation attaches to the form's submit event BEFORE
    // our listener, so by the time our callback runs the form is already proven
    // valid (it would have been cancelled otherwise).
    requestForm?.addEventListener("submit", (e) => {
        // If jQuery validation is present, double-check validity synchronously.
        if (typeof $ !== "undefined" && $.validator) {
            const $form = $(requestForm);
            if ($form.validate && !$form.valid()) {
                // validation failed — do not disable button
                return;
            }
        }

        if (submitBtn && !submitBtn.disabled) {
            submitBtn.disabled    = true;
            submitBtn.textContent = "Saving\u2026";
        }
    });
})();
