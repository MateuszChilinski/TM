using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELMO2TensorFlow
{
    class Tensor
    {
        public string Name { get; set; }
        public double[] Vector { get; set; }

        public Tensor()
        {

        }
        public Tensor(string name, double[] vector)
        {
            Name = name;
            Vector = vector;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<Tensor> tensors = new List<Tensor>();
            foreach (string line in File.ReadAllLines("leekseek.txt"))
            {
                if (line == "") continue;
                var lineSplit = line.Split('	');
                var name = lineSplit[0];
                if(name.Contains(".") || name.Contains("�") || name.Contains(":") || name.Contains(",")) continue;
                double[] vector = Array.ConvertAll(lineSplit.Skip(1).ToArray(), double.Parse);
                tensors.Add(new Tensor(name, vector));
            }
            List<Tensor> uniqueTensors = new List<Tensor>();
            foreach (var group in tensors.GroupBy(x => x.Name))
            {
                Tensor t = new Tensor();
                t.Name = group.Key;
                t.Vector = new double[group.FirstOrDefault().Vector.Length];
                foreach (var item in group)
                {
                    for (int i = 0; i < item.Vector.Length; i++)
                    {
                        t.Vector[i] += item.Vector[i] / group.Count();
                    }
                }
                uniqueTensors.Add(t);
            }

            tensors = uniqueTensors; // delete if not avg

            if (File.Exists("vectors.tsv"))
            {
                File.Delete("vectors.tsv");
            }

            // Create a new file     
            using (StreamWriter sw = File.CreateText("vectors.tsv"))
            {
                foreach (double[] vector in tensors.Select(x => x.Vector))
                {
                    string line = "";
                    foreach (var scalar in vector)
                        line += scalar + "\t";
                    line = line.Substring(0, line.Length - 2);
                    sw.WriteLine(line);
                }
            }
            if (File.Exists("labels.tsv"))
            {
                File.Delete("labels.tsv");
            }

            // Create a new file     
            using (StreamWriter sw = File.CreateText("labels.tsv"))
            {
                foreach (string name in tensors.Select(x => x.Name))
                {
                    string line = name;
                    sw.WriteLine(line);
                }
            }
        }
    }
}
