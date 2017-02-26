using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1Project.Model
{
    public class TruthTableModel
    {
        public TruthTableModel()
        {
            Rows = new List<string>();
        }

        public List<string> Rows { get; set; }
        public bool[,] TableValues { get; set; }
        public bool[] Answers { get; set; }
        public string Hexadecimal { get; set; }
        public string Binary { get; set; }
    }
}
