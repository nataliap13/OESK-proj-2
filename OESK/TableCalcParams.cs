using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OESK
{
    class TableCalcParams
    {
        public string FunctionName { get; private set; }
        public int TextLength { get; private set; }
        public int NumberOfIterations { get; private set; }
        public string TestTimeInSeconds { get; private set; }
        public TimeSpan TestTimeInSecondsAsTimeSpan { get; private set; }

        public TableCalcParams(string functionName, int textLength, int numberOfIterations,
            TimeSpan testTime)
        {
            FunctionName = functionName;
            TextLength = textLength;
            TestTimeInSecondsAsTimeSpan = testTime;
            TestTimeInSeconds = TimeSpanConverter.ToSecondsMiliseconds(testTime);
            NumberOfIterations = numberOfIterations;
        }

        private static TimeSpan LongTicksToTimespan(long value)
        { return new TimeSpan(Convert.ToInt64(value)); }
    }
}
