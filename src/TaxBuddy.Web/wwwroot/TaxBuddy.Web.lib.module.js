// Blazor JavaScript initializer (loaded by blazor.web.js). The site's pages are static and work without script; this
// file only adds conveniences on top of them.
//
// Profile picture (F06 PP10; prototype option E): on Account, the pencil on the avatar opens the device's file
// chooser, the chosen photo is previewed in the avatar, and Save posts the page's own upload form. Without script
// the pencil is a plain link that shows the picture panel (PP09), so nothing here is required.
const TYPES = ["image/jpeg", "image/png", "image/webp"];
const MAX_BYTES = 5 * 1024 * 1024;

export function setupPicture(root = document) {
    const area = root.querySelector("[data-picture=on]");
    if (!area || area.dataset.ready) return;
    const pencil = area.querySelector("[data-picture-pencil]");
    const input = area.querySelector("#picture-file");
    const confirm = area.querySelector("[data-picture-confirm]");
    if (!pencil || !input || !confirm) return;
    area.dataset.ready = "1";
    area.classList.add("picture-js"); // hides the panel: the name card now does its job

    const avatar = pencil.querySelector(".avatar");
    const remove = area.querySelector("[data-picture-remove]");
    const result = area.querySelector("[data-picture-result]");
    const error = area.querySelector("[data-picture-error]");
    const errorText = area.querySelector("[data-picture-error-text]");
    const original = { html: avatar.innerHTML, className: avatar.className };
    let preview = null;

    if (remove) remove.hidden = false;

    const showError = text => {
        if (result) result.hidden = true;
        errorText.textContent = text;
        error.hidden = false;
    };
    const reset = () => {
        if (preview) URL.revokeObjectURL(preview);
        preview = null;
        input.value = "";
        avatar.className = original.className;
        avatar.innerHTML = original.html;
        confirm.hidden = true;
        if (remove) remove.hidden = false;
    };

    pencil.addEventListener("click", event => {
        event.preventDefault();
        input.click();
    });
    input.addEventListener("change", () => {
        const file = input.files && input.files[0];
        if (!file) return;
        error.hidden = true;
        // The server checks again and has the last word (PP03); these two save a round trip.
        if (!TYPES.includes(file.type)) { reset(); showError("That file isn't a JPEG, PNG or WebP photo."); return; }
        if (file.size > MAX_BYTES) { reset(); showError("That file is over 5 MB. Choose a smaller photo."); return; }
        if (preview) URL.revokeObjectURL(preview);
        preview = URL.createObjectURL(file);
        const image = document.createElement("img");
        image.alt = "";
        image.src = preview;
        avatar.className = original.className.replace(/\bt[1-3]\b/, "").trim() + " has-picture";
        avatar.replaceChildren(image);
        if (result) result.hidden = true;
        if (remove) remove.hidden = true;
        confirm.hidden = false;
        confirm.querySelector("[data-picture-save]").focus();
    });
    area.querySelector("[data-picture-cancel]").addEventListener("click", () => {
        reset();
        pencil.focus();
    });
}

export function afterWebStarted(blazor) {
    setupPicture();
    // Moving between pages replaces the page's content without a full load.
    blazor.addEventListener("enhancedload", () => setupPicture());
}
