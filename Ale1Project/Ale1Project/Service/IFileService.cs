using System.Collections.Generic;
using System.Windows.Documents;
using Ale2Project.Model;

namespace Ale2Project.Service
{
    public interface IFileService
    {
        void WriteGraphVizFileToDotFile(List<string> lines);
    }
}