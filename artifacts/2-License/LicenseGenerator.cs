using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CRMFiloServis.Licensing;

/// <summary>
/// Lisans dosyası oluşturma servisi - Sadece yönetim tarafı için
/// </summary>
public class LicenseGenerator
{
    private const string PrivateKeyPath = "license-private.key";
    private const string PublicKeyPath = "license-public.key";
    
    /// <summary>
    /// RSA anahtar çifti oluşturur
    /// </summary>
    public static void GenerateKeyPair()
    {
        using var rsa = RSA.Create(2048);
        
        // Private key (güvenli saklanmalı)
        var privateKey = rsa.ExportRSAPrivateKey();
        File.WriteAllBytes(PrivateKeyPath, privateKey);
        
        // Public key (uygulamaya gömülür)
        var publicKey = rsa.ExportRSAPublicKey();
        File.WriteAllBytes(PublicKeyPath, publicKey);
        
        Console.WriteLine("Anahtar çifti oluşturuldu.");
        Console.WriteLine($"Private Key: {PrivateKeyPath} (GÜVENLİ SAKLAYIN!)");
        Console.WriteLine($"Public Key: {PublicKeyPath} (Uygulamaya ekleyin)");
    }
    
    /// <summary>
    /// Lisans dosyası oluşturur
    /// </summary>
    public static string GenerateLicense(LicenseInfo info)
    {
        // Private key'i yükle
        var privateKeyBytes = File.ReadAllBytes(PrivateKeyPath);
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        
        // Lisans ID oluştur
        info.LicenseId = Guid.NewGuid().ToString();
        
        // Lisans verisini JSON'a çevir
        var licenseData = JsonSerializer.Serialize(info);
        var dataBytes = Encoding.UTF8.GetBytes(licenseData);
        
        // İmzala
        var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var signatureBase64 = Convert.ToBase64String(signature);
        
        // Lisans dosyası formatı
        var licenseFile = $@"-----BEGIN LICENSE-----
LicenseId: {info.LicenseId}
Type: {info.Type}
Company: {info.Company}
MaxUsers: {info.MaxUsers}
MaxVehicles: {info.MaxVehicles}
IssuedDate: {info.IssuedDate:yyyy-MM-dd}
ExpiryDate: {info.ExpiryDate:yyyy-MM-dd}
Features: {string.Join(",", info.Features)}
HardwareId: {info.HardwareId ?? "*"}
Signature: {signatureBase64}
-----END LICENSE-----";

        return licenseFile;
    }
    
    /// <summary>
    /// Toplu lisans oluşturma örneği
    /// </summary>
    public static void GenerateSampleLicenses()
    {
        var licenses = new[]
        {
            new LicenseInfo
            {
                Type = "Trial",
                Company = "Demo Company",
                MaxUsers = 2,
                MaxVehicles = 10,
                IssuedDate = DateTime.Today,
                ExpiryDate = DateTime.Today.AddDays(30),
                Features = new[] { "Basic", "Reports" }
            },
            new LicenseInfo
            {
                Type = "Starter",
                Company = "Sample Customer",
                MaxUsers = 5,
                MaxVehicles = 50,
                IssuedDate = DateTime.Today,
                ExpiryDate = DateTime.Today.AddYears(1),
                Features = new[] { "Basic", "Reports", "Export" }
            },
            new LicenseInfo
            {
                Type = "Professional",
                Company = "Professional Customer",
                MaxUsers = 20,
                MaxVehicles = 200,
                IssuedDate = DateTime.Today,
                ExpiryDate = DateTime.Today.AddYears(1),
                Features = new[] { "All" }
            },
            new LicenseInfo
            {
                Type = "Enterprise",
                Company = "Enterprise Customer",
                MaxUsers = -1, // Sınırsız
                MaxVehicles = -1,
                IssuedDate = DateTime.Today,
                ExpiryDate = DateTime.Today.AddYears(1),
                Features = new[] { "All", "API", "Customization", "MultiTenant" }
            }
        };
        
        foreach (var license in licenses)
        {
            var licenseFile = GenerateLicense(license);
            var fileName = $"license-{license.Type.ToLower()}-sample.key";
            File.WriteAllText(fileName, licenseFile);
            Console.WriteLine($"Oluşturuldu: {fileName}");
        }
    }
}

public class LicenseInfo
{
    public string LicenseId { get; set; } = "";
    public string Type { get; set; } = "Trial";
    public string Company { get; set; } = "";
    public int MaxUsers { get; set; } = 2;
    public int MaxVehicles { get; set; } = 10;
    public DateTime IssuedDate { get; set; } = DateTime.Today;
    public DateTime ExpiryDate { get; set; } = DateTime.Today.AddDays(30);
    public string[] Features { get; set; } = Array.Empty<string>();
    public string? HardwareId { get; set; }
}
