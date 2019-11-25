using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OESK
{
    public class TableText
    {
        [Key]
        public int IDText { get; set; }
        public string Text { get; set; }

        public virtual List<TableTestResult> TableTestResult { get; set; }
    }
}
