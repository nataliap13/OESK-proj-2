using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OESK
{
    public class TableTestResult
    {
        [Key]
        public int ID { get; set; }

        public int IDText { get; set; }//FK
        public virtual TableText TableText { get; set; }

        public string MD5CalculationTime { get; set; }
        public string SHA1CalculationTime { get; set; }
        public string SHA256CalculationTime { get; set; }

        public string RamFrequency { get; set; }
        public string RamCLDelay { get; set; }
    }
}
