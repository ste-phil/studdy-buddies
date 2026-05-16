window.studyBuddiesLang = {
    set: function (culture) {
        try {
            const value = 'c=' + culture + '|uic=' + culture;
            document.cookie = '.AspNetCore.Culture=' + encodeURIComponent(value) + '; path=/; max-age=31536000; samesite=lax';
            window.location.reload();
        } catch (e) {
            console.error('Failed to set culture', e);
        }
    }
};

window.studyBuddiesTheme = {
    get: function () {
        try {
            const m = document.cookie.match(/(?:^|; )theme=([^;]+)/);
            return m ? decodeURIComponent(m[1]) : null;
        } catch (e) {
            return null;
        }
    },
    set: function (theme) {
        try {
            document.cookie = 'theme=' + encodeURIComponent(theme) + '; path=/; max-age=31536000; samesite=lax';
            if (theme === 'dark') {
                document.documentElement.setAttribute('data-theme', 'dark');
            } else {
                document.documentElement.removeAttribute('data-theme');
            }
        } catch (e) {
            console.error('Failed to set theme', e);
        }
    }
};

window.studyBuddiesHotkeys = {
    _handler: null,
    register: function (dotNetRef) {
        this.unregister();
        this._handler = (e) => {
            const ae = document.activeElement;
            const tag = (ae && ae.tagName ? ae.tagName : '').toLowerCase();
            if (tag === 'input' || tag === 'textarea' || (ae && ae.isContentEditable)) return;
            if (e.ctrlKey || e.altKey || e.metaKey) return;
            const map = { '1': 1, '2': 3, '3': 4, '4': 5 };
            if (e.key in map) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('OnHotkey', map[e.key]);
            }
        };
        document.addEventListener('keydown', this._handler);
    },
    unregister: function () {
        if (this._handler) {
            document.removeEventListener('keydown', this._handler);
            this._handler = null;
        }
    }
};

window.studyBuddiesSpeech = {
    speak: function (text, lang) {
        if (!('speechSynthesis' in window)) {
            return false;
        }
        try {
            window.speechSynthesis.cancel();
            const utterance = new SpeechSynthesisUtterance(text);
            if (lang) {
                utterance.lang = lang;
            }
            window.speechSynthesis.speak(utterance);
            return true;
        } catch (e) {
            console.error('Speech synthesis failed', e);
            return false;
        }
    }
};
