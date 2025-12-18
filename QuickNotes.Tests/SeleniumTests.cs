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
        public void CRUD_Flow_Create_Edit_Delete()
        {
            var username = "crud_user_" + Guid.NewGuid().ToString().Substring(0, 8);
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
            _driver.FindElement(By.Name("content")).SendKeys("Original Note Content");
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();
            wait.Until(d => d.PageSource.Contains("Original Note Content"));

            _driver.FindElement(By.LinkText("Edit")).Click();
            var contentArea = _driver.FindElement(By.Name("content"));
            contentArea.Clear();
            contentArea.SendKeys("Updated Note Content");
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();
            wait.Until(d => d.PageSource.Contains("Updated Note Content"));
            Assert.DoesNotContain("Original Note Content", _driver.PageSource);

            _driver.FindElement(By.XPath("//button[contains(text(), 'Delete')]")).Click();
            _driver.SwitchTo().Alert().Accept();
            wait.Until(d => !d.PageSource.Contains("Updated Note Content"));
            Assert.DoesNotContain("Updated Note Content", _driver.PageSource);
        }

        [Fact]
        public void CreateNote_WithEmptyText_ShouldShowError()
        {
            var username = "empty_user_" + Guid.NewGuid().ToString().Substring(0, 8);
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

            _driver.Navigate().GoToUrl($"{_baseUrl}/Notes/Create");
            _driver.FindElement(By.Name("content")).SendKeys("");
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            wait.Until(d => d.PageSource.Contains("Content cannot be empty"));
            Assert.Contains("Content cannot be empty", _driver.PageSource);
        }

        [Fact]
        public void CreateNote_WithTooLongText_ShouldShowError()
        {
            var username = "long_user_" + Guid.NewGuid().ToString().Substring(0, 8);
            var password = "SafePassword!123";
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var longText = new string('A', 1001);

            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Register");
            _driver.FindElement(By.Id("Username")).SendKeys(username);
            _driver.FindElement(By.Id("Password")).SendKeys(password);
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();
            wait.Until(d => d.Url.Contains("/Account/Login"));
            _driver.FindElement(By.Id("Username")).SendKeys(username);
            _driver.FindElement(By.Id("Password")).SendKeys(password);
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();
            wait.Until(d => d.FindElements(By.XPath("//button[contains(text(), 'Logout')]")).Count > 0);

            _driver.Navigate().GoToUrl($"{_baseUrl}/Notes/Create");
            _driver.FindElement(By.Name("content")).SendKeys(longText);
            _driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            wait.Until(d => d.PageSource.Contains("Content cannot exceed 1000 characters"));
            Assert.Contains("Content cannot exceed 1000 characters", _driver.PageSource);
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
