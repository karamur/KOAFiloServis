using Microsoft.Extensions.Logging;

namespace KOAFiloServis.Mobile.Services;

/// <summary>
/// MAUI Geolocation API kullanan konum servisi
/// </summary>
public class KonumService : IKonumService, IDisposable
{
    private readonly ILogger<KonumService> _logger;
    private CancellationTokenSource? _cts;
    private bool _disposed;
    
    public event EventHandler<KonumBilgisi>? KonumDegisti;
    public bool TakipAktif { get; private set; }

    public KonumService(ILogger<KonumService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> KonumIzniVarMiAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Konum izni kontrol edilirken hata");
            return false;
        }
    }

    public async Task<bool> KonumIzniIsteAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            
            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // iOS'ta bir kere reddedilince tekrar istemek yerine ayarlara yönlendir
                return false;
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Konum izni istenirken hata");
            return false;
        }
    }

    public async Task<KonumBilgisi?> MevcutKonumuAlAsync()
    {
        try
        {
            var izinVar = await KonumIzniVarMiAsync();
            if (!izinVar)
            {
                izinVar = await KonumIzniIsteAsync();
                if (!izinVar) return null;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location == null)
            {
                // Son bilinen konumu dene
                location = await Geolocation.Default.GetLastKnownLocationAsync();
            }

            if (location != null)
            {
                return new KonumBilgisi
                {
                    Enlem = location.Latitude,
                    Boylam = location.Longitude,
                    Hiz = location.Speed.HasValue ? location.Speed.Value * 3.6 : null, // m/s -> km/h
                    Yon = location.Course,
                    Yukseklik = location.Altitude,
                    Hassasiyet = location.Accuracy,
                    Zaman = location.Timestamp.DateTime
                };
            }
        }
        catch (FeatureNotSupportedException ex)
        {
            _logger.LogWarning(ex, "Cihaz GPS desteklemiyor");
        }
        catch (FeatureNotEnabledException ex)
        {
            _logger.LogWarning(ex, "GPS kapalı");
        }
        catch (PermissionException ex)
        {
            _logger.LogWarning(ex, "Konum izni yok");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Konum alınırken hata");
        }

        return null;
    }

    public async Task<bool> TakipBaslatAsync()
    {
        if (TakipAktif) return true;

        try
        {
            var izinVar = await KonumIzniVarMiAsync();
            if (!izinVar)
            {
                izinVar = await KonumIzniIsteAsync();
                if (!izinVar)
                {
                    _logger.LogWarning("Konum izni alınamadı, takip başlatılamıyor");
                    return false;
                }
            }

            _cts = new CancellationTokenSource();
            TakipAktif = true;

            // Arka planda konum takibi
            _ = Task.Run(async () => await KonumTakipDongusuAsync(_cts.Token));

            _logger.LogInformation("Konum takibi başlatıldı");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Konum takibi başlatılırken hata");
            TakipAktif = false;
            return false;
        }
    }

    public Task DurdurAsync()
    {
        try
        {
            _cts?.Cancel();
            TakipAktif = false;
            _logger.LogInformation("Konum takibi durduruldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Konum takibi durdurulurken hata");
        }

        return Task.CompletedTask;
    }

    private async Task KonumTakipDongusuAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var konum = await MevcutKonumuAlAsync();
                if (konum != null)
                {
                    KonumDegisti?.Invoke(this, konum);
                }

                // 10 saniyede bir konum al
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Konum takip döngüsünde hata");
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _cts?.Cancel();
        _cts?.Dispose();
        _disposed = true;
    }
}
