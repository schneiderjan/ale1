using System.Collections.Generic;
using System.Windows.Documents;
using Ale1Project.Model;

namespace Ale1Project.Service
{
    public interface IFileService
    {
        void WriteGraphVizFileToDotFile(List<string> lines,string name);
    }
}