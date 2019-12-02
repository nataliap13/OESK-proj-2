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
        public string StdDev { get; private set; }

        public TableCalcParams(string functionName, int textLength, List<TimeSpan> listOfTimes)
        {
            Name = functionName;
            Length = textLength;
            Min = TimeSpanConverter.ToSecondsMiliseconds(listOfTimes.Min());
            Avg = TimeSpanConverter.ToSecondsMiliseconds(DoubleTicksToTimespan(listOfTimes.Average(timeSpan => timeSpan.Ticks)));
            Max = TimeSpanConverter.ToSecondsMiliseconds(listOfTimes.Max());
            StdDev = StandardDeviation(listOfTimes);
        }

        public static string StandardDeviation(IEnumerable<TimeSpan> values)
        {
            double avgTicks = values.Average(timeSpan => timeSpan.Ticks);
            var stdDevInTicks = Math.Sqrt(values.Average(v => Math.Pow(v.Ticks - avgTicks, 2)));
            return TimeSpanConverter.ToSecondsMiliseconds(
                DoubleTicksToTimespan(stdDevInTicks));
        }
        private static TimeSpan DoubleTicksToTimespan(double value)
        { return new TimeSpan(Convert.ToInt64(value)); }
    }
}
