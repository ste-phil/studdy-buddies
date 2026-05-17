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

window.studyBuddiesSound = (function () {
    let ctx = null;
    const isEnabled = () => /(?:^|; )sbsound=on(?:;|$)/.test(document.cookie);
    const ensureCtx = () => {
        if (!isEnabled()) return null;
        if (!('AudioContext' in window || 'webkitAudioContext' in window)) return null;
        if (!ctx) {
            try { ctx = new (window.AudioContext || window.webkitAudioContext)(); }
            catch (e) { return null; }
        }
        if (ctx.state === 'suspended') { try { ctx.resume(); } catch (e) { } }
        return ctx;
    };
    const tone = (freqs, dur, type, gainPeak) => {
        const ac = ensureCtx();
        if (!ac) return;
        const now = ac.currentTime;
        const segments = Array.isArray(freqs) ? freqs : [freqs];
        const segDur = dur / segments.length;
        const osc = ac.createOscillator();
        const gain = ac.createGain();
        osc.type = type || 'sine';
        osc.frequency.setValueAtTime(segments[0], now);
        for (let i = 1; i < segments.length; i++) {
            osc.frequency.linearRampToValueAtTime(segments[i], now + segDur * i);
        }
        const peak = gainPeak != null ? gainPeak : 0.18;
        gain.gain.setValueAtTime(0.0001, now);
        gain.gain.exponentialRampToValueAtTime(peak, now + 0.01);
        gain.gain.exponentialRampToValueAtTime(0.0001, now + dur);
        osc.connect(gain).connect(ac.destination);
        osc.start(now);
        osc.stop(now + dur + 0.02);
    };
    return {
        get: function () { return isEnabled() ? 'on' : 'off'; },
        set: function (state) {
            const val = state === 'on' ? 'on' : 'off';
            document.cookie = 'sbsound=' + val + '; path=/; max-age=31536000; samesite=lax';
            if (val === 'on') { ensureCtx(); }
        },
        pop:     function () { tone(820, 0.08, 'sine', 0.16); },
        flip:    function () { tone([520, 720], 0.10, 'triangle', 0.14); },
        success: function () { tone([523, 659, 784], 0.28, 'sine', 0.18); },
        error:   function () { tone([320, 200], 0.18, 'sine', 0.16); }
    };
})();

window.studyBuddiesConfetti = {
    burst: function (count) {
        if (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            return;
        }
        const n = count || 36;
        const layer = document.createElement('div');
        layer.style.cssText = 'position:fixed;inset:0;pointer-events:none;z-index:9999;overflow:hidden;';
        document.body.appendChild(layer);
        const palette = ['#F08A7E', '#B6A4E8', '#7FD1B0', '#F5C26B', '#F4A496', '#C8B8F0'];
        const emojis = ['❤️', '✨', '💖', '🌸', '⭐'];
        for (let i = 0; i < n; i++) {
            const useEmoji = Math.random() < 0.25;
            const p = document.createElement('span');
            const startX = 50 + (Math.random() - 0.5) * 30;
            const angle = (Math.random() - 0.5) * 140;
            const distance = 40 + Math.random() * 40;
            const rot = (Math.random() - 0.5) * 720;
            const dur = 900 + Math.random() * 700;
            if (useEmoji) {
                p.textContent = emojis[Math.floor(Math.random() * emojis.length)];
                p.style.cssText = 'position:absolute;left:' + startX + 'vw;top:50vh;font-size:' + (16 + Math.random() * 16) + 'px;will-change:transform,opacity;';
            } else {
                const size = 6 + Math.random() * 8;
                p.style.cssText = 'position:absolute;left:' + startX + 'vw;top:50vh;width:' + size + 'px;height:' + (size * 0.4) + 'px;background:' + palette[Math.floor(Math.random() * palette.length)] + ';border-radius:2px;will-change:transform,opacity;';
            }
            layer.appendChild(p);
            p.animate(
                [
                    { transform: 'translate(-50%, -50%) rotate(0deg)', opacity: 1 },
                    { transform: 'translate(' + angle + 'vw, ' + distance + 'vh) rotate(' + rot + 'deg)', opacity: 0 }
                ],
                { duration: dur, easing: 'cubic-bezier(0.2, 0.7, 0.4, 1)', fill: 'forwards' }
            );
        }
        setTimeout(function () { layer.remove(); }, 1800);
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

window.studyBuddiesPrefs = (function () {
    const todayKey = () => 'sb_goal_seen_' + new Date().toISOString().slice(0, 10);
    const badgesKey = 'sb_badges_seen';
    const safeStorage = () => { try { return window.localStorage; } catch (e) { return null; } };
    return {
        seenGoalToday: function () {
            const s = safeStorage(); if (!s) return false;
            try { return s.getItem(todayKey()) === '1'; } catch (e) { return false; }
        },
        markGoalSeen: function () {
            const s = safeStorage(); if (!s) return;
            try { s.setItem(todayKey(), '1'); } catch (e) { }
        },
        getSeenBadges: function () {
            const s = safeStorage(); if (!s) return [];
            try {
                const raw = s.getItem(badgesKey);
                return raw ? JSON.parse(raw) : [];
            } catch (e) { return []; }
        },
        addSeenBadge: function (key) {
            const s = safeStorage(); if (!s) return false;
            try {
                const raw = s.getItem(badgesKey);
                const list = raw ? JSON.parse(raw) : [];
                if (list.indexOf(key) >= 0) return false;
                list.push(key);
                s.setItem(badgesKey, JSON.stringify(list));
                return true;
            } catch (e) { return false; }
        }
    };
})();

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
