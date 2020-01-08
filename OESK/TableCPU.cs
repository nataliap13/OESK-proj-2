using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OESK
{
    public class TableCPU
    {
        [Key]
        public int IDCPU { get; set; }
        public string CPUName { get; set; }
    }
}
