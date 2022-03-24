using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FuckDrive4You
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Title = "FuckDrive4You";
                ConsoleExtensions.WriteLine("Preparing setup...", ConsoleColor.Magenta);
                Run();
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteLine(ex.ToString(), ConsoleColor.Red);
            }
        }

        public static void Run()
        {
            ChromeOptions cho = new ChromeOptions();
            cho.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
            cho.AddArgument("--incognito");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            IWebDriver driver = new ChromeDriver(service, cho);
            TimeSpan waittime = driver.Manage().Timeouts().ImplicitWait;

        retrylogin:
            ConsoleExtensions.Write("Waiting for user to login", ConsoleColor.Cyan);
            driver.Navigate().GoToUrl("https://undervisning.drive4you.dk/Account/Login");

            while (driver.Url == "https://undervisning.drive4you.dk/Account/Login")
            {
                ConsoleExtensions.Write(".", ConsoleColor.Cyan);
                Thread.Sleep(1000);
            }
            if (driver.Url != "https://undervisning.drive4you.dk/Home/Student")
            {
                Console.WriteLine();
                ConsoleExtensions.WriteLine("Url changed, but user has not logged in?", ConsoleColor.Yellow);
                Thread.Sleep(1000);
                goto retrylogin;
            }

            Console.WriteLine();
            ConsoleExtensions.WriteLine("User has logged in!", ConsoleColor.Green);
        restart:
            ConsoleExtensions.WriteLine("SET TEST TO DIGITAL WITH NO SOUND!!!", ConsoleColor.Yellow);
            ConsoleExtensions.WriteLine("MAKE SURE YOU ARE ON QUESTION 1 BEFORE YOU CLICK ENTER!!!", ConsoleColor.Yellow);
            ConsoleExtensions.WriteLine("READY!!! Start the test, and click enter here!", ConsoleColor.Green);
            Console.ReadLine();

            try
            {
                ConsoleExtensions.WriteLine("Gathering result id, please wait...", ConsoleColor.Cyan);
                int resultid = Convert.ToInt32(driver.FindElement(By.Id("ElevProveResultID")).GetAttribute("value"));
                ConsoleExtensions.WriteLine($"Result id found: {resultid}", ConsoleColor.Green);
                ConsoleExtensions.WriteLine("Dumping answers, please wait...", ConsoleColor.Cyan);
                AnswersLoader.DumpAnswers(driver, resultid);
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteLine("DUMPING ANSWERS FAILED!!!", ConsoleColor.Red);
                ConsoleExtensions.WriteLine(ex.ToString(), ConsoleColor.Red);
                goto restart;
            }

            ConsoleExtensions.WriteLine("Answers dumped!", ConsoleColor.Green);

            ConsoleExtensions.WriteLine("Filling out answers, please wait...", ConsoleColor.Cyan);

            foreach (Answer answer in AnswersLoader.LoadedAnswers)
            {
                Thread.Sleep(500);
                if (answer.ksvar1 == 1) driver.FindElement(By.Id("a1y")).Click();
                else if (answer.ksvar1 == 0) driver.FindElement(By.Id("a1n")).Click();

                if (answer.ksvar2 == 1) driver.FindElement(By.Id("a2y")).Click();
                else if (answer.ksvar2 == 0) driver.FindElement(By.Id("a2n")).Click();

                if (answer.ksvar3 == 1) driver.FindElement(By.Id("a3y")).Click();
                else if (answer.ksvar3 == 0) driver.FindElement(By.Id("a3n")).Click();

                if (answer.ksvar4 == 1) driver.FindElement(By.Id("a4y")).Click();
                else if (answer.ksvar4 == 0) driver.FindElement(By.Id("a4n")).Click();

                //ConsoleExtensions.WriteLine("Click enter to continue", ConsoleColor.Cyan);
                //Console.ReadLine();

                if (answer == AnswersLoader.LoadedAnswers.Last())
                {
                    Random rnd = new Random();
                    int minutestowaitfor = 26 - rnd.Next(Settings.MinMinutesSpent, Settings.MaxMinutesSpent + 1);

                    ConsoleExtensions.WriteLine("TEST COMPLETE!!!", ConsoleColor.Green);
                    ConsoleExtensions.WriteLine($"Waiting down to {minutestowaitfor} minutes left before submitting...", ConsoleColor.Cyan);
                    while (true)
                    {
                        try
                        {
                            string minutesleft = driver.FindElement(By.ClassName("minutes")).Text;
                            string secondsleft = driver.FindElement(By.ClassName("seconds")).Text;
                            ConsoleExtensions.WriteLine($"{minutesleft}:{secondsleft}", ConsoleColor.Magenta);
                            if (minutesleft == minutestowaitfor.ToString())
                                break;
                        }
                        catch { }
                        
                        Thread.Sleep(1000);
                    }
                }

                driver.FindElement(By.Id("next")).Click();

                try { new WebDriverWait(driver, waittime).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete")); } catch { Thread.Sleep(5000); }
            }

            ConsoleExtensions.WriteLine("Test has been submitted!", ConsoleColor.Green);
            ConsoleExtensions.WriteLine("Enjoy :)", ConsoleColor.Cyan);

            Console.ReadLine();
            goto restart;
        }
    }
}