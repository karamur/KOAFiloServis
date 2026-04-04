using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CRMFiloServis.Web.Services;

/// <summary>
/// Lisans doğrulama servisi - Uygulamaya dahil edilir
/// </summary>
public class LicenseValidator
{
    // Public key uygulamaya gömülü olmalı (Base64 encoded)
    private const string EmbeddedPublicKey = "YOUR_PUBLIC_KEY_BASE64_HERE";
    
    private readonly string _licensePath;
    private LicenseStatus? _cachedStatus;
    private DateTime _lastCheck = DateTime.MinValue;
    
    public LicenseValidator(string licensePath = "license.key")
    {
        _licensePath = licensePath;
    }
    
    /// <summary>
    /// Lisans durumunu kontrol eder
    /// </summary>
    public LicenseStatus ValidateLicense()
    {
        // Cache kontrolü (her 5 dakikada bir kontrol)
        if (_cachedStatus != null && (DateTime.Now - _lastCheck).TotalMinutes < 5)
            return _cachedStatus;
        
        try
        {
            if (!File.Exists(_licensePath))
            {
                return new LicenseStatus
                {
                    IsValid = false,
                    ErrorMessage = "Lisans dosyası bulunamadı"
                };
            }
            
            var licenseContent = File.ReadAllText(_licensePath);
            var status = ParseAndValidate(licenseContent);
            
            _cachedStatus = status;
            _lastCheck = DateTime.Now;
            
            return status;
        }
        catch (Exception ex)
        {
            return new LicenseStatus
            {
                IsValid = false,
                ErrorMessage = $"Lisans doğrulama hatası: {ex.Message}"
            };
        }
    }
    
    private LicenseStatus ParseAndValidate(string content)
    {
        var status = new LicenseStatus();
        
        // Lisans bilgilerini parse et
        var licenseId = ExtractValue(content, "LicenseId");
        var type = ExtractValue(content, "Type");
        var company = ExtractValue(content, "Company");
        var maxUsersStr = ExtractValue(content, "MaxUsers");
        var maxVehiclesStr = ExtractValue(content, "MaxVehicles");
        var issuedDateStr = ExtractValue(content, "IssuedDate");
        var expiryDateStr = ExtractValue(content, "ExpiryDate");
        var featuresStr = ExtractValue(content, "Features");
        var signature = ExtractValue(content, "Signature");
        
        // Zorunlu alanları kontrol et
        if (string.IsNullOrEmpty(licenseId) || string.IsNullOrEmpty(type) || 
            string.IsNullOrEmpty(signature))
        {
            status.ErrorMessage = "Geçersiz lisans formatı";
            return status;
        }
        
        // Tarihleri parse et
        if (!DateTime.TryParse(expiryDateStr, out var expiryDate))
        {
            status.ErrorMessage = "Geçersiz bitiş tarihi";
            return status;
        }
        
        // Süre kontrolü
        if (expiryDate < DateTime.Today)
        {
            status.ErrorMessage = "Lisans süresi dolmuş";
            status.ExpiryDate = expiryDate;
            return status;
        }
        
        // İmza doğrulama
        var dataToVerify = BuildDataString(licenseId, type, company, maxUsersStr, 
            maxVehiclesStr, issuedDateStr, expiryDateStr, featuresStr);
        
        if (!VerifySignature(dataToVerify, signature))
        {
            status.ErrorMessage = "Geçersiz lisans imzası";
            return status;
        }
        
        // Başarılı
        status.IsValid = true;
        status.LicenseId = licenseId;
        status.Type = type;
        status.Company = company;
        status.MaxUsers = int.TryParse(maxUsersStr, out var mu) ? mu : 0;
        status.MaxVehicles = int.TryParse(maxVehiclesStr, out var mv) ? mv : 0;
        status.ExpiryDate = expiryDate;
        status.Features = featuresStr?.Split(',') ?? Array.Empty<string>();
        
        // Süre uyarısı (30 gün kala)
        var daysRemaining = (expiryDate - DateTime.Today).Days;
        if (daysRemaining <= 30)
        {
            status.WarningMessage = $"Lisans {daysRemaining} gün sonra dolacak";
        }
        
        return status;
    }
    
    private string ExtractValue(string content, string key)
    {
        var match = Regex.Match(content, $@"{key}:\s*(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }
    
    private string BuildDataString(params string[] values)
    {
        return string.Join("|", values.Where(v => !string.IsNullOrEmpty(v)));
    }
    
    private bool VerifySignature(string data, string signatureBase64)
    {
        try
        {
            // Demo modda imza kontrolü atlanır
            if (signatureBase64 == "DEMO_LICENSE_NOT_FOR_PRODUCTION")
                return true;
            
            var publicKeyBytes = Convert.FromBase64String(EmbeddedPublicKey);
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(publicKeyBytes, out _);
            
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = Convert.FromBase64String(signatureBase64);
            
            return rsa.VerifyData(dataBytes, signatureBytes, 
                HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch
        {
            return false;
        }
    }
}

public class LicenseStatus
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? WarningMessage { get; set; }
    
    public string LicenseId { get; set; } = "";
    public string Type { get; set; } = "";
    public string Company { get; set; } = "";
    public int MaxUsers { get; set; }
    public int MaxVehicles { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string[] Features { get; set; } = Array.Empty<string>();
    
    public bool HasFeature(string feature)
    {
        return Features.Contains("All") || Features.Contains(feature);
    }
    
    public int DaysRemaining => (ExpiryDate - DateTime.Today).Days;
}
