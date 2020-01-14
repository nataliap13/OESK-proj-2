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
using System.Globalization;
using System.Threading;

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

        private void ShowChart(Dictionary<double, TableTest> dict)
        {
            try
            {
                var list = dict.OrderBy(x => x.Key).ToList();
                var max = CalculatePoints(list.Last().Value.NumberOfIterations, list.Last().Value.FullTime);
                //Title.Text = "Ranking";

                //tak sam dla Col2 Col3 itd...
                Col1.MaxValue = max;
                Col1.Value = CalculatePoints(list[0].Value.NumberOfIterations, list[0].Value.FullTime);
                //chart.Col1.Color = Brushes.Green;
                Col1.Color = Brushes.CadetBlue;

                Col2.MaxValue = max;
                Col2.Value = CalculatePoints(list[1].Value.NumberOfIterations, list[1].Value.FullTime);
                Col2.Color = Brushes.Coral;

                Col3.MaxValue = max;
                Col3.Value = CalculatePoints(list[2].Value.NumberOfIterations, list[2].Value.FullTime);
                //chart.Col3.Color = Brushes.Azure;
                Col3.Color = Brushes.BurlyWood;

                Col4.MaxValue = max;
                Col4.Value = CalculatePoints(list[3].Value.NumberOfIterations, list[3].Value.FullTime);
                Col4.Color = Brushes.Chocolate;

                Col5.MaxValue = max;
                Col5.Value = CalculatePoints(list[4].Value.NumberOfIterations, list[4].Value.FullTime);
                Col5.Color = Brushes.CornflowerBlue;
            }
            catch (Exception e)
            { }
        }
        /*
        private void ShowChart(Dictionary<double, TableTest> dict)
        {
            try
            {
                var list = dict.OrderBy(x => x.Key).ToList();
                var max = CalculatePoints(list.Last().Value.NumberOfIterations, list.Last().Value.FullTime);
                Title.Text = "Ranking";

                //tak sam dla Col2 Col3 itd...
                chart.Col1.MaxValue = max;
                chart.Col1.Value = CalculatePoints(list[0].Value.NumberOfIterations, list[0].Value.FullTime);
                //chart.Col1.Color = Brushes.Green;
                chart.Col1.Color = Brushes.CadetBlue;

                chart.Col2.MaxValue = max;
                chart.Col2.Value = CalculatePoints(list[1].Value.NumberOfIterations, list[1].Value.FullTime);
                chart.Col2.Color = Brushes.Coral;

                chart.Col3.MaxValue = max;
                chart.Col3.Value = CalculatePoints(list[2].Value.NumberOfIterations, list[2].Value.FullTime);
                //chart.Col3.Color = Brushes.Azure;
                chart.Col3.Color = Brushes.BurlyWood;

                chart.Col4.MaxValue = max;
                chart.Col4.Value = CalculatePoints(list[3].Value.NumberOfIterations, list[3].Value.FullTime);
                chart.Col4.Color = Brushes.Chocolate;

                chart.Col5.MaxValue = max;
                chart.Col5.Value = CalculatePoints(list[4].Value.NumberOfIterations, list[4].Value.FullTime);
                chart.Col5.Color = Brushes.CornflowerBlue;
            }
            catch (Exception e)
            { }

            chart.Show();
        }
        */
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
            //updateListView
            var function = CmbBxFunction.SelectedItem.ToString();
            var IDFunction = SearchForIDFunctionInDatabaseAddIfNotExist(function);
            var textLength = 100;
            var text = new String('A', textLength);
            int IDText = SearchForIDTextInDatabaseAddIfNotExist(text);

            var foundObjects = new Dictionary<double, TableTest>();
            DownloadAllDBResults(IDFunction, IDText, 0, out foundObjects, out int temp);
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

        private int SaveTestToDatabase(int IDPC, int IDFunction, int IDText, TableCalcParams tableCalcParams)
        {
            try
            {
                var entityTest = new TableTest();
                entityTest.IDPC = IDPC;
                entityTest.IDFunction = IDFunction;
                entityTest.IDTest = entityTest.IDTest;
                entityTest.IDText = IDText;
                entityTest.NumberOfIterations = tableCalcParams.NumberOfIterations;
                entityTest.FullTime = tableCalcParams.TestTimeInSeconds;
                entityTest = conn.TableTest.Add(entityTest);
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
            int IDTest = 0;
            var textLength = 100;
            var text = new String('A', textLength);
            int IDText = SearchForIDTextInDatabaseAddIfNotExist(text);
            int numberOfIterations = 100000;

            var listOfTimes = new List<TimeSpan>();
            try
            {
                for (int i = 0; i < 3; i++)
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
                TxtBlockPoints.Text = CalculatePoints(numberOfIterations, bestTime).ToString() + " pkt";

                var foundObjects = new Dictionary<double, TableTest>();
                DownloadAllDBResults(IDFunction, IDText, IDTest, out foundObjects, out int temp);
                ShowChart(foundObjects);
            }
            catch (Exception ex)
            { MessageBox.Show("Error: " + ex.Message); MessageBox.Show(ex.InnerException.Message); }
        }

        private double CalculatePoints(int numberOfIterations, TimeSpan time)
        { return Math.Round((Convert.ToDouble(numberOfIterations) / time.TotalSeconds), 0); }
        private double CalculatePoints(int numberOfIterations, string timeAsString)
        {
            var time = ParseTimeFromString(timeAsString);
            return Math.Round((Convert.ToDouble(numberOfIterations) / time.TotalSeconds), 0);
        }

        private void DownloadAllDBResults(int IDFunction, int IDText, int IDTest,
            out Dictionary<double, TableTest> foundObjects, out int numberOfAll)
        {
            var tab = conn.TableTest.Where(x => x.IDFunction == IDFunction)
                .Where(x => x.IDText == IDText)
                .OrderBy(x => x.FullTime).ToList();

            numberOfAll = tab.Count();

            //var foundObjects = new Dictionary<double, TableTest>();
            foundObjects = new Dictionary<double, TableTest>();
            var searchingPositions = new List<int>();
            searchingPositions.Add(Convert.ToInt32(numberOfAll * 3 / 4));//in 3/4 from top
            searchingPositions.Add(Convert.ToInt32(numberOfAll / 2));//in 1/2 from top
            searchingPositions.Add(Convert.ToInt32(numberOfAll / 4));//in 1/4 from top
            searchingPositions.Add(1);//the best

            ///Adding numeration to listView
            List<object> lista = new List<object>();
            foreach (var item in tab)
            {
                var index = tab.FindIndex
                    (x => x.IDTest == item.IDTest);

                var time = ParseTimeFromString(item.FullTime);
                var points = CalculatePoints(item.NumberOfIterations, time);
                var newObj = new
                {
                    Index = index + 1,
                    Points = points,
                    TableTest = item
                };
                lista.Add(newObj);

                int percent = Convert.ToInt32(Math.Round((numberOfAll - index) * 100.0 / numberOfAll, 0));//100%

                //find user position in ranking
                if (newObj.TableTest.IDTest == IDTest)
                {
                    TxtBlockScore.Text = newObj.Index.ToString();
                    foundObjects.Add(percent, newObj.TableTest);

                    //if (searchingPositions.Contains(newObj.Index))//do not duplicate same position
                    //{ searchingPositions.Remove(newObj.Index); }
                }
                else
                {
                    if (searchingPositions.Contains(newObj.Index))
                    {
                        foundObjects.Add(percent, newObj.TableTest);
                        searchingPositions.Remove(newObj.Index);
                    }
                }
            }
            ListViewMain.ItemsSource = lista;
            return;
        }

        private TimeSpan ParseTimeFromString(string timeAsString)
        {
            var splited = timeAsString.Split(new char[] { ',', '.' });
            Thread.CurrentThread.CurrentCulture = new CultureInfo("hr-HR");
            return TimeSpan.Parse("0:0:0" + splited[0].ToString() + "," + splited[1].ToString());
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
