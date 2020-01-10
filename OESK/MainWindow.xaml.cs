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
using System.Management;

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
        {
            InitializeComponent();
        }

        private void ReadPCConfiguration(out string CPUName,
            out int RAMCapacity, out int RAMFrequency)
        {
            CPUName = string.Empty;
            RAMCapacity = 0;
            RAMFrequency = 0;
            try
            {
                //Win32_Processor Name
                //Win32_PhysicalMemory Speed, Capacity suma/1024/1024/1024
                //Win32_MemoryArray EndingAddress +1 /1024/1024
                //https://www.codeguru.com/columns/dotnet/using-c-to-find-out-what-your-computer-is-made-of.html
                //https://docs.microsoft.com/pl-pl/windows/win32/cimwin32prov/computer-system-hardware-classes?redirectedfrom=MSDN
                {
                    ManagementClass myManagementClass = new ManagementClass("Win32_Processor");
                    ManagementObjectCollection myManagementCollection = myManagementClass.GetInstances();
                    //if (myManagementCollection.Count > 1)
                    //{ MessageBox.Show("UWAGA, wiele obiektów dla Win32_Processor"); }
                    foreach (ManagementObject obj in myManagementCollection)
                    {
                        try
                        { CPUName = obj.Properties["Name"].Value.ToString(); }
                        catch (Exception)
                        { }
                    }
                }
                {
                    ManagementClass myManagementClass = new ManagementClass("Win32_PhysicalMemory");
                    ManagementObjectCollection myManagementCollection = myManagementClass.GetInstances();
                    //if (myManagementCollection.Count > 1)
                    //{ MessageBox.Show("UWAGA, wiele obiektów dla Win32_PhysicalMemory"); }
                    foreach (ManagementObject obj in myManagementCollection)
                    {
                        try
                        {
                            RAMFrequency = Convert.ToInt32(obj.Properties["Speed"].Value);
                            if (RAMFrequency > 0)
                            { break; }
                        }
                        catch (Exception)
                        { }
                    }
                }
                {
                    ManagementClass myManagementClass = new ManagementClass("Win32_MemoryArray");
                    ManagementObjectCollection myManagementCollection = myManagementClass.GetInstances();
                    //if (myManagementCollection.Count > 1)
                    //{ MessageBox.Show("UWAGA, wiele obiektów dla Win32_MemoryArray"); }
                    foreach (ManagementObject obj in myManagementCollection)
                    {
                        try
                        {
                            RAMCapacity = Convert.ToInt32((Convert.ToInt64(obj.Properties["EndingAddress"].Value) + 1) / 1024 / 1024);
                        }
                        catch (Exception)
                        { }
                    }
                }
            }
            catch (Exception)
            { }
        }
        /*
        private void Button_Click(object sender, RoutedEventArgs e)
        {//sprawdzenie wszystkich mozliwosci PC.
        //W linkach sa kody ktore nalezy podac podczas tworzenia ManagementClass aby dostać info o PC
            var listOfCalcResults = new List<TableCalcParams>();
            var templist = new List<TimeSpan>();
            templist.Add(new TimeSpan(1));
            try
            {
                //Win32_Processor Name
                //Win32_PhysicalMemory Manufacturer, Part Number, Speed, Capacity suma/1024/1024/1024
                //Win32_MemoryArray Ending address /1024/1024
                //https://www.codeguru.com/columns/dotnet/using-c-to-find-out-what-your-computer-is-made-of.html
                //https://docs.microsoft.com/pl-pl/windows/win32/cimwin32prov/computer-system-hardware-classes?redirectedfrom=MSDN
                ManagementClass myManagementClass = new ManagementClass(pole.Text);
                ManagementObjectCollection myManagementCollection = myManagementClass.GetInstances();
                PropertyDataCollection myProperties = myManagementClass.Properties;
                int i = 0;
                foreach (ManagementObject obj in myManagementCollection)
                {
                    foreach (PropertyData property in myProperties)
                    {
                        try
                        {
                            listOfCalcResults.Add(new TableCalcParams(property.Name, i, templist));
                        }
                        catch (Exception)
                        { }
                        try
                        {
                            listOfCalcResults.Add(new TableCalcParams(obj.Properties[property.Name].Value.ToString(), i, templist));
                        }
                        catch (Exception)
                        { }
                        i++;
                    }
                }

            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
            ListViewMain.ItemsSource = listOfCalcResults;
        }*/

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
        /*
        private void someOldStartFuntion_Click(object sender, RoutedEventArgs e)
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
        }*/

        private void SaveTestToDatabase(ref int IDText, ref TimeSpan MD5Time, ref TimeSpan SHA1Time, ref TimeSpan SHA256Time)
        {
            var entryTestResult = new TableTestResult();
            entryTestResult.IDText = IDText;
            entryTestResult.MD5CalculationTime = TimeSpanConverter.ToSecondsMiliseconds(MD5Time);
            entryTestResult.SHA1CalculationTime = TimeSpanConverter.ToSecondsMiliseconds(SHA1Time);
            entryTestResult.SHA256CalculationTime = TimeSpanConverter.ToSecondsMiliseconds(SHA256Time);
            entryTestResult = conn.TableTestResult.Add(entryTestResult);
            try
            { conn.SaveChanges(); }
            catch (Exception e)
            { MessageBox.Show("Nie mozna zapisac do db: " + e.Message); }
        }

        private int SearchForTextIDInDatabaseAddIfNotExists(string text)
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

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var begin = DateTime.Now;
            var MD5Times = new List<TimeSpan>();
            var SHA1Times = new List<TimeSpan>();
            var SHA256Times = new List<TimeSpan>();
            try
            {
                var listOfCalcResults = new List<TableCalcParams>();
                for (int i = 0; i < 3; i++)
                {
                    //text += new StringBuilder(text.Length * 9).Insert(0, text, 9).ToString();
                    //text = string.Empty;
                    var textLength = Convert.ToInt32(Math.Pow(10, i));
                    var text = new String('A', textLength);
                    var IDText = SearchForTextIDInDatabaseAddIfNotExists(text);
                    TimeSpan MD5Time;
                    TimeSpan SHA1Time;
                    TimeSpan SHA256Time;
                    for (int j = 0; j < 100; j++)
                    {
                        GetMd5Hash(ref text, out MD5Time);
                        GetSHA1Hash(ref text, out SHA1Time);
                        GetSHA256Hash(ref text, out SHA256Time);
                        MD5Times.Add(MD5Time);
                        SHA1Times.Add(SHA1Time);
                        SHA256Times.Add(SHA256Time);
                        //SaveTestToDatabase(ref IDText, ref MD5Time, ref SHA1Time, ref SHA256Time);
                    }
                    listOfCalcResults.Add(new TableCalcParams("MD5", textLength, MD5Times));
                    listOfCalcResults.Add(new TableCalcParams("SHA1", textLength, SHA1Times));
                    listOfCalcResults.Add(new TableCalcParams("SHA256", textLength, SHA256Times));
                }
                ListViewMain.ItemsSource = listOfCalcResults;
                TxtBlockFullTime.Text = (DateTime.Now - begin).ToString();
            }
            catch (Exception ex)
            { MessageBox.Show("Error: " + ex.Message); MessageBox.Show(ex.InnerException.Message); }
        }

        private void BtnMyPC_Click(object sender, RoutedEventArgs e)
        {
            string CPUName;
            int RAMCapacity;
            int RAMFrequency;
            ReadPCConfiguration(out CPUName, out RAMCapacity, out RAMFrequency);
            MessageBox.Show(CPUName + "\n" + RAMCapacity + "\n" + RAMFrequency);
        }
    }
}
