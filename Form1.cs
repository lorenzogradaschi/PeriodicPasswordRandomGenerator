using System;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace PseudoRandomGenerator
{
    public partial class Form1 : Form
    {
        private long seed;
        public Form1()
        {
            InitializeComponent();

           
            seed = DateTime.Now.Ticks;
        }

        static int TimeSeed()
        {
            return (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }

        

        public int EntropySeed()
        {

            return (int)seed;

        }

        
        static double AngularFunction(double x, int t)
        {
            double a = 1.61803398875;
            double b = 1 - a;
            double y = (x + t) % 1;
            y = (a * y + b) % 1;
            return y;
        }
       
        static double SawFunction(double x, int t)
        {
            double y = (x + t) % 1;
            if (y < 0.5)
            {
                y = 2 * y;
            }
            else
            {
                y = 2 * (1 - y);
            }
            return y;
        }

   
        static List<int> GeneratePseudoRandomNumbers(Func<double, int, double> algorithm, Func<int> seedMethod, double initialValue, int count)
        {
            // Ustvarm seme
            int t = seedMethod();
            double x = initialValue;

            List<int> numbers = new List<int>();
            
            for (int i = 0; i < count; i++)
            {
                
                double y = algorithm(x, t);
               
                t = seedMethod();
                x = y;
                
                numbers.Add((int)(y * 10) + 1); 
            }
            return numbers;
        }

        
        static int FindRecurrencePeriod(List<int> numbers)
        {
            
            Dictionary<int, int> firstOccurrence = new Dictionary<int, int>();
            for (int i = 0; i < numbers.Count; i++)
            {
                if (!firstOccurrence.ContainsKey(numbers[i]))
                {
                    firstOccurrence[numbers[i]] = i;
                }
            }
           
            int maxPeriod = numbers.Count / 2;
            for (int period = 1; period <= maxPeriod; period++)
            {
                bool isPeriodic = true;
                for (int i = 0; i < numbers.Count - period; i++)
                {
                    if (numbers[i] != numbers[i + period])
                    {
                        isPeriodic = false;
                        break;
                    }
                }
                if (isPeriodic)
                {
                    return period;
                }
            }
            return -1;
        }

        
        static void WriteNumbersToFile(List<int> numbers, string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (int number in numbers)
                {
                    writer.WriteLine(number);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Func<double, int, double> algorithm;

            if (radioButton1.Checked)
            {
                algorithm = AngularFunction;
            }
            else if (radioButton2.Checked)
            {
                algorithm = SawFunction;
            }
            else
            {
                MessageBox.Show("Algorithm not selected");
                return;
            }
            Func<int> seedMethod;

            if (radioButton3.Checked)
            {
                seedMethod = TimeSeed;
            }
            else if (radioButton4.Checked)
            {
                seedMethod = EntropySeed;
            }
            else
            {
                MessageBox.Show("No method is selected");
                return;
            }
            double initialValue;
            if (Double.TryParse(textBox2.Text, out initialValue))
            {
                if (initialValue > 1 || initialValue < 0)
                {
                    MessageBox.Show("The initial value must be between 0 and 1");
                    return;
                }
            }
            else
            {
                MessageBox.Show("The initial value must be between 0 and 1");
                return;
            }
            int count = int.Parse(textBox3.Text);
            if (count < 0)
            {
                MessageBox.Show("Invalid set size");
                return;
            }

            List<int> numbers = GeneratePseudoRandomNumbers(algorithm, seedMethod, initialValue, count);

            
            int period = FindRecurrencePeriod(numbers);
            if (period == -1)
            {
                MessageBox.Show("The sequence is aperiodically");
            }
            else
            {
                MessageBox.Show("The recurrence period is " + period + ".");
            }

            
            string filename = textBox1.Text;
            if (!string.IsNullOrEmpty(filename))
            {
                WriteNumbersToFile(numbers, filename);
            }

            
            Dictionary<int, int> frequencyDistribution = new Dictionary<int, int>();

           
            foreach (int number in numbers)
            {
                if (frequencyDistribution.ContainsKey(number))
                {
                    frequencyDistribution[number]++;
                }
                else
                {
                    frequencyDistribution.Add(number, 1);
                }
            }

            
            chart.Size = new Size(600, 400);

           
            ChartArea chartArea = new ChartArea();
            chartArea.AxisX.Title = "Number";
            chartArea.AxisY.Title = "Frequency";
            chart.ChartAreas.Add(chartArea);

            
            Series series = new Series();
            series.ChartType = SeriesChartType.Column;
            foreach (int number in frequencyDistribution.Keys)
            {
                series.Points.AddXY(number, frequencyDistribution[number]);
            }
            chart.Series.Add(series);

           
            chart.SaveImage("frequency_distribution.png", ChartImageFormat.Png);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
