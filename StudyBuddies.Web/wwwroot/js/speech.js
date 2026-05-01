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
