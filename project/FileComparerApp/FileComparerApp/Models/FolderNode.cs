/**
 * FileComparerApp - A file comparison application
 * 
 * Description: This is Folder Node class that represents a folder in the file comparison application.
 * 
 * Author: Adam Chen
 * Date: 2025/07/25
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparerApp.Models
{
    public class FolderNode : ICompareNode
    {
        public string FullPath { get; set; }
        public CompareItemType CompareType { get; set; }

        public List<ICompareNode> Children { get; set; } = new();

        public bool IsDirectory => true;
        public string Name => Path.GetFileName(FullPath);

        IReadOnlyList<ICompareNode> ICompareNode.Children => Children;
    }
}
