// wwwroot/js/hashtag-suggestion.js
class HashtagSuggestion {
    constructor(textareaId, suggestionsBoxId) {
        this.textarea = document.getElementById(textareaId);
        this.suggestionsBox = document.getElementById(suggestionsBoxId);
        this.debounceTimer = null;
        this.currentPrefix = '';

        if (!this.textarea || !this.suggestionsBox) {
            console.warn('HashtagSuggestion: textarea hoặc suggestionsBox không tồn tại');
            return;
        }

        this.init();
    }

    init() {
        this.textarea.addEventListener('input', () => {
            clearTimeout(this.debounceTimer);
            this.debounceTimer = setTimeout(() => this.checkForHashtag(), 300);
        });

        // Ẩn khi click ra ngoài
        document.addEventListener('click', (e) => {
            if (!this.textarea.contains(e.target) && !this.suggestionsBox.contains(e.target)) {
                this.hideSuggestions();
            }
        });
    }

    checkForHashtag() {
        const cursorPos = this.textarea.selectionStart;
        const textBeforeCursor = this.textarea.value.substring(0, cursorPos);

        // Tìm # gần nhất trước con trỏ
        const lastHashIndex = textBeforeCursor.lastIndexOf('#');
        if (lastHashIndex === -1) {
            this.hideSuggestions();
            return;
        }

        // Lấy từ sau dấu cách hoặc xuống dòng gần nhất
        const lastBreak = Math.max(
            textBeforeCursor.lastIndexOf(' '),
            textBeforeCursor.lastIndexOf('\n')
        );
        const textAfterBreak = textBeforeCursor.substring(lastBreak + 1);

        if (!textAfterBreak.startsWith('#')) {
            this.hideSuggestions();
            return;
        }

        const prefix = textAfterBreak.substring(1).toLowerCase();
        if (prefix.length === 0) {
            this.hideSuggestions();
            return;
        }

        this.currentPrefix = prefix;
        this.fetchSuggestions(prefix);
    }

    fetchSuggestions(prefix) {
        fetch(`${window.location.origin}/api/tag/suggest?prefix=${encodeURIComponent(prefix)}`)
            .then(response => response.json())
            .then(data => this.showSuggestions(data))
            .catch(err => {
                console.error('Lỗi gợi ý hashtag:', err);
                this.hideSuggestions();
            });
    }

    showSuggestions(tags) {
        if (!tags || tags.length === 0) {
            this.hideSuggestions();
            return;
        }

        this.suggestionsBox.innerHTML = '';

        tags.forEach(tag => {
            const item = document.createElement('div');
            item.className = 'px-3 py-2 hover-bg-light cursor-pointer border-bottom small';
            item.innerHTML = `
                <div class="d-flex justify-content-between align-items-center">
                    <strong class="text-primary">${tag.display}</strong>
                    <span class="text-muted">${tag.count} bài</span>
                </div>
            `;
            item.onclick = () => this.insertHashtag(tag.name);
            this.suggestionsBox.appendChild(item);
        });

        this.suggestionsBox.classList.remove('d-none');
    }

    insertHashtag(tagName) {
        const cursorPos = this.textarea.selectionStart;
        const textBeforeCursor = this.textarea.value.substring(0, cursorPos);
        const lastHashIndex = textBeforeCursor.lastIndexOf('#');
        const textAfterCursor = this.textarea.value.substring(cursorPos);

        const beforeHash = textBeforeCursor.substring(0, lastHashIndex);
        const newText = beforeHash + '#' + tagName + ' ' + textAfterCursor;

        this.textarea.value = newText;
        this.hideSuggestions();

        // Đặt con trỏ sau hashtag
        const newCursorPos = beforeHash.length + tagName.length + 2;
        this.textarea.focus();
        this.textarea.setSelectionRange(newCursorPos, newCursorPos);
    }

    hideSuggestions() {
        this.suggestionsBox.classList.add('d-none');
        this.suggestionsBox.innerHTML = '';
    }
}

// Khởi tạo khi DOM load xong
document.addEventListener('DOMContentLoaded', () => {
    // Chỉ khởi tạo nếu đang ở trang có textarea tạo bài
    const textarea = document.getElementById('postContent');
    const suggestionsBox = document.getElementById('hashtagSuggestions');

    if (textarea && suggestionsBox) {
        new HashtagSuggestion('postContent', 'hashtagSuggestions');
    }
});