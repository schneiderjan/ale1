using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1Project.Model
{
    public class ImplicantModel
    {
        public ImplicantModel(int group, string implicant)
        {
            Implicant = implicant;
            Group = group;
        }

        public string Implicant { get; set; }
        public int Group { get; set; }
    }
}
