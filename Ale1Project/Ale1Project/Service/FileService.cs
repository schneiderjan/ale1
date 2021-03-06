﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Ale1Project.Model;

namespace Ale1Project.Service
{
    public class FileService : IFileService
    {
        public void WriteGraphVizFileToDotFile(List<string> lines, string name)
        {
            using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\dot"+ name +".dot", false))
            {
                foreach (var line in lines)
                {
                    sw.WriteLine(line);
                }
            }
        }
    }

}

