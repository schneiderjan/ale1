using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ale1Project.Model;

namespace Ale1Project.Service
{
    public class GraphVizService : IGraphVizService
    {
        public void DisplayAutomaton()
        {
            //Path to GrahpViz depends on Installation path!!!
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.WorkingDirectory = @"C:\Program Files (x86)\Graphviz2.38\bin";
            processStartInfo.FileName = @"C:\Program Files (x86)\Graphviz2.38\bin\dot.exe";
            processStartInfo.Arguments = $"-Tpng -odot.png{System.IO.Directory.GetCurrentDirectory()} dot.dot";
            processStartInfo.ErrorDialog = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;

            Process p = Process.Start(processStartInfo);

            Thread.Sleep(100);
            try
            {
                ProcessStartInfo pictureProcessStartInfo = new ProcessStartInfo();
                pictureProcessStartInfo.FileName = System.IO.Directory.GetCurrentDirectory() + "dot.png";
                Process.Start(pictureProcessStartInfo);
            }
            catch (Win32Exception exception)
            {
                Debug.WriteLine(exception.Message);
                Debug.WriteLine("Cannot open file. A file with the same name is already opened.");
            }
        }

        public GraphVizFileModel ConvertExpressionModelToGraphVizFile(ExpressionModel expressionModel)
        {
            var file = new GraphVizFileModel();
            file.Lines.Add("graph logic {");
            file.Lines.Add("node[fontname = \"Arial\"]");

            RecursivelyAddLines(file, expressionModel);

            file.Lines.Add("}");

            return file;
        }

        private void RecursivelyAddLines(GraphVizFileModel file, ExpressionModel expressionModel)
        {
            foreach (var treeNode in expressionModel.TreeNodes)
            {
                Debug.WriteLine(treeNode.Value);
            }
        }
    }
}
