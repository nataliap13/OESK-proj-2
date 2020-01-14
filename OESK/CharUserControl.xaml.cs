using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Shapes;

namespace OESK
{
    /// <summary>
    /// Interaction logic for CharUserControl.xaml
    /// </summary>
    public partial class CharUserControl : UserControl, INotifyPropertyChanged
    {
        private double _value;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if(PropertyChanged !=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                UpdateBarHeight();
                NotifyPropertyChanged("Value");

            }
        }

        private double maxValue;
        public double MaxValue
        {
            get { return maxValue; }
            set
            {
                maxValue = value;
                UpdateBarHeight();
                NotifyPropertyChanged("MaxValue");
            }
        }

        private double barHeight;
        public double BarHeight
        {
            get { return barHeight; }
            private set { barHeight = value; NotifyPropertyChanged("BarHeight"); }
        }

        private Brush color;
        public Brush Color
        {
            get { return color; }
            set { color = value; NotifyPropertyChanged("Color"); }
        }

        private void UpdateBarHeight()
        {
            if(MaxValue > 0)
            {
                var percent = (_value * 100)/maxValue;
                BarHeight = (percent * this.ActualHeight) / 100;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBarHeight();
        }

        public CharUserControl()
        {
            InitializeComponent();
            this.DataContext = this;
            Color = Brushes.Black;
        }
    }
}
