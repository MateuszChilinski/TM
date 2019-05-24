using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordCloudGen = WordCloud.WordCloud;

namespace TMStats
{
    class Program
    {
        static public List<Visit> LoadJson()
        {
            using (StreamReader r = new StreamReader("visits_1000.txt"))
            {
                string json = r.ReadToEnd();
                var items = JsonConvert.DeserializeObject<List<Visit>>(json);
                return items;
            }
            return null;
        }
        static void Main(string[] args)
        {
            var visits = LoadJson();
            var file = File.AppendText("results.csv");
            visits.Where(x => x.Text.Length > 3).GroupBy(x => x.Code)
                .Select(y => new { Code = y.Key, Count = y.Count() }).OrderByDescending(x => x.Count).ToList()
                .ForEach(x => file.WriteLine(x.Code + "," + x.Count));
            var visits_parsed = visits.Where(x => x.Text.Length > 3);
            var keys = visits.Where(x => x.Text.Length > 3).GroupBy(x => x.Code)
                .Select(y => new {Code = y.Key, Count = y.Count()}).OrderByDescending(x => x.Count).Where(x => x.Count >= 10).Select(x => x.Code);
                Dictionary<string, int> wordFreq = new Dictionary<string, int>();
                foreach (var visit in visits_parsed)
                {
                    visit.Text = visit.Text.Replace(".", " .");
                    visit.Text = visit.Text.Replace(":", "  :");
                    visit.Text = visit.Text.Replace(",", " , ");
                    visit.Text = visit.Text.Replace("(", " ( ");
                    visit.Text = visit.Text.Replace(")", " )");
                    visit.Text = visit.Text.Replace("  ", " ");
                }
                foreach (var visit in visits_parsed)
                {
                    var words = visit.Text.Split(' ');
                    foreach (var word in words)
                    {
                        if (wordFreq.ContainsKey(word))
                            wordFreq[word]++;
                        else
                            wordFreq.Add(word, 1);
                    }
                }
            var vocabulary = File.AppendText("vocabulary");
            var train = File.AppendText("train");

            var ordrred = wordFreq.OrderByDescending(x => x.Value);
            vocabulary.WriteLine("</S>");
            vocabulary.WriteLine("<S>");
            vocabulary.WriteLine("<UNK>");
            foreach (var word in ordrred)
            {
                vocabulary.WriteLine(word.Key);
            }
            foreach (var visit in visits_parsed)
            {
                train.WriteLine(visit.Text);
            }

            //var wc = new WordCloudGen(1200, 1200);
            //List<string> wordsCloud = new List<string>();
            //List<int> wordsFreq = new List<int>();
            //int countAll = 0;
            //foreach (var entity in wordFreq)
            //{
            //    countAll += entity.Value;
            //}

            //List<string> toRemove = new List<string>();
            //foreach (var entity in wordFreq)
            //{
            //    if (entity.Value < countAll * 0.005)
            //    {
            //        toRemove.Add(entity.Key);
            //    }
            //}
            //foreach (var remov in toRemove)
            //{
            //    wordFreq.Remove(remov);
            //}
            //foreach (var entity in wordFreq)
            //{
            //    wordsCloud.Add(entity.Key);
            //    wordsFreq.Add(entity.Value);
            //}
            //Image image = wc.Draw(wordsCloud, wordsFreq);
            //image.Save(code+".png", ImageFormat.Png);

        }
    }
    public class Visit
    {
        public string Text;
        public string Sex;
        public int Age;
        public string Code;
    }
}
