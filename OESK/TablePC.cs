using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OESK
{
    public class TablePC
    {
        [Key]
        public int IDPC { get; set; }

        public int IDCPU { get; set; }//FK
        public virtual TableCPU TableCPU { get; set; }

        public int IDRAM { get; set; }//FK
        public virtual TableRAM TableRAM { get; set; }

        public virtual List<TableTest> TableTest { get; set; }
    }
}
