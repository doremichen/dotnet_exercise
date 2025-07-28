/**
 * FileComparerApp - A file comparison application
 * 
 * Description: This is an interface that defines the structure for comparison nodes in the FileComparer application.
 * 
 * Author: Adam Chen
 * Date: 2025/07/25
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparerApp.Models
{
    public interface ICompareNode
    {
        // Name property represents the name of the comparison node.
        string Name { get; }
        // FullPath property represents the full path of the file or directory being compared.
        string FullPath { get; }
        // CompareType property indicates the type of comparison result for the node (e.g., unchanged, added, removed, modified).
        CompareItemType CompareType { get; set; }
        // IsDirectory property indicates whether the node represents a directory.
        bool IsDirectory { get; }
        public IReadOnlyList<ICompareNode> Children => new List<ICompareNode>();
    }
}
