using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using System;

namespace QuickNotes.Tests
{
    public class SeleniumTests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl = "http://localhost:5035";

        public SeleniumTests()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            _driver = new ChromeDriver(options);
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }

        [Fact]
        public void FullUserFlow_Register_Login_CreateNote()
        {
            var username = "user_" + Guid.NewGuid().ToString().Substring(0, 8);
            var password = "SafePassword!123";

            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Register");
            _driver.FindElement(By.Id("Username")).SendKeys(username);
            _driver.FindElement(By.Id("Password")).SendKeys(password);
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Url.Contains("/Account/Login"));

            _driver.FindElement(By.Id("Username")).SendKeys(username);
            _driver.FindElement(By.Id("Password")).SendKeys(password);
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            wait.Until(d => d.FindElements(By.XPath("//button[contains(text(), 'Logout')]")).Count > 0);

            _driver.Navigate().GoToUrl($"{_baseUrl}/Notes");
            
            wait.Until(d => d.FindElements(By.CssSelector("a.btn-success")).Count > 0);
            
            _driver.FindElement(By.CssSelector("a.btn-success")).Click();
            _driver.FindElement(By.Name("content")).SendKeys("Buy milk and eggs");
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            wait.Until(d => d.Url.Contains("/Notes")); 
            
            wait.Until(d => d.PageSource.Contains("Buy milk and eggs"));
            
            var pageSource = _driver.PageSource;
            Assert.Contains("Buy milk and eggs", pageSource);
        }

        [Fact]
        public void Login_WithInvalidCredentials_ShouldShowError()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            
            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Login");
            _driver.FindElement(By.Id("Username")).SendKeys("nonexistent_user");
            _driver.FindElement(By.Id("Password")).SendKeys("wrongpassword");
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            wait.Until(d => d.PageSource.Contains("Invalid login attempt"));
            Assert.Contains("Invalid login attempt", _driver.PageSource);
        }

        [Fact]
        public void Register_WithDuplicateUsername_ShouldShowError()
        {
            var username = "duplicate_" + Guid.NewGuid().ToString().Substring(0, 8);
            var password = "SafePassword!123";
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Register");
            _driver.FindElement(By.Id("Username")).SendKeys(username);
            _driver.FindElement(By.Id("Password")).SendKeys(password);
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();
            wait.Until(d => d.Url.Contains("/Account/Login"));

            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Register");
            _driver.FindElement(By.Id("Username")).SendKeys(username);
            _driver.FindElement(By.Id("Password")).SendKeys(password);
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            wait.Until(d => d.PageSource.Contains("Username already exists"));
            Assert.Contains("Username already exists", _driver.PageSource);
        }

        [Fact]
        public void AccessNotesPage_WhenNotLoggedIn_ShouldReturnUnauthorized()
        {
            _driver.Manage().Cookies.DeleteAllCookies();
            
            _driver.Navigate().GoToUrl($"{_baseUrl}/Notes");
            
            System.Threading.Thread.Sleep(1000);
            
            var hasCreateButton = _driver.FindElements(By.CssSelector("a.btn-success")).Count > 0;
            Assert.False(hasCreateButton, "Create button should not be visible when not authenticated");
        }

        [Fact]
        public void Logout_ShouldRedirectToHomeAndRemoveAuth()
        {
            var username = "logout_user_" + Guid.NewGuid().ToString().Substring(0, 8);
            var password = "SafePassword!123";
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Register");
            _driver.FindElement(By.Id("Username")).SendKeys(username);
            _driver.FindElement(By.Id("Password")).SendKeys(password);
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();
            
            wait.Until(d => d.Url.Contains("/Account/Login"));
            
            _driver.FindElement(By.Id("Username")).SendKeys(username);
            _driver.FindElement(By.Id("Password")).SendKeys(password);
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            wait.Until(d => d.FindElements(By.XPath("//button[contains(text(), 'Logout')]")).Count > 0);

            _driver.FindElement(By.XPath("//button[contains(text(), 'Logout')]")).Click();
            
            wait.Until(d => d.FindElements(By.XPath("//button[contains(text(), 'Logout')]")).Count == 0);
            
            System.Threading.Thread.Sleep(1000);

            _driver.Navigate().GoToUrl($"{_baseUrl}/Notes");
            System.Threading.Thread.Sleep(1000);
            var hasCreateButton = _driver.FindElements(By.CssSelector("a.btn-success")).Count > 0;
            Assert.False(hasCreateButton, "Should not have access to Notes after logout");
        }

        [Fact]
        public void CreateMultipleNotes_ShouldDisplayAllNotes()
        {
            var username = "multi_notes_" + Guid.NewGuid().ToString().Substring(0, 8);
            var password = "SafePassword!123";
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Register");
            _driver.FindElement(By.Id("Username")).SendKeys(username);
            _driver.FindElement(By.Id("Password")).SendKeys(password);
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();
            
            wait.Until(d => d.Url.Contains("/Account/Login"));
            
            _driver.FindElement(By.Id("Username")).SendKeys(username);
            _driver.FindElement(By.Id("Password")).SendKeys(password);
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            wait.Until(d => d.FindElements(By.XPath("//button[contains(text(), 'Logout')]")).Count > 0);

            _driver.Navigate().GoToUrl($"{_baseUrl}/Notes");
            wait.Until(d => d.FindElements(By.CssSelector("a.btn-success")).Count > 0);
            _driver.FindElement(By.CssSelector("a.btn-success")).Click();
            _driver.FindElement(By.Name("content")).SendKeys("First note");
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();
            wait.Until(d => d.PageSource.Contains("First note"));

            _driver.FindElement(By.CssSelector("a.btn-success")).Click();
            _driver.FindElement(By.Name("content")).SendKeys("Second note");
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();
            wait.Until(d => d.PageSource.Contains("Second note"));

            _driver.FindElement(By.CssSelector("a.btn-success")).Click();
            _driver.FindElement(By.Name("content")).SendKeys("Third note");
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();
            wait.Until(d => d.PageSource.Contains("Third note"));

            var pageSource = _driver.PageSource;
            Assert.Contains("First note", pageSource);
            Assert.Contains("Second note", pageSource);
            Assert.Contains("Third note", pageSource);
        }

        [Fact]
        public void HomePage_WhenNotLoggedIn_ShouldShowLoginAndRegisterLinks()
        {
            _driver.Manage().Cookies.DeleteAllCookies();
            
            _driver.Navigate().GoToUrl($"{_baseUrl}");
            
            System.Threading.Thread.Sleep(1000);
            
            var pageSource = _driver.PageSource;
            Assert.Contains("Login", pageSource);
            Assert.Contains("Register", pageSource);
            Assert.DoesNotContain("Logout", pageSource);
        }
    }
}
