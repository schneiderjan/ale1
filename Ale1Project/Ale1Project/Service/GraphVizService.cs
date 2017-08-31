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
        public void DisplayGraph()
        {
            //Path to GrahpViz depends on Installation path!!!
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                WorkingDirectory = @"C:\Program Files (x86)\Graphviz2.38\bin",
                FileName = @"C:\Program Files (x86)\Graphviz2.38\bin\dot.exe",
                Arguments = $"-Tpng -o{System.IO.Directory.GetCurrentDirectory()}\\dot.png {System.IO.Directory.GetCurrentDirectory()}\\dot.dot",
                ErrorDialog = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true
            };

            Debug.WriteLine(processStartInfo.Arguments);
            Process p = Process.Start(processStartInfo);

            Thread.Sleep(100);
            try
            {
                ProcessStartInfo pictureProcessStartInfo = new ProcessStartInfo();
                pictureProcessStartInfo.FileName = System.IO.Directory.GetCurrentDirectory() + "\\dot.png";
                Process.Start(pictureProcessStartInfo);
            }
            catch (Win32Exception exception)
            {
                Debug.WriteLine(exception.Message);
                //Debug.WriteLine("Cannot open file. A file with the same name is already opened.");
            }
        }

        public GraphVizFileModel ConvertExpressionModelToGraphVizFile(ExpressionModel expressionModel)
        {
            var file = new GraphVizFileModel();
            file.Lines.Add("graph logic {");
            file.Lines.Add("node[fontname = \"Arial\"]");

            var currentNode = expressionModel.TreeNodes.FirstOrDefault();
            RecursivelyAddLines(file, currentNode);

            file.Lines.Add("}");

            return file;
        }

        private void RecursivelyAddLines(GraphVizFileModel file, NodeModel currentNode)
        {
            //Add node to graph
            file.Lines.Add($"node{currentNode.Id} [ label = \"{currentNode.Value}\" ]");

            //Add connection to childs
            if (currentNode.LeftChild != null)
            {
                file.Lines.Add($"node{currentNode.Id} -- node{currentNode.LeftChild.Id}");
                RecursivelyAddLines(file, currentNode.LeftChild);
            }
            if (currentNode.RightChild != null)
            {
                file.Lines.Add($"node{currentNode.Id} -- node{currentNode.RightChild.Id}");
                RecursivelyAddLines(file, currentNode.RightChild);
            }
        }
    }
}
