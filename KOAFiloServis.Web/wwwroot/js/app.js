// KOA Filo Servis - Genel JS yardımcı fonksiyonları

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
