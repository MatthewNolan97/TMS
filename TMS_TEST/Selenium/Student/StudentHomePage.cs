//using Xunit;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.Support.UI;
//using System;
//using System.Threading;

//namespace TMS_TEST.Selenium.Student
//{
//    public class StudentHomePageTest
//    {
//        [Fact]
//        public void HomePage_Should_Display_ToysLink_And_SignOutButton()
//        {
//            var options = new ChromeOptions();
//            options.AddArgument("--start-maximized");

//            IWebDriver driver = new ChromeDriver(options);
//            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

//            try
//            {
//                // Navigate to login page
//                driver.Navigate().GoToUrl("http://localhost:5000/Home/Login");
//                Assert.Contains("Student Login", driver.Title);
//                Console.WriteLine("On login page...");
//                Thread.Sleep(3000); // Pause to view login page

//                // Wait for Sign In button and click it
//                var signInButton = wait.Until(d =>
//                {
//                    var btn = d.FindElement(By.CssSelector("a.btn-primary"));
//                    return btn.Displayed && btn.Enabled ? btn : null;
//                });
//                signInButton.Click();
//                Console.WriteLine("Clicked Sign In button...");
//                Thread.Sleep(3000); // Pause to see button click effect

//                // Pause for manual login
//                Console.WriteLine("Please log in manually and press Enter when done...");
//                Console.ReadLine();

//                // Navigate to dashboard after login
//                driver.Navigate().GoToUrl("http://localhost:5000/Home/Dashboard");
//                Console.WriteLine("Navigated to Dashboard...");
//                Thread.Sleep(3000); // Pause to view dashboard

//                // Wait for header and verify
//                var header = wait.Until(d =>
//                {
//                    var el = d.FindElement(By.CssSelector("header.header h1"));
//                    return el.Displayed ? el : null;
//                });
//                Assert.Contains("Toy Management System", header.Text);
//                Console.WriteLine("Header found: " + header.Text);
//                Thread.Sleep(3000); // Pause to view header

//                // Wait for Toys link
//                var toysLink = wait.Until(d =>
//                {
//                    var el = d.FindElement(By.CssSelector("header.header a.nav-link[href='/Toy']"));
//                    return el.Displayed && el.Enabled ? el : null;
//                });
//                Assert.Contains("Toys", toysLink.Text);
//                toysLink.Click();
//                Console.WriteLine("Clicked Toys link...");
//                Thread.Sleep(3000); // Pause to view Toys page

//                // Check that URL contains /Toy
//                string currentUrl = driver.Url;
//                Assert.Contains("/Toy", currentUrl);
//                Console.WriteLine("Current URL: " + currentUrl);
//                Thread.Sleep(3000); // Pause to view URL change

//                // Wait for Sign Out button
//                var signOutBtn = wait.Until(d =>
//                {
//                    var el = d.FindElement(By.CssSelector("header.header a.btn-danger[href='/MicrosoftIdentity/Account/SignOut']"));
//                    return el.Displayed && el.Enabled ? el : null;
//                });
//                Assert.Contains("Sign Out", signOutBtn.Text);
//                signOutBtn.Click();
//                Console.WriteLine("Clicked Sign Out button...");
//                Thread.Sleep(3000); // Pause to view sign out

//                Console.WriteLine("Test finished. Press Enter to close browser...");
//                Console.ReadLine();
//            }
//            finally
//            {
//                driver.Quit();
//            }
//        }// cd to the project directory and run: dotnet test --filter StudentHomePageTest
//         // This test lets you manually sign in and then it checks the toys view and then signs out, eventually it will borrow a toy and then return it
//    }
//}

