using Blazored.LocalStorage;
using CRMFiloServis.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace CRMFiloServis.Mobile;

public static class MauiProgram
{
	// API Base URL - Platform bazlı yapılandırma
#if DEBUG
#if WINDOWS
	private const string ApiBaseUrl = "http://localhost:5190/"; // Windows masaüstü için localhost
#elif ANDROID
	private const string ApiBaseUrl = "http://10.0.2.2:5190/"; // Android emulator için host makinesi
#else
	private const string ApiBaseUrl = "http://10.0.0.2:5190/"; // Diğer platformlar / fiziksel cihaz
#endif
#else
	private const string ApiBaseUrl = "https://api.koafiloservis.com/"; // Üretim ortamı
#endif

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddMauiBlazorWebView();

		// Blazored LocalStorage
		builder.Services.AddBlazoredLocalStorage();

		// HttpClient yapılandırması
#if DEBUG
		// Geliştirme ortamında SSL sertifika doğrulamasını bypass et
		builder.Services.AddHttpClient<IApiService, ApiService>(client =>
		{
			client.BaseAddress = new Uri(ApiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
			client.DefaultRequestHeaders.Add("X-Client-Type", "Mobile");
			client.DefaultRequestHeaders.Add("X-Client-Version", "1.0.0");
		}).ConfigurePrimaryHttpMessageHandler(() =>
		{
			var handler = new HttpClientHandler();
			// Geliştirme ortamında self-signed sertifika kabul et
			handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
			return handler;
		});
#else
		builder.Services.AddHttpClient<IApiService, ApiService>(client =>
		{
			client.BaseAddress = new Uri(ApiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
			client.DefaultRequestHeaders.Add("X-Client-Type", "Mobile");
			client.DefaultRequestHeaders.Add("X-Client-Version", "1.0.0");
		});
#endif

		// Servis kayıtları
		builder.Services.AddSingleton<IKonumService, KonumService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
