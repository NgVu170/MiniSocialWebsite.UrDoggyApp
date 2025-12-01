let tagList = [];
let lastQuery = "";

document.addEventListener("DOMContentLoaded", () => {
    const input = document.getElementById("content-box");
    const popup = document.getElementById("tag-popup");

    if (!input || !popup) return;

    input.addEventListener("keyup", async (e) => {
        const text = input.value;
        const cursor = input.selectionStart;

        const atIndex = text.lastIndexOf("@", cursor - 1);
        if (atIndex === -1) {
            popup.style.display = "none";
            return;
        }

        const query = text.substring(atIndex + 1, cursor).trim();
        if (query === lastQuery) return;
        lastQuery = query;

        let res = await fetch(`/Api/Friends/Recommend?keyword=${query}`);
        let users = await res.json();

        popup.innerHTML = "";
        users.forEach(u => {
            const item = document.createElement("div");
            item.classList.add("tag-item");
            item.textContent = u.userName;

            item.onclick = () => {
                tagList.push(u.id);

                let newText =
                    text.substring(0, atIndex + 1) +
                    u.userName +
                    " " +
                    text.substring(cursor);

                input.value = newText;
                popup.style.display = "none";
            };

            popup.appendChild(item);
        });

        popup.style.display = "block";
    });
});
