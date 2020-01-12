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
//using System.Globalization;

namespace OESK
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        private MySQLiteDbContext conn = new MySQLiteDbContext();

        public ResultsWindow(int IDFunction, int IDText)
        {
            InitializeComponent();
            DownloadAllDBResults(IDFunction, IDText);
        }

        private void DownloadAllDBResults(int IDFunction, int IDText)
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

            ListViewMain.ItemsSource = TestsAndTestResultsAndPCs;
            return;
        }
    }
}
