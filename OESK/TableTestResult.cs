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
        public int IDTestResult { get; set; }

        public int IDTest { get; set; }//FK
        public virtual TableTest TableTest { get; set; }

        public int IDText { get; set; }//FK
        public virtual TableText TableText { get; set; }

        public int NumberOfIterations { get; set; }
        public string FullTime { get; set; }
        public string AvgTime { get; set; }
    }
}
