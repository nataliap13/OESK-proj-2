using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
//using System.Management;

namespace OESK
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MD5 md5Hash = MD5.Create();
        private SHA1 sha1Hash = SHA1.Create();
        private SHA256 sha256Hash = SHA256.Create();
        private MySQLiteDbContext conn = new MySQLiteDbContext();
        public MainWindow()
        { InitializeComponent(); }

        private string buildHashString(byte[] data)
        {
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            { sBuilder.Append(data[i].ToString("x2")); }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        private string GetMd5Hash(ref string input, out TimeSpan timeSpan)
        {
            #region set priority
            //use the first Core/Processor for the test
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);

            //prevent "Normal" Processes from interrupting Threads
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            //prevent "Normal" Threads from interrupting this thread
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
            #endregion

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            timeSpan = stopWatch.Elapsed;
            return buildHashString(data);
        }
        private string GetSHA1Hash(ref string input, out TimeSpan timeSpan)
        {
            #region set priority
            //use the first Core/Processor for the test
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);

            //prevent "Normal" Processes from interrupting Threads
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            //prevent "Normal" Threads from interrupting this thread
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
            #endregion

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            timeSpan = stopWatch.Elapsed;
            return buildHashString(data);
        }
        private string GetSHA256Hash(ref string input, out TimeSpan timeSpan)
        {
            #region set priority
            //use the first Core/Processor for the test
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);

            //prevent "Normal" Processes from interrupting Threads
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            //prevent "Normal" Threads from interrupting this thread
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
            #endregion

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            timeSpan = stopWatch.Elapsed;
            return buildHashString(data);
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var begin = DateTime.Now;
            TimeSpan MD5Time;
            TimeSpan SHA1Time;
            TimeSpan SHA256Time;
            var text = UserTxtBox.Text;
            var IDText = SearchForTextInDatabaseAddIfNotExists(text);

            string hash = GetMd5Hash(ref text, out MD5Time);
            MD5TxtBlockHash.Text = hash;
            MD5TxtBlockTime.Text = ((int)MD5Time.TotalSeconds).ToString() + ","
                + String.Format("{0:fffffff}", MD5Time); ;
            //MD5TxtBlockTime.Text = String.Format("{0:mm\\:ss\\:fffffff}", timeOfCalculation);

            hash = GetSHA1Hash(ref text, out SHA1Time);
            SHA1TxtBlockHash.Text = hash;
            SHA1TxtBlockTime.Text = ((int)SHA1Time.TotalSeconds).ToString() + ","
                + String.Format("{0:fffffff}", SHA1Time);
            //SHA1TxtBlockTime.Text = String.Format("{0:mm\\:ss\\:fffffff}", timeOfCalculation);

            hash = GetSHA256Hash(ref text, out SHA256Time);
            SHA256TxtBlockHash.Text = hash;
            SHA256TxtBlockTime.Text = ((int)SHA256Time.TotalSeconds).ToString() + ","
                + String.Format("{0:fffffff}", SHA256Time);
            //SHA256TxtBlockTime.Text = String.Format("{0:mm\\:ss\\:fffffff}", timeOfCalculation);
            SaveTestToDatabase(ref IDText, ref MD5Time, ref SHA1Time, ref SHA256Time);
            TxtBlockFullTime.Text = (DateTime.Now - begin).ToString();
        }

        private void SaveTestToDatabase(ref int IDText, ref TimeSpan MD5Time, ref TimeSpan SHA1Time, ref TimeSpan SHA256Time)
        {
            var entryTestResult = new TableTestResult();
            entryTestResult.IDText = IDText;
            entryTestResult.MD5CalculationTime = ((int)MD5Time.TotalSeconds).ToString() + "," + String.Format("{0:fffffff}", MD5Time);
            entryTestResult.SHA1CalculationTime = ((int)SHA1Time.TotalSeconds).ToString() + "," + String.Format("{0:fffffff}", SHA1Time);
            entryTestResult.SHA256CalculationTime = ((int)SHA256Time.TotalSeconds).ToString() + "," + String.Format("{0:fffffff}", SHA256Time);
            entryTestResult = conn.TableTestResult.Add(entryTestResult);
            try
            { conn.SaveChanges(); }
            catch (Exception e)
            { MessageBox.Show("Nie mozna zapisac do db: " + e.Message); }
        }

        private int SearchForTextInDatabaseAddIfNotExists(string text)
        {
            //search for this text in database
            var listOfTexts = conn.TableText.Where(x => x.Text == text).ToList();
            var IDText = 0;
            if (listOfTexts.Count() == 0)
            {
                var entryText = new TableText();
                entryText.Text = text;
                entryText = conn.TableText.Add(entryText);
                try
                { conn.SaveChanges(); }
                catch (Exception ex)
                { MessageBox.Show("Nie mozna zapisac do db: " + ex.Message); }
                IDText = entryText.IDText;
            }
            else { IDText = listOfTexts.First().IDText; }
            return IDText;
        }

        private void BtnStartStandardTest_Click(object sender, RoutedEventArgs e)
        {
            var begin = DateTime.Now;
            try
            {
                for (int i = 0; i < 7; i++)
                {
                    //text += new StringBuilder(text.Length * 9).Insert(0, text, 9).ToString();
                    //text = string.Empty;
                    var text = new String('A', Convert.ToInt32(Math.Pow(10, i)));
                    var IDText = SearchForTextInDatabaseAddIfNotExists(text);

                    for (int j = 0; j < 100; j++)
                    {
                        TimeSpan MD5Time;
                        TimeSpan SHA1Time;
                        TimeSpan SHA256Time;
                        GetMd5Hash(ref text, out MD5Time);
                        GetSHA1Hash(ref text, out SHA1Time);
                        GetSHA256Hash(ref text, out SHA256Time);
                        SaveTestToDatabase(ref IDText, ref MD5Time, ref SHA1Time, ref SHA256Time);
                    }
                }
                TxtBlockFullTime.Text = (DateTime.Now - begin).ToString();
                MessageBox.Show("Test zakończony poprawnie.");
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); MessageBox.Show(ex.InnerException.Message); }
        }
    }
}
