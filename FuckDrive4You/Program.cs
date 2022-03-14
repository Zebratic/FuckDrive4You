using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
                AnswersLoader.Setup();
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
            ConsoleExtensions.WriteLine("SET TEST TO DIGITAL WITH NO SOUND!!!", ConsoleColor.Yellow);
            ConsoleExtensions.WriteLine("READY!!! Start the test, and click enter here!", ConsoleColor.Green);
            Console.ReadLine();

            try
            {
                ConsoleExtensions.WriteLine("Gathering result id, please wait...", ConsoleColor.Cyan);
                int resultid = Convert.ToInt32(driver.FindElement(By.Id("ElevProveResultID")).GetAttribute("value"));
                ConsoleExtensions.WriteLine($"Result id found: {resultid}", ConsoleColor.Green);
                ConsoleExtensions.WriteLine("Dumping answers, please wait...", ConsoleColor.Cyan);
                AnswersLoader.DumpAnswers(resultid);

                foreach (Answer answer in AnswersLoader.LoadedAnswers)
                {
                    Console.WriteLine("nr: " + answer.nr);
                    Console.WriteLine("ksvar1: " + answer.ksvar1);
                    Console.WriteLine("ksvar2: " + answer.ksvar2);
                    Console.WriteLine("ksvar3: " + answer.ksvar3);
                    Console.WriteLine("ksvar4: " + answer.ksvar4);
                    ConsoleExtensions.WriteLine("", ConsoleColor.Magenta);
                }

            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteLine("DUMPING ANSWERS FAILED!!!", ConsoleColor.Red);
                ConsoleExtensions.WriteLine(ex.ToString(), ConsoleColor.Red);
            }









            ConsoleExtensions.WriteLine("Answers dumped!", ConsoleColor.Green);

            ConsoleExtensions.WriteLine("Filling out answers, please wait...", ConsoleColor.Cyan);
            // https://undervisning.drive4you.dk/elevprove/teoriprove?catid=3&serie=1&audio=0&regain=false

            foreach (Answer answer in AnswersLoader.LoadedAnswers)
            {

            }

            Console.ReadLine();
        }
    }
}