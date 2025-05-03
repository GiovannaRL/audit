using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;			
using OpenQA.Selenium.Firefox;	
using OpenQA.Selenium.Chrome;	
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;

namespace xPlannerAutomatedUITests
{
    [TestClass]
    public class SmokeTests
    {
        private TestContext testContextInstance;
        private IWebDriver _driver;
        private string _url;

        [TestInitialize]
        public void Initialize()
        {
            _url = "http://localhost:51134/xPlannerUI/xplanner_debug.html";

            string browser = "Chrome";
            switch (browser)
            {
                case "Chrome":
                    _driver = new ChromeDriver();
                    break;
                case "Firefox":
                    _driver = new FirefoxDriver();
                    break;
                case "IE":
                    _driver = new InternetExplorerDriver();
                    break;
                default:
                    _driver = new ChromeDriver();
                    break;
            }

        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            _driver.Quit();
        }

        [TestMethod]
        public void Login()
        {
            // Browse to Xplanner Site
            _driver.Navigate().GoToUrl(_url + "#/login");
            // Validate Xplanner Site
            Assert.AreEqual("", _driver.FindElement(By.Id("Image1")).Text);
            // Fill the login Fild
            _driver.FindElement(By.Name("username")).Clear();
            _driver.FindElement(By.Name("username")).SendKeys("rafael.toledo@audaxware.com");
            _driver.FindElement(By.Name("password")).Clear();
            _driver.FindElement(By.Name("password")).SendKeys("-r4F413x245#");
            // Validate Login
            _driver.FindElement(By.XPath("//button[@type='button']")).Click();
            // Select the Enterprise
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            var domainBox = new SelectElement(_driver.FindElement(By.Name("domain")));
            domainBox.SelectByText("AudaxWare");
            _driver.FindElement(By.Name("Select")).Click();
            // Check home page            
            Assert.AreEqual("Audaxware xPlanner", _driver.FindElement(By.XPath("//h1")).Text);
        }
    }
}
