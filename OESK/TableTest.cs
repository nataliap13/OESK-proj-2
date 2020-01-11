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
    }
}
