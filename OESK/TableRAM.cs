using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OESK
{
    public class TableRAM
    {
        [Key]
        public int IDRAM { get; set; }
        public int RAMCapacity { get; set; }
        public int RAMFrequancy { get; set; }
    }
}
