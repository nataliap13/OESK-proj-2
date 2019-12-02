using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OESK
{
    static class TimeSpanConverter
    {
        public static string ToSecondsMiliseconds(TimeSpan MD5Time)
        { return (((int)MD5Time.TotalSeconds).ToString() + "," + String.Format("{0:fffffff}", MD5Time)); }
    }
}
