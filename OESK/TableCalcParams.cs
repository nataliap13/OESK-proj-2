using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OESK
{
    class TableCalcParams
    {
        public string Name { get; private set; }
        public int Length { get; private set; }
        public string Min { get; private set; }
        public string Avg { get; private set; }
        public string Max { get; private set; }
        public double StdDev { get; private set; }

        public TableCalcParams(string functionName, int textLength, List<TimeSpan> listOfTimes)
        {
            Name = functionName;
            Length = textLength;
            Min = listOfTimes.Min().ToString();
            Avg = listOfTimes.Average(timeSpan => timeSpan.Ticks).ToString();
            Max = listOfTimes.Max().ToString();
            StdDev = StandardDeviation(listOfTimes);
        }

        public static double StandardDeviation(IEnumerable<TimeSpan> values)
        {
            double avgTicks = values.Average(timeSpan => timeSpan.Ticks);
            return Math.Sqrt(values.Average(v => Math.Pow(v.Ticks - avgTicks, 2)));
        }
    }
}
