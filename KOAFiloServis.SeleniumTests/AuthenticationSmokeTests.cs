using System.Diagnostics;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace KOAFiloServis.SeleniumTests;

[TestClass]
[DoNotParallelize]
public sealed class AuthenticationSmokeTests
{
    private static LocalWebAppHost _webAppHost = null!;
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;

    private const string Username = "admin";
    private const string Password = "admin123";

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        _webAppHost = new LocalWebAppHost();
        _webAppHost.Start();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _webAppHost.Dispose();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1600,1200");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--ignore-certificate-errors");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public void Anonymous_User_IsRedirected_To_Login_For_Protected_Page()
    {
        _driver.Navigate().GoToUrl(_webAppHost.GetUrl("/araclar"));

        _wait.Until(d => d.Url.Contains("/login", StringComparison.OrdinalIgnoreCase));

        StringAssert.Contains(_driver.Url, "/login");
    }

    [TestMethod]
    public void Login_With_Valid_Credentials_Navigates_To_Application()
    {
        Login(rememberMe: false);

        _wait.Until(d => d.FindElements(By.Id("logout-button")).Count > 0);

        Assert.IsFalse(_driver.Url.Contains("/login", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Left_Menu_Araclar_Link_Does_Not_Redirect_Back_To_Login()
    {
        Login(rememberMe: false);
        WaitForAuthenticatedShell();

        OpenFiloServisSection();
        Retry(() => _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("nav-link-araclar"))).Click());

        _wait.Until(d => d.Url.Contains("/araclar", StringComparison.OrdinalIgnoreCase));
        _wait.Until(d => d.FindElements(By.XPath("//*[contains(text(),'Araçlar') or contains(text(),'Araclar')]")).Count > 0);

        Assert.IsFalse(_driver.Url.Contains("/login", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void RememberMe_Prefills_Username_After_Logout_Redirect()
    {
        Login(rememberMe: true);

        WaitForAuthenticatedShell();

        Retry(() => _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("logout-button"))).Click());

        _wait.Until(d => d.Url.Contains("/login", StringComparison.OrdinalIgnoreCase));
        _wait.Until(d => d.FindElement(By.Id("kullaniciAdi")).GetAttribute("value") == Username);

        Assert.AreEqual(Username, _driver.FindElement(By.Id("kullaniciAdi")).GetAttribute("value"));
    }

    [TestMethod]
    public void Settings_Menu_License_Link_Does_Not_Redirect_Back_To_Login()
    {
        Login(rememberMe: false);
        WaitForAuthenticatedShell();

        OpenSettingsDropdown();
        Retry(() => _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("settings-license-link"))).Click());

        _wait.Until(d => d.Url.Contains("/ayarlar/lisans", StringComparison.OrdinalIgnoreCase));
        _wait.Until(d => d.FindElements(By.XPath("//*[contains(text(),'Lisans Aktivasyonu')]")).Count > 0);

        Assert.IsFalse(_driver.Url.Contains("/login", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Settings_Menu_Database_Link_Does_Not_Redirect_Back_To_Login()
    {
        Login(rememberMe: false);
        WaitForAuthenticatedShell();

        OpenSettingsDropdown();
        Retry(() => _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("settings-database-link"))).Click());

        _wait.Until(d => d.Url.Contains("/ayarlar/veritabani", StringComparison.OrdinalIgnoreCase));
        _wait.Until(d => d.FindElements(By.XPath("//*[contains(text(),'Veritabani Baglanti Ayarlari')]")).Count > 0);

        Assert.IsFalse(_driver.Url.Contains("/login", StringComparison.OrdinalIgnoreCase));
    }

    private void Login(bool rememberMe)
    {
        _driver.Navigate().GoToUrl(_webAppHost.GetUrl("/login"));

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            TypeInto(By.Id("kullaniciAdi"), Username);
            TypeInto(By.Id("sifre"), Password);

            Retry(() =>
            {
                var rememberMeCheckbox = _wait.Until(ExpectedConditions.ElementExists(By.Id("beniHatirla")));
                if (rememberMeCheckbox.Selected != rememberMe)
                {
                    rememberMeCheckbox.Click();
                }
            });

            Retry(() => _wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.btn-login"))).Click());

            var navigated = SpinWait.SpinUntil(
                () => !_driver.Url.Contains("/login", StringComparison.OrdinalIgnoreCase)
                    || _driver.FindElements(By.Id("logout-button")).Count > 0,
                TimeSpan.FromSeconds(15));

            if (navigated)
            {
                return;
            }

            _driver.Navigate().GoToUrl(_webAppHost.GetUrl("/login"));
        }

        Assert.Fail("Login akisi beklenen surede tamamlanmadi.");
    }

    private void TypeInto(By selector, string text)
    {
        Retry(() =>
        {
            var element = _wait.Until(ExpectedConditions.ElementToBeClickable(selector));
            element.Clear();
            element.SendKeys(text);
        });
    }

    private void WaitForAuthenticatedShell()
    {
        _wait.Until(d => d.FindElements(By.Id("logout-button")).Count > 0);
    }

    private void OpenSettingsDropdown()
    {
        Retry(() => _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("settings-dropdown-button"))).Click());
        _wait.Until(d => d.FindElements(By.Id("settings-license-link")).Count > 0);
    }

    private void OpenFiloServisSection()
    {
        if (_driver.FindElements(By.Id("nav-link-araclar")).Count > 0)
        {
            return;
        }

        Retry(() => _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("nav-section-filo-toggle"))).Click());
        _wait.Until(d => d.FindElements(By.Id("nav-link-araclar")).Count > 0);
    }

    private static void Retry(Action action, int attempts = 3)
    {
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                action();
                return;
            }
            catch (StaleElementReferenceException) when (attempt < attempts)
            {
                Thread.Sleep(200);
            }
            catch (ElementNotInteractableException) when (attempt < attempts)
            {
                Thread.Sleep(200);
            }
        }
    }

    private sealed class LocalWebAppHost : IDisposable
    {
        private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(2) };
        private readonly string _baseUrl = "http://127.0.0.1:5190";
        private Process? _process;
        private bool _ownsProcess;

        public string GetUrl(string relativePath) => $"{_baseUrl}{relativePath}";

        public void Start()
        {
            if (IsApplicationResponsive())
            {
                return;
            }

            var projectPath = FindWebProjectPath();
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectPath}\" --no-launch-profile --urls {_baseUrl}",
                WorkingDirectory = Path.GetDirectoryName(projectPath)!,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

            _process = Process.Start(startInfo) ?? throw new InvalidOperationException("Web uygulamasi baslatilamadi.");
            _ownsProcess = true;

            _ = Task.Run(() => DrainAsync(_process.StandardOutput));
            _ = Task.Run(() => DrainAsync(_process.StandardError));

            var started = SpinWait.SpinUntil(IsApplicationResponsive, TimeSpan.FromSeconds(90));
            if (!started)
            {
                throw new TimeoutException("Web uygulamasi belirlenen surede hazir olmadi.");
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();

            if (!_ownsProcess || _process is null)
            {
                return;
            }

            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                    _process.WaitForExit(5000);
                }
            }
            catch
            {
            }
            finally
            {
                _process.Dispose();
            }
        }

        private bool IsApplicationResponsive()
        {
            try
            {
                using var response = _httpClient.GetAsync(GetUrl("/login")).GetAwaiter().GetResult();
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private static async Task DrainAsync(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                await reader.ReadLineAsync();
            }
        }

        private static string FindWebProjectPath()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current is not null)
            {
                var candidate = Path.Combine(current.FullName, "KOAFiloServis.Web", "KOAFiloServis.Web.csproj");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            throw new FileNotFoundException("KOAFiloServis.Web.csproj bulunamadi.");
        }
    }
}
