using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System.Text;

namespace prep2
{
    public class Tests
    {
        private readonly static string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";
        private WebDriver driver;
        private Actions actions;
        private string? lastCreatedTitle;

        [OneTimeSetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            actions = new Actions(driver);
            driver.Navigate().GoToUrl(BaseUrl);
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            driver.Navigate().GoToUrl($"{BaseUrl}/Users/Login");
            var loginForm = driver.FindElement(By.XPath("//form[@method='post']"));
            actions.ScrollToElement(loginForm).Perform();

            driver.FindElement(By.Id("form3Example3")).SendKeys("test12@mail.com");
            driver.FindElement(By.Id("form3Example4")).SendKeys("123456");

            driver.FindElement(By.CssSelector(".btn")).Click();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }

        [Test, Order(1)]
        public void TestInvalidData()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/Create");

            var formCard = driver.FindElement(By.CssSelector(".card-body"));
            actions.ScrollToElement(formCard).Perform();

            var titleInput = driver.FindElement(By.Id("form3Example1c"));
            titleInput.Clear();
            titleInput.SendKeys("");

            var descriptionInput = driver.FindElement(By.Id("form3Example4cd"));
            descriptionInput.Clear();
            descriptionInput.SendKeys("");

            driver.FindElement(By.CssSelector(".btn.btn-primary")).Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/Revue/Create"), "user should remain on the same page with same URL");

            var errorMessage = driver.FindElement(By.CssSelector(".card-body li"));
            Assert.That(errorMessage.Text, Is.EqualTo("Unable to create new Revue!"), "The error message for invalid data when creating Revue is not there");
        }

        [Test, Order(2)]
        public void TestCreateRandomTitleRevue()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/Create");

            var formCard = driver.FindElement(By.CssSelector(".card-body"));
            actions.ScrollToElement(formCard).Perform();

            var titleInput = driver.FindElement(By.Id("form3Example1c"));
            titleInput.Clear();
            lastCreatedTitle = GenerateRandomString(6);
            titleInput.SendKeys(lastCreatedTitle);

            var descriptionInput = driver.FindElement(By.Id("form3Example4cd"));
            descriptionInput.Clear();
            var newRevueDescription = GenerateRandomString(40);
            descriptionInput.SendKeys(newRevueDescription);

            driver.FindElement(By.CssSelector(".btn.btn-primary")).Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/Revue/MyRevues"), "user was not redirected to My Revues Page");

            var revues = driver.FindElements(By.CssSelector(".card.mb-4"));
            var lastRevueTitle = revues.Last().FindElement(By.CssSelector(".text-muted")).Text;
            Assert.That(lastRevueTitle, Is.EqualTo(lastCreatedTitle), "The last revue is not present");
        }

        [Test, Order(3)]
        public void TestSearchReviueByTitle()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/MyRevues");

            var searchField = driver.FindElement(By.Id("keyword"));
            actions.ScrollToElement(searchField).Perform();

            searchField.SendKeys(lastCreatedTitle);
            driver.FindElement(By.Id("search-button")).Click();

            var searchResultRevueTitle = driver.FindElement(By.CssSelector(".text-muted")).Text;
            Assert.That(searchResultRevueTitle, Is.EqualTo(lastCreatedTitle), "The search resulting revue is not present");
        }

        [Test, Order(4)]
        public void TestEditLastCreatedRevueTitle()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/MyRevues");

            var revues = driver.FindElements(By.CssSelector(".card.mb-4"));
            Assert.That(revues.Count(), Is.AtLeast(1), "There are no Revues");

            var lastRevue = revues.Last();
            actions.ScrollToElement(lastRevue).Perform();
            driver.FindElement(By.XPath($"//div[text()='{lastCreatedTitle}']/..//a[text()='Edit']")).Click();

            var formCard = driver.FindElement(By.CssSelector(".card-body"));
            actions.ScrollToElement(formCard).Perform();

            var titleInput = driver.FindElement(By.Id("form3Example1c"));
            titleInput.Clear();
            lastCreatedTitle = GenerateRandomString(6) + " updated";
            titleInput.SendKeys(lastCreatedTitle);

            driver.FindElement(By.CssSelector(".btn.btn-primary")).Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/Revue/MyRevues"), "User was not redirected to My Revues Page.");

            var revuesResult = driver.FindElements(By.CssSelector(".card.mb-4"));
            var lastRevueTitle = revuesResult.Last().FindElement(By.CssSelector(".text-muted")).Text;
            Assert.That(lastRevueTitle, Is.EqualTo(lastCreatedTitle), "The last Revue is not present on the screen.");
        }

        [Test, Order(5)]
        public void TestDeleteLastCreated()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/MyRevues");

            var revues = driver.FindElements(By.CssSelector(".card.mb-4"));
            Assert.That(revues.Count(), Is.AtLeast(1), "There are no Revues");

            var lastRevue = revues.Last();
            actions.ScrollToElement(lastRevue).Perform();

            driver.FindElement(By.XPath($"//div[text()='{lastCreatedTitle}']/..//a[text()='Delete']")).Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/Revue/MyRevues"), "User was not redirected to My Revues Page.");

            var revuesResult = driver.FindElements(By.CssSelector(".card.mb-4"));
            Assert.That(revuesResult.Count(), Is.LessThan(revues.Count()), "The number of Revues did not decrease");

            var lastRevueTitle = revuesResult.Last().FindElement(By.CssSelector(".text-muted")).Text;
            Assert.That(lastRevueTitle, !Is.EqualTo(lastCreatedTitle), "The last Revue is not present on the screen.");

        }

        [Test, Order(6)]
        public void TestNonExistingRevue()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/MyRevues");

            var searchField = driver.FindElement(By.Id("keyword"));
            actions.ScrollToElement(searchField).Perform();

            searchField.SendKeys("non-existing-revue");

            driver.FindElement(By.Id("search-button")).Click();

            var noResultsMessage = driver.FindElement(By.CssSelector(".text-muted")).Text;

            Assert.That(noResultsMessage, Is.EqualTo("No Revues yet!"));
        }



        public static string GenerateRandomString(int length)
        {
            char[] chars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
            if (length <= 0)
            {
                throw new ArgumentException("Length must be greater than zero.", nameof(length));
            }
            var random = new Random();
            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }
            return result.ToString();
        }
    }
}