let tagList = [];
let lastQuery = null; // null để lần đầu vẫn chạy

document.addEventListener("DOMContentLoaded", () => {
    const input = document.getElementById("content-box");
    const popup = document.getElementById("tag-popup");
    const hiddenField = document.getElementById("tagged-users");

    console.log("post-tagging.js loaded", { inputExists: !!input, popupExists: !!popup, hiddenExists: !!hiddenField });

    if (!input || !popup) return;

    input.addEventListener("keyup", async (e) => {
        // debug nhanh
        // console.log("key", e.key, "value:", input.value, "cursor:", input.selectionStart);

        const text = input.value;
        const cursor = input.selectionStart;

        // tìm vị trí @ gần nhất trước con trỏ
        const atIndex = text.lastIndexOf("@", cursor - 1);
        if (atIndex === -1) {
            popup.style.display = "none";
            lastQuery = null; // reset
            return;
        }

        // Lấy từ sau dấu @ tới con trỏ
        const query = text.substring(atIndex + 1, cursor).trim();

        // Nếu query không thay đổi so với lần trước thì không lặp lại request
        if (lastQuery !== null && query === lastQuery) return;
        lastQuery = query;

        // nếu muốn show gợi ý ngay khi vừa gõ @ (query == ""), gọi API để lấy recommended
        const encoded = encodeURIComponent(query);
        let res;
        try {
            res = await fetch(`/Recommend?keyword=${encoded}`, { cache: "no-store" });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
        } catch (err) {
            console.error("Failed to fetch Recommend:", err);
            popup.style.display = "none";
            return;
        }

        let users = [];
        try {
            users = await res.json();
        } catch (err) {
            console.error("Invalid JSON from Recommend:", err);
            users = [];
        }

        popup.innerHTML = "";

        if (!users || users.length === 0) {
            // nếu muốn, show message "No users"
            const empty = document.createElement("div");
            empty.classList.add("tag-item");
            empty.textContent = "No results";
            empty.style.opacity = "0.6";
            popup.appendChild(empty);
        } else {
            users.forEach(u => {
                const item = document.createElement("div");
                item.classList.add("tag-item");
                item.textContent = u.userName;

                item.onclick = () => {
                    tagList.push(u.id);

                    const newText =
                        text.substring(0, atIndex + 1) +
                        u.userName +
                        " " +
                        text.substring(cursor);

                    input.value = newText;

                    // đặt con trỏ sau tên vừa chèn
                    const newPos = (atIndex + 1) + u.userName.length + 1;
                    input.focus();
                    input.setSelectionRange(newPos, newPos);

                    popup.style.display = "none";
                };

                popup.appendChild(item);
            });
        }

        // Định vị popup: tính toán lại dựa trên rect và scroll
        const rect = input.getBoundingClientRect();
        popup.style.left = rect.left + "px";
        popup.style.top = (rect.bottom + window.scrollY) + "px";

        // đảm bảo hiển thị trên cùng
        popup.style.display = "block";
        popup.style.zIndex = 99999;
    });

    // submit: nếu input.form tồn tại
    if (input.form) {
        input.form.addEventListener("submit", () => {
            hiddenField.value = JSON.stringify(tagList || []);
        });
    } else {
        console.warn("Textarea has no associated form - submit tagList won't be set automatically.");
    }

    // click ngoài -> ẩn popup
    document.addEventListener("click", (e) => {
        if (!popup.contains(e.target) && e.target !== input) {
            popup.style.display = "none";
        }
    });
});
