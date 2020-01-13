using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OESK
{
    public class TableTest
    {
        [Key]
        public int IDTest { get; set; }

        public int IDPC { get; set; }//FK
        public virtual TablePC TablePC { get; set; }

        public int IDFunction { get; set; }//FK
        public virtual TableFunction TableFunction { get; set; }

        public int IDText { get; set; }//FK
        public virtual TableText TableText { get; set; }

        public int NumberOfIterations { get; set; }
        public string FullTime { get; set; }
    }
}
