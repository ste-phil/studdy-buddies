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
