using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FuckDrive4You
{
    public class AnswersLoader
    {
        public static List<Answer> LoadedAnswers = new List<Answer>();

        public static bool DumpAnswers(IWebDriver driver, int ResultID)
        {
            LoadedAnswers.Clear();

            string url = "https://undervisning.drive4you.dk/ElevProveResult/MyElevProveResultFromStat?ID=" + ResultID;

            try
            {
                TimeSpan waittime = driver.Manage().Timeouts().ImplicitWait;

                // we cant do webclient, we need to open new tab to dump
                ((IJavaScriptExecutor)driver).ExecuteScript("window.open();");
                driver.SwitchTo().Window(driver.WindowHandles.Last());
                driver.Navigate().GoToUrl(url);

                try { new WebDriverWait(driver, waittime).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete")); } catch { Thread.Sleep(5000); }

                string[] answerids = driver.PageSource.Split(new[] { "onclick=\"getUrl('elevproveresultitem/elevproveresultitem?id=" }, StringSplitOptions.None);
                int i = 1;
                foreach (string question in answerids)
                {
                    try
                    {
                        string id = question.Split(new[] { "')\">" }, StringSplitOptions.None)[0];
                        if (IsDigitsOnly(id))
                        {
                            Answer answer = new Answer(i);
                            driver.Navigate().GoToUrl("https://undervisning.drive4you.dk/elevproveresultitem/elevproveresultitem?id=" + id);

                            try { new WebDriverWait(driver, waittime).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete")); } catch { Thread.Sleep(5000); }

                            string[] answers = driver.PageSource.Split(new[] { "Korrekt svar : " }, StringSplitOptions.None);
                            int ii = 0;
                            foreach (string questionanswer in answers)
                            {
                                try
                                {
                                    string ksvar = questionanswer.Split(new[] { ". Du svarede" }, StringSplitOptions.None)[0];
                                    switch (ii)
                                    {
                                        case 1: answer.ksvar1 = ksvar == "Ja" ? 1 : 0; Console.WriteLine("ksvar1 = " + ksvar); break;
                                        case 2: answer.ksvar2 = ksvar == "Ja" ? 1 : 0; Console.WriteLine("ksvar2 = " + ksvar); break;
                                        case 3: answer.ksvar3 = ksvar == "Ja" ? 1 : 0; Console.WriteLine("ksvar3 = " + ksvar); break;
                                        case 4: answer.ksvar4 = ksvar == "Ja" ? 1 : 0; Console.WriteLine("ksvar4 = " + ksvar); break;
                                    }
                                    ii++;

                                }
                                catch { }
                            }
                            Console.WriteLine("------------");

                            LoadedAnswers.Add(answer);
                            i++;
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteLine(ex.ToString(), ConsoleColor.Red);
            }

            driver.SwitchTo().Window(driver.WindowHandles.First());

            return true;
        }

        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }

    public class Answer
    {
        public int nr { get; set; }
        public int ksvar1 { get; set; }
        public int ksvar2 { get; set; }
        public int ksvar3 { get; set; }
        public int ksvar4 { get; set; }

        public Answer(int _nr, int _ksvar1 = -1, int _ksvar2 = -1, int _ksvar3 = -1, int _ksvar4 = -1)
        {
            nr = _nr;
            ksvar1 = _ksvar1;
            ksvar2 = _ksvar2;
            ksvar3 = _ksvar3;
            ksvar4 = _ksvar4;
        }
    }
}