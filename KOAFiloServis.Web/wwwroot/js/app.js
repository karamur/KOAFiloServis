// KOA Filo Servis - Genel JS yardımcı fonksiyonları

// Favoriler / Hızlı Erişim
window.favorites = {
    _key: 'crm-favorites',
    getAll: function () {
        try { return JSON.parse(localStorage.getItem(this._key) || '[]'); } catch { return []; }
    },
    add: function (url, title, icon) {
        var list = this.getAll();
        if (!list.some(function (f) { return f.url === url; })) {
            list.unshift({ url: url, title: title, icon: icon || 'bi-star' });
            if (list.length > 20) list = list.slice(0, 20);
            localStorage.setItem(this._key, JSON.stringify(list));
        }
        return list;
    },
    remove: function (url) {
        var list = this.getAll().filter(function (f) { return f.url !== url; });
        localStorage.setItem(this._key, JSON.stringify(list));
        return list;
    },
    isFavorite: function (url) {
        return this.getAll().some(function (f) { return f.url === url; });
    }
};

// Dark Mode
window.darkMode = {
    get: function () {
        return localStorage.getItem('crm-theme') || 'light';
    },
    set: function (theme) {
        localStorage.setItem('crm-theme', theme);
        document.documentElement.setAttribute('data-theme', theme);
    },
    init: function () {
        const saved = localStorage.getItem('crm-theme') || 'light';
        document.documentElement.setAttribute('data-theme', saved);
        return saved;
    }
};

/**
 * Base64 kodlu içeriği tarayıcıda dosya olarak indirir.
 * @param {string} base64 - Base64 kodlu dosya içeriği
 * @param {string} fileName - İndirilecek dosya adı
 * @param {string} mimeType - MIME türü (örn: application/pdf)
 */
window.downloadBase64File = function (base64, fileName, mimeType) {
    const byteChars = atob(base64);
    const byteArrays = [];
    for (let i = 0; i < byteChars.length; i += 512) {
        const slice = byteChars.slice(i, i + 512);
        const bytes = new Uint8Array(slice.length);
        for (let j = 0; j < slice.length; j++) {
            bytes[j] = slice.charCodeAt(j);
        }
        byteArrays.push(bytes);
    }
    const blob = new Blob(byteArrays, { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
