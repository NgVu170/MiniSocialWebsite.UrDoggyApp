// wwwroot/js/post-tagging.js
let tagList = [];
let lastQuery = null;

document.addEventListener("DOMContentLoaded", () => {
    const input = document.getElementById("postContent");
    const popup = document.getElementById("tag-popup");
    const hiddenField = document.getElementById("tagged-users");

    console.log("post-tagging init", {
        inputExists: !!input,
        popupExists: !!popup,
        hiddenExists: !!hiddenField
    });

    if (!input || !popup) {
        console.error("post-tagging STOPPED: Required elements missing.");
        return;
    }

    input.addEventListener("input", async () => {
        const text = input.value;
        const cursor = input.selectionStart;

        const atIndex = text.lastIndexOf("@", cursor - 1);
        if (atIndex === -1) {
            popup.style.display = "none";
            lastQuery = null;
            return;
        }

        const query = text.substring(atIndex + 1, cursor).trim();

        if (query === lastQuery) return;
        lastQuery = query;

        let res;
        try {
            res = await fetch(`/Recommend?keyword=${encodeURIComponent(query)}`, {
                cache: "no-store"
            });
            if (!res.ok) throw new Error(res.status);
        } catch (err) {
            console.error("Recommend API error:", err);
            popup.style.display = "none";
            return;
        }

        let users = [];
        try {
            users = await res.json();
        } catch {
            users = [];
        }

        popup.innerHTML = "";

        if (users.length === 0) {
            const empty = document.createElement("div");
            empty.className = "tag-item text-muted";
            empty.textContent = "No users found";
            popup.appendChild(empty);
        } else {
            users.forEach(u => {
                const item = document.createElement("div");
                item.className = "tag-item";
                item.textContent = u.userName;

                item.onclick = () => {
                    tagList.push(u.id);

                    const newText =
                        text.substring(0, atIndex + 1) +
                        u.userName + " " +
                        text.substring(cursor);

                    input.value = newText;

                    const newPos = atIndex + u.userName.length + 2;
                    input.focus();
                    input.setSelectionRange(newPos, newPos);

                    popup.style.display = "none";
                };

                popup.appendChild(item);
            });
        }

        const rect = input.getBoundingClientRect();
        popup.style.left = rect.left + "px";
        popup.style.top = rect.bottom + window.scrollY + "px";
        popup.style.zIndex = 99999;
        popup.style.display = "block";
    });

    if (input.form && hiddenField) {
        input.form.addEventListener("submit", () => {
            hiddenField.value = JSON.stringify(tagList);
        });
    }

    document.addEventListener("click", (e) => {
        if (!popup.contains(e.target) && e.target !== input) {
            popup.style.display = "none";
        }
    });
});
