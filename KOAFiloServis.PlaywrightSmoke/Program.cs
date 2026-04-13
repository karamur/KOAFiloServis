using Microsoft.Playwright;

var baseUrl = args.FirstOrDefault()
    ?? Environment.GetEnvironmentVariable("CRMFILO_BASE_URL")
    ?? "http://127.0.0.1:5190";

var username = Environment.GetEnvironmentVariable("CRMFILO_TEST_USER") ?? "admin";
var password = Environment.GetEnvironmentVariable("CRMFILO_TEST_PASSWORD") ?? "admin123";

Console.WriteLine($"Playwright smoke test başlıyor: {baseUrl}");

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = true
});

await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
{
    IgnoreHTTPSErrors = true,
    ViewportSize = new ViewportSize { Width = 1440, Height = 900 }
});

var page = await context.NewPageAsync();

await AssertAnonymousRedirectAsync(page, baseUrl);
await AssertLoginAsync(page, baseUrl, username, password);
await AssertPersonelMenuAsync(page, baseUrl);
await AssertPortalLandingAsync(page, baseUrl);

Console.WriteLine("Tüm Playwright smoke testleri başarılı.");
return;

static async Task AssertAnonymousRedirectAsync(IPage page, string baseUrl)
{
    await page.GotoAsync($"{baseUrl}/dashboard", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    await page.WaitForTimeoutAsync(1000);

    if (!page.Url.Contains("/login", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Anonim kullanıcı login sayfasına yönlenmedi. URL: {page.Url}");
    }

    Console.WriteLine("✓ Anonim yönlendirme testi geçti");
}

static async Task AssertLoginAsync(IPage page, string baseUrl, string username, string password)
{
    await page.GotoAsync($"{baseUrl}/login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    await page.FillAsync("#kullaniciAdi", username);
    await page.FillAsync("#sifre", password);
    await page.ClickAsync("button:has-text('Giris Yap')");
    await page.WaitForURLAsync("**/dashboard", new PageWaitForURLOptions { Timeout = 15000 });

    Console.WriteLine("✓ Giriş testi geçti");
}

static async Task AssertPersonelMenuAsync(IPage page, string baseUrl)
{
    await page.GotoAsync($"{baseUrl}/personel", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    var bodyText = await page.TextContentAsync("body") ?? string.Empty;
    if (!bodyText.Contains("Personel Listesi", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Personel sayfası açılamadı veya beklenen içerik görünmüyor.");
    }

    Console.WriteLine("✓ Personel menüsü/sayfa testi geçti");
}

static async Task AssertPortalLandingAsync(IPage page, string baseUrl)
{
    await page.GotoAsync(baseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    var bodyText = await page.TextContentAsync("body") ?? string.Empty;

    if (!bodyText.Contains("Kurumsal Çözüm Portalı", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Public landing sayfası beklenen içeriği göstermiyor.");
    }

    Console.WriteLine("✓ Public landing testi geçti");
}
