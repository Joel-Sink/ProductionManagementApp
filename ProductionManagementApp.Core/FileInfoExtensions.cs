using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProductionManagementApp.Core
{
    public static class FileInfoExtensions
    {
        public static List<FileInfo> ToList(this FileInfo[] files)
        {
            List<FileInfo> fileList = new List<FileInfo>();
            foreach (var file in files)
            {
                fileList.Add(file);
            }
            return fileList;
        }
    }
}
