using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
