// F07 composer (prototype composerHTML). The text and the message's ID are kept in this tab's session storage, so a
// lost connection or a reload keeps them (R10), and sending the same draft again stores one message (BR04). Nothing
// is ever sent in the background: only a press of Send or Enter sends.
const MAX = 2000;
const PREFIX = "taxbuddy:chat-draft:";
const OFFLINE = "You're offline. Your message wasn't sent and is still here.";

const read = key => {
    try { return JSON.parse(sessionStorage.getItem(PREFIX + key) || "null"); } catch { return null; }
};
const write = (key, value) => {
    try { value ? sessionStorage.setItem(PREFIX + key, JSON.stringify(value)) : sessionStorage.removeItem(PREFIX + key); } catch { }
};
const newId = () => crypto.randomUUID();
const finePointer = () => matchMedia("(pointer: fine)").matches;

export function init(root, dotnet, key, fallbackKey) {
    const form = root.querySelector("[data-chat-composer]");
    if (!form || form.dataset.ready) return;
    form.dataset.ready = "1";
    const field = form.querySelector("textarea");
    const button = form.querySelector("button[type=submit]");
    const status = form.querySelector("[data-composer-status]");
    const count = form.querySelector("[data-composer-count]");
    const hint = finePointer() ? "Enter to send · Shift+Enter for a new line" : "";

    // A visitor's draft for this accountant moves into the conversation once it exists.
    let draft = read(key);
    if (!draft && fallbackKey) {
        draft = read(fallbackKey);
        if (draft) { write(key, draft); write(fallbackKey, null); }
    }
    let id = draft?.id || newId();
    if (draft?.text && !field.value) field.value = draft.text;
    let sending = false;
    let error = "";

    const update = () => {
        const length = field.value.length;
        count.textContent = `${length.toLocaleString("en-US")} / ${MAX.toLocaleString("en-US")}`;
        count.classList.toggle("over", length > MAX);
        button.disabled = sending || !field.value.trim() || length > MAX;
        status.innerHTML = "";
        if (error) {
            const span = document.createElement("span");
            span.className = "err";
            span.textContent = error;
            status.append(span);
        } else {
            status.textContent = hint;
        }
        field.style.height = "auto";
        field.style.height = `${Math.min(field.scrollHeight + 2, 120)}px`;
    };
    const save = () => write(key, field.value ? { text: field.value, id } : null);

    field.addEventListener("input", () => { error = ""; save(); update(); });
    field.addEventListener("keydown", e => {
        if (e.key === "Enter" && !e.shiftKey && !e.isComposing && finePointer()) {
            e.preventDefault();
            if (!button.disabled) form.requestSubmit();
        }
    });
    root.querySelectorAll("[data-starter]").forEach(chip => chip.addEventListener("click", () => {
        field.value = chip.dataset.starter;
        error = "";
        save();
        update();
        field.focus();
    }));

    form.addEventListener("submit", async e => {
        e.preventDefault();
        if (sending || !field.value.trim() || field.value.length > MAX) return;
        sending = true;
        error = "";
        update();
        const text = field.value;
        let reply;
        try {
            // A connection that drops mid-send never answers. The text and ID stay, so sending again stores it once.
            const timeout = new Promise((_, reject) => setTimeout(() => reject(new Error("timeout")), 20000));
            reply = await Promise.race([dotnet.invokeMethodAsync("SendFromComposer", text, id), timeout]);
        } catch {
            reply = { outcome: "offline" };
        }
        sending = false;
        switch (reply.outcome) {
            case "sent":
                write(key, null);
                id = newId();
                field.value = "";
                break;
            case "moved":
                write(key, null);
                location.replace(reply.url);
                return;
            case "signin":
                save();
                location.assign(reply.url);
                return;
            case "failed":
                // The message is shown with Retry. Its text stays saved with its ID until it is sent, so a reload
                // brings it back into the field and sending it then still stores it once.
                id = newId();
                field.value = "";
                break;
            case "offline":
                error = OFFLINE;
                break;
            default:
                error = reply.error || "Not sent. Please try again.";
                break;
        }
        update();
        if (reply.outcome !== "offline") field.focus();
    });

    form.chatConfirmed = sentId => {
        if (read(key)?.id === sentId && !field.value) write(key, null);
    };
    update();
}

/** After a retry succeeds: forget the saved draft if it was that message. */
export function confirmed(root, sentId) {
    root.querySelector("[data-chat-composer]")?.chatConfirmed?.(sentId);
}

/** Shows the newest message. */
export function scrollToEnd(root) {
    const scroller = root.querySelector("[data-chat-scroll]");
    if (scroller) scroller.scrollTop = scroller.scrollHeight;
}
