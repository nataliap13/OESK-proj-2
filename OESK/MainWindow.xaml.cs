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
        private string CPUName;
        private int RAMCapacity = 0;
        private int RAMFrequency = 0;
        private int IDCPU = 0;
        private int IDRAM = 0;
        private int IDPC = 0;

        public MainWindow()
        {
            InitializeComponent();

            ReadPCConfiguration(out CPUName, out RAMCapacity, out RAMFrequency);
            IDCPU = SearchForIDCPUInDatabaseAddIfNotExist(CPUName);
            IDRAM = SearchForIDRAMInDatabaseAddIfNotExist(RAMCapacity, RAMFrequency);
            IDPC = SearchForIDPCInDatabaseAddIfNotExist(IDCPU, IDRAM);

        }
        #region CmbBxFunction
        private void CmbBxFunction_Loaded(object sender, RoutedEventArgs e)
        {
            var functionsList = conn.TableFunction.Select(x => x).OrderBy(x => x.IDFunction).ToList();
            List<string> data = new List<string>();
            foreach (var item in functionsList)
            { data.Add(item.FunctionName); }
            CmbBxFunction.ItemsSource = data;
            CmbBxFunction.SelectedIndex = 0;
        }

        private void CmbBxFunction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        #endregion

        private void ReadPCConfiguration(out string CPUName, out int RAMCapacity, out int RAMFrequency)
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

        private void GetMd5Hashes(ref string input, out TimeSpan timeSpan, int numberOfIterations)
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
            for (int i = 0; i < numberOfIterations; i++)
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            timeSpan = stopWatch.Elapsed;
            return;
        }
        private void GetSHA1Hashes(ref string input, out TimeSpan timeSpan, int numberOfIterations)
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
            for (int i = 0; i < numberOfIterations; i++)
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            timeSpan = stopWatch.Elapsed;
            return;
        }
        private void GetSHA256Hashes(ref string input, out TimeSpan timeSpan, int numberOfIterations)
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
            for (int i = 0; i < numberOfIterations; i++)
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            timeSpan = stopWatch.Elapsed;
            return;
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

        private int SaveTestToDatabase(int IDPC, int IDFunction, int IDText, TableCalcParams tableCalcParams)
        {
            try
            {
                var entityTest = new TableTest();
                entityTest.IDPC = IDPC;
                entityTest.IDFunction = IDFunction;
                entityTest = conn.TableTest.Add(entityTest);
                conn.SaveChanges();

                var entityTestResult = new TableTestResult();
                entityTestResult.IDTest = entityTest.IDTest;
                entityTestResult.IDText = IDText;
                entityTestResult.NumberOfIterations = tableCalcParams.NumberOfIterations;
                entityTestResult.FullTime = tableCalcParams.TestTimeInSeconds;
                conn.TableTestResult.Add(entityTestResult);
                conn.SaveChanges();
                return entityTest.IDTest;
            }
            catch (Exception e)
            { MessageBox.Show("Error: " + e.Message); }
            return -1;
        }

        private int SearchForIDFunctionInDatabaseAddIfNotExist(string functionName)
        {
            //search for this text in database
            var listOfEntities = conn.TableFunction.Where(x => x.FunctionName == functionName).ToList();
            var ID = 0;
            if (listOfEntities.Count() == 0)
            {
                var entity = new TableFunction();
                entity.FunctionName = functionName;
                entity = conn.TableFunction.Add(entity);
                try
                { conn.SaveChanges(); }
                catch (Exception ex)
                { MessageBox.Show("Nie mozna zapisac do db: " + ex.Message); }
                ID = entity.IDFunction;
            }
            else { ID = listOfEntities.First().IDFunction; }
            return ID;
        }
        private int SearchForIDTextInDatabaseAddIfNotExist(string text)
        {
            //search for this text in database
            var listOfEntities = conn.TableText.Where(x => x.Text == text).ToList();
            var ID = 0;
            if (listOfEntities.Count() == 0)
            {
                var entity = new TableText();
                entity.Text = text;
                entity = conn.TableText.Add(entity);
                try
                { conn.SaveChanges(); }
                catch (Exception ex)
                { MessageBox.Show("Nie mozna zapisac do db: " + ex.Message); }
                ID = entity.IDText;
            }
            else { ID = listOfEntities.First().IDText; }
            return ID;
        }
        private int SearchForIDCPUInDatabaseAddIfNotExist(string CPUName)
        {
            //search for this text in database
            var listOfEntities = conn.TableCPU.Where(x => x.CPUName == CPUName).ToList();
            var ID = 0;
            if (listOfEntities.Count() == 0)
            {
                var entity = new TableCPU();
                entity.CPUName = CPUName;
                entity = conn.TableCPU.Add(entity);
                try
                { conn.SaveChanges(); }
                catch (Exception ex)
                { MessageBox.Show("Nie mozna zapisac do db: " + ex.Message); }
                ID = entity.IDCPU;
            }
            else { ID = listOfEntities.First().IDCPU; }
            return ID;
        }
        private int SearchForIDRAMInDatabaseAddIfNotExist(int RAMCapacity, int RAMFrequency)
        {
            //search for this text in database
            var listOfEntities = conn.TableRAM.Where(x => x.RAMCapacity == RAMCapacity).Where(x => x.RAMFrequency == RAMFrequency).ToList();
            var ID = 0;
            if (listOfEntities.Count() == 0)
            {
                var entity = new TableRAM();
                entity.RAMCapacity = RAMCapacity;
                entity.RAMFrequency = RAMFrequency;
                entity = conn.TableRAM.Add(entity);
                try
                { conn.SaveChanges(); }
                catch (Exception ex)
                { MessageBox.Show("Nie mozna zapisac do db: " + ex.Message); }
                ID = entity.IDRAM;
            }
            else { ID = listOfEntities.First().IDRAM; }
            return ID;
        }
        private int SearchForIDPCInDatabaseAddIfNotExist(int IDCPU, int IDRAM)
        {
            //search for this text in database
            var listOfEntities = conn.TablePC.Where(x => x.IDCPU == IDCPU).Where(x => x.IDRAM == IDRAM).ToList();
            var ID = 0;
            if (listOfEntities.Count() == 0)
            {
                var entity = new TablePC();
                entity.IDCPU = IDCPU;
                entity.IDRAM = IDRAM;
                entity = conn.TablePC.Add(entity);
                try
                { conn.SaveChanges(); }
                catch (Exception ex)
                { MessageBox.Show("Nie mozna zapisac do db: " + ex.Message); }
                ID = entity.IDRAM;
            }
            else { ID = listOfEntities.First().IDPC; }
            return ID;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var begin = DateTime.Now;
            var function = CmbBxFunction.SelectionBoxItem.ToString();
            var IDFunction = SearchForIDFunctionInDatabaseAddIfNotExist(function);
            int IDText = 0;
            int IDTest = 0;
            var textLength = 100;
            var text = new String('A', textLength);
            int numberOfIterations = 100000;
            IDText = SearchForIDTextInDatabaseAddIfNotExist(text);
            var listOfTimes = new List<TimeSpan>();
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    TimeSpan CalcTime = new TimeSpan();
                    switch (function)
                    {
                        case "MD5":
                            { GetMd5Hashes(ref text, out CalcTime, numberOfIterations); break; }
                        case "SHA1":
                            { GetSHA1Hashes(ref text, out CalcTime, numberOfIterations); break; }
                        case "SHA256":
                            { GetSHA256Hashes(ref text, out CalcTime, numberOfIterations); break; }
                        default:
                            { MessageBox.Show("Function Error!"); break; }
                    }
                    listOfTimes.Add(CalcTime);
                }
                var bestTime = listOfTimes.Min();
                //Add best result to UI Table/List
                var tableCalcParams = new TableCalcParams(function, textLength, numberOfIterations, bestTime);
                IDTest = SaveTestToDatabase(IDPC, IDFunction, IDText, tableCalcParams);
                TxtBlockFullTime.Text = tableCalcParams.TestTimeInSeconds;
                TxtBlockPoints.Text = CalculatePoints(numberOfIterations, bestTime).ToString() + " pkt";
                //var win2 = new ResultsWindow(IDFunction, IDText);
                //win2.Show();
                DownloadAllDBResults(IDFunction, IDText, IDTest);
            }
            catch (Exception ex)
            { MessageBox.Show("Error: " + ex.Message); MessageBox.Show(ex.InnerException.Message); }
        }

        private double CalculatePoints(int numberOfIterations, TimeSpan time)
        { return Math.Round((Convert.ToDouble(numberOfIterations) / time.TotalSeconds), 0); }

        private void DownloadAllDBResults(int IDFunction, int IDText, int IDTest)
        {
            var TestsAndTestResults = conn.TableTest.Join(conn.TableTestResult,
                TableTest => TableTest.IDTest,
                TableTestResult => TableTestResult.IDTest,
                (tableTest, tableTestResult) =>
                new { TableTest = tableTest, TableTestResult = tableTestResult })
                .Where(joined => joined.TableTest.IDFunction == IDFunction)
                .Where(joined => joined.TableTestResult.IDText == IDText);

            var TestsAndTestResultsAndPCs = TestsAndTestResults.Join(conn.TablePC,
                TestsAndTestResults_ => TestsAndTestResults_.TableTest.IDPC,
                TablePC => TablePC.IDPC,
                (testsAndTestResults, tablePC) =>
                new { TestsAndTestResults = testsAndTestResults, TablePC = tablePC })
                .OrderBy(x => x.TestsAndTestResults.TableTestResult.FullTime)
                .ToList();
            ///Adding numeration to listView
            List<object> lista = new List<object>();
            foreach (var item in TestsAndTestResultsAndPCs)
            {
                var index = TestsAndTestResultsAndPCs.FindIndex
                    (x => x.TestsAndTestResults.TableTestResult.IDTestResult == item.TestsAndTestResults.TableTestResult.IDTestResult);
                var newObj = new { Index = index + 1,
                    TestsAndTestResults = item.TestsAndTestResults, TablePC = item.TablePC, };
                /*
                var newObj = new
                {
                    Index = index + 1,
                    Points = CalculatePoints(item.TestsAndTestResults.TableTestResult.NumberOfIterations, new TimeSpan(item.TestsAndTestResults.TableTestResult.FullTime),
                 TestsAndTestResults = item.TestsAndTestResults, TablePC = item.TablePC, };
                 */

                lista.Add(newObj);

                //find user position in ranking
                if (newObj.TestsAndTestResults.TableTest.IDTest == IDTest)
                { TxtBlockScore.Text = newObj.Index.ToString(); }
            }

            //ListViewMain.ItemsSource = TestsAndTestResultsAndPCs;
            ListViewMain.ItemsSource = lista;
            return;
        }

        private void BtnMyPC_Click(object sender, RoutedEventArgs e)
        {
            string CPUName;
            int RAMCapacity;
            int RAMFrequency;
            ReadPCConfiguration(out CPUName, out RAMCapacity, out RAMFrequency);
            MessageBox.Show("CPU: " + CPUName + "\nRAM Capacity: "
                + RAMCapacity + " GB\nRAM Frequency: " + RAMFrequency + " MHz");
        }
    }
}
