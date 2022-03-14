using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FuckDrive4You
{
    public class AnswersLoader
    {
        public static string answerspath = Environment.CurrentDirectory + @"\answers";
        public static List<Answer> LoadedAnswers = new List<Answer>();

        public static void Setup()
        {
            if (!Directory.Exists(answerspath))
                Directory.CreateDirectory(answerspath);
        }

        public static bool LoadAnswersFromID(string id)
        {
            if (!File.Exists($"{answerspath}\\{id}.txt"))
            {
                ConsoleExtensions.WriteLine($"{answerspath}\\{id}.txt is missing!!!", ConsoleColor.Red);
                return false;
            }

            string content = File.ReadAllText($"{answerspath}\\{id}.txt");
            foreach (string line in content.Split('\n'))
            {
                try
                {
                    int nr = Convert.ToInt32(line.Split(':')[0]);
                    Answer answer = new Answer(nr);

                    string ksvars = line.Split(':')[1];
                    try { answer.ksvar1 = Convert.ToInt32(ksvars.Substring(0, 1)); } catch { }
                    try { answer.ksvar2 = Convert.ToInt32(ksvars.Substring(1, 1)); } catch { }
                    try { answer.ksvar3 = Convert.ToInt32(ksvars.Substring(2, 1)); } catch { }
                    try { answer.ksvar4 = Convert.ToInt32(ksvars.Substring(3, 1)); } catch { }

                    LoadedAnswers.Add(answer);
                }
                catch
                {
                    ConsoleExtensions.WriteLine($"line: '{line}' <- is wrongly formatted!!!", ConsoleColor.Red);
                }
            }

            return true;
        }

        public static Answer GetAnswer(int nr) => LoadedAnswers.Find(x => x.nr == nr);

        public static bool DumpAnswers(int ResultID)
        {
            LoadedAnswers.Clear();

            string url = "https://undervisning.drive4you.dk/ElevProveResult/MyElevProveResultFromStat?ID=" + ResultID;

            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("user-agent", "Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
                // we cant do webclient, we need to open new tab to dump
                string html = wc.DownloadString(url);
                Console.WriteLine(html);
                string[] answerids = html.Split(new[] { "onclick=\"getUrl('elevproveresultitem/elevproveresultitem?id=" }, StringSplitOptions.None);
                int i = 1;
                foreach (string id in answerids)
                {
                    try
                    {
                        Answer answer = new Answer(i);

                        string answerhtml = wc.DownloadString("https://undervisning.drive4you.dk/ElevProveResult/MyElevProveResultFromStat?ID=" + id);
                        string[] answers = answerhtml.Split(new[] { "Korrekt svar : " }, StringSplitOptions.None);
                        int ii = 1;
                        foreach (string questionanswer in answers)
                        {
                            try
                            {
                                string ksvar = questionanswer.Split(new[] { ". Du svarede ikke</b></td>" }, StringSplitOptions.None)[0];
                                switch (ii)
                                {
                                    case 1: answer.ksvar1 = ksvar == "Ja" ? 1 : 0; break;
                                    case 2: answer.ksvar2 = ksvar == "Ja" ? 1 : 0; break;
                                    case 3: answer.ksvar3 = ksvar == "Ja" ? 1 : 0; break;
                                    case 4: answer.ksvar4 = ksvar == "Ja" ? 1 : 0; break;
                                }
                                ii++;

                            }
                            catch { }
                        }

                        LoadedAnswers.Add(answer);
                        i++;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteLine(ex.ToString(), ConsoleColor.Red);
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