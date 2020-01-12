using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OESK
{
    public class TableFunction
    {
        [Key]
        public int IDFunction { get; set; }
        public string FunctionName { get; set; }

        public virtual List<TableTest> TableTest { get; set; }
    }
}
