// wwwroot/js/hashtag-suggestion.js
class HashtagSuggestion {
    constructor(textareaId, suggestionsBoxId) {
        this.textarea = document.getElementById(textareaId);
        this.suggestionsBox = document.getElementById(suggestionsBoxId);
        this.debounceTimer = null;

        if (!this.textarea || !this.suggestionsBox) {
            console.warn("HashtagSuggestion disabled: elements missing.");
            return;
        }

        this.init();
    }

    init() {
        this.textarea.addEventListener("input", () => {
            clearTimeout(this.debounceTimer);
            this.debounceTimer = setTimeout(() => this.checkForHashtag(), 300);
        });

        document.addEventListener("click", (e) => {
            if (!this.textarea.contains(e.target) && !this.suggestionsBox.contains(e.target)) {
                this.hideSuggestions();
            }
        });
    }

    checkForHashtag() {
        const cursor = this.textarea.selectionStart;
        const text = this.textarea.value.substring(0, cursor);

        const lastHash = text.lastIndexOf("#");
        if (lastHash === -1) return this.hideSuggestions();

        const prefix = text.substring(lastHash + 1).toLowerCase();
        if (!prefix) return this.hideSuggestions();

        this.fetchSuggestions(prefix);
    }

    fetchSuggestions(prefix) {
        fetch(`/api/tag/suggest?prefix=${encodeURIComponent(prefix)}`)
            .then(res => res.ok ? res.json() : [])
            .then(data => this.showSuggestions(data))
            .catch(() => this.hideSuggestions());
    }

    showSuggestions(tags) {
        if (!tags || !tags.length) return this.hideSuggestions();

        this.suggestionsBox.innerHTML = "";

        tags.forEach(tag => {
            const item = document.createElement("div");
            item.className = "hashtag-item";
            item.textContent = "#" + tag.name;

            item.onclick = () => this.insertHashtag(tag.name);
            this.suggestionsBox.appendChild(item);
        });

        this.suggestionsBox.classList.remove("d-none");
    }

    insertHashtag(tagName) {
        const cursor = this.textarea.selectionStart;
        const text = this.textarea.value;

        const lastHash = text.lastIndexOf("#", cursor);
        const newText =
            text.substring(0, lastHash) +
            "#" + tagName + " " +
            text.substring(cursor);

        this.textarea.value = newText;
        this.hideSuggestions();
    }

    hideSuggestions() {
        this.suggestionsBox.classList.add("d-none");
        this.suggestionsBox.innerHTML = "";
    }
}

document.addEventListener("DOMContentLoaded", () => {
    new HashtagSuggestion("postContent", "hashtagSuggestions");
});
