/** FileComparerApp - A simple file comparison application.
 * 
 * Description: This class is FileNode, which represents a file node in the FileComparerApp.
 * 
 * Author: Adam Chen
 * Date: 2025/07/28
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparerApp.Models
{
    public class FileNode : ICompareNode
    {
        public string? LeftPath { get; set; }
        public string? RightPath { get; set; }

        public string Name => System.IO.Path.GetFileName(LeftPath ?? RightPath ?? "");
        public string FullPath => LeftPath ?? RightPath ?? "";

        public CompareItemType CompareType { get; set; }

        public bool IsDirectory => false;

        public DateTime? LeftModified { get; set; }
        public DateTime? RightModified { get; set; }

        public long? LeftSize { get; set; }
        public long? RightSize { get; set; }

        public string? LeftHash { get; set; }
        public string? RightHash { get; set; }

        public IReadOnlyList<ICompareNode> Children => new List<ICompareNode>();
    }
}
