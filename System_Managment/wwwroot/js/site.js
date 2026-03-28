(function (window, document) {
    "use strict";

    const app = window.App || {};
    let activeDialog = null;

    function applySwalDefaults() {
        if (!window.Swal || typeof window.Swal.fire !== "function" || window.Swal.__appDefaultsApplied) {
            return;
        }

        const originalFire = window.Swal.fire.bind(window.Swal);

        window.Swal.fire = function (options, ...rest) {
            if (typeof options === "string") {
                return originalFire(options, ...rest);
            }

            const defaults = {
                confirmButtonText: "OK",
                confirmButtonColor: "#2a7f40",
                cancelButtonColor: "#64748b",
                reverseButtons: true,
                heightAuto: false,
                customClass: {
                    popup: "rounded-3xl",
                    confirmButton: "px-5 py-3 rounded-2xl font-bold",
                    cancelButton: "px-5 py-3 rounded-2xl font-bold"
                },
                buttonsStyling: false
            };

            return originalFire(window.jQuery ? window.jQuery.extend(true, {}, defaults, options) : { ...defaults, ...options });
        };

        window.Swal.__appDefaultsApplied = true;
    }

    function closeAjaxDialog() {
        if (!activeDialog) {
            return;
        }

        activeDialog.remove();
        activeDialog = null;
        document.body.classList.remove("overflow-hidden");
    }

    function buildDialog(content) {
        closeAjaxDialog();

        const wrapper = document.createElement("div");
        wrapper.className = "app-ajax-dialog fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm overflow-y-auto";
        wrapper.innerHTML = `<div class="w-full max-w-2xl my-8">${content}</div>`;

        wrapper.addEventListener("click", function (event) {
            if (event.target === wrapper) {
                closeAjaxDialog();
            }
        });

        document.body.appendChild(wrapper);
        document.body.classList.add("overflow-hidden");
        activeDialog = wrapper;
        executeEmbeddedScripts(wrapper);
    }

    function executeEmbeddedScripts(container) {
        const scripts = Array.from(container.querySelectorAll("script"));

        scripts.forEach(function (script) {
            const executableScript = document.createElement("script");

            Array.from(script.attributes).forEach(function (attribute) {
                executableScript.setAttribute(attribute.name, attribute.value);
            });

            executableScript.text = script.textContent || "";
            script.parentNode.replaceChild(executableScript, script);
        });
    }

    function openAjaxDialog(url) {
        if (!window.jQuery) {
            window.location.href = url;
            return;
        }

        window.jQuery.ajax({
            url: url,
            type: "GET",
            cache: true,
            headers: { "X-Requested-With": "XMLHttpRequest" }
        }).done(function (html) {
            buildDialog(html);
        }).fail(function () {
            if (window.Swal) {
                window.Swal.fire({
                    icon: "error",
                    title: "Load Error",
                    text: "Unable to load the requested content."
                });
            }
        });
    }

    function showTempDataAlerts() {
        const holder = document.getElementById("app-tempdata-alerts");
        if (!holder || !window.Swal) {
            return;
        }

        const success = holder.dataset.success || holder.dataset.successMessage;
        const error = holder.dataset.error || holder.dataset.errorMessage;

        if (success) {
            window.Swal.fire({
                icon: "success",
                title: "Success",
                text: success
            });
        } else if (error) {
            window.Swal.fire({
                icon: "error",
                title: "Error",
                text: error
            });
        }
    }

    function wireGlobalEvents() {
        if (!window.jQuery) {
            return;
        }

        window.jQuery(document).on("click", ".openCreateDialog, .openEditDialog, .openDetailsDialog, [data-ajax-dialog]", function (event) {
            event.preventDefault();
            openAjaxDialog(this.dataset.url || this.getAttribute("href"));
        });

        window.jQuery(document).on("click", ".closeDialog", function () {
            closeAjaxDialog();
        });

        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape") {
                closeAjaxDialog();
            }
        });
    }

    app.ui = app.ui || {};
    app.ui.openAjaxDialog = openAjaxDialog;
    app.ui.closeAjaxDialog = closeAjaxDialog;

    window.App = app;

    applySwalDefaults();
    wireGlobalEvents();

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", showTempDataAlerts);
    } else {
        showTempDataAlerts();
    }
})(window, document);
