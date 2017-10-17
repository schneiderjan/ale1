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

        public ImplicantModel(int group, string implicant, int originalNrOfOnes)
        {
            Implicant = implicant;
            OriginalNrOfOnes = originalNrOfOnes;
            Group = group;
        }

        public string Implicant { get; private set; }
        public int OriginalNrOfOnes { get; private set; }
        public int Group { get; private set; }
    }
}
