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
        static public List<Word> LoadWords()
        {
            using (StreamReader r1 = new StreamReader("labels.tsv"))
            {
                using(StreamReader r2 = new StreamReader("vectors.tsv"))
                {
                    var words = new List<Word>();
                    string name;
                    while((name = r1.ReadLine()) != null)
                    {
                        Word w = new Word();
                        w.Name = name;
                        var vecString = r2.ReadLine();
                        var vecArray = vecString.Split('	');
                        double[] vec = new double[vecArray.Length];
                        int i = 0;
                        foreach (var scalar in vecArray)
                        {
                            vec[i++] = Double.Parse(scalar);
                        }
                        w.Vec = vec;
                        words.Add(w);
                    }
                    return words;
                }
            }
            return null;
        }
        static void Main(string[] args)
        {
            var wordsVecs = LoadWords();
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
                    visit.Text = visit.Text.Replace(".", "");
                    visit.Text = visit.Text.Replace(":", "");
                    visit.Text = visit.Text.Replace(",", "");
                    visit.Text = visit.Text.Replace("(", "");
                    visit.Text = visit.Text.Replace(")", "");
                visit.Text = visit.Text.Replace("¶", " ");
                visit.Text = visit.Text.Replace("-", " ");
                visit.Text = visit.Text.Replace("  ", " ");
                }
            Dictionary<Visit, double[]> visitsEmb = new Dictionary<Visit, double[]>();
                foreach(var visit in visits_parsed)
                {
                    var words = visit.Text.Split(' ');
                    var visitVec = new double[wordsVecs[0].Vec.Length];
                    int consideredWords = 0;
                    foreach (var word in words)
                    {
                        if (wordsVecs.Count(x => x.Name == word) > 0)
                        {
                            consideredWords++;
                            var vec = wordsVecs.FirstOrDefault(x => x.Name == word).Vec;
                            for(int i = 0; i < visitVec.Length; i++)
                            {
                                visitVec[i] += vec[i];
                            }
                        }
                    }
                    if (consideredWords == 0) continue;
                    for (int i = 0; i < visitVec.Length; i++)
                    {
                        visitVec[i] /= consideredWords;
                    }
                    visitsEmb.Add(visit, visitVec);
                }

            var vectors = File.AppendText("vectors_visits.tsv");
            var labels = File.AppendText("labels_visits.tsv");
            foreach (var entry in visitsEmb)
            {
                if (entry.Value[0] == 0 && entry.Value[1] == 0) continue;
                string line = "";
                foreach (var scalar in entry.Value)
                    line += scalar + "\t";
                line = line.Substring(0, line.Length - 2);
                vectors.WriteLine(line);
                labels.WriteLine(entry.Key.Code + "\t" + entry.Key.Text);
            }
            vectors.Close();
            labels.Close();
            var lines = System.IO.File.ReadAllLines("vectors_visits.tsv");
            System.IO.File.WriteAllLines("vectors_visits.tsv", lines.Take(lines.Length - 1).ToArray());
            return;
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
    public class Word
    {
        public double[] Vec;
        public string Name;
    }
}
