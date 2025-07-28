
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparerApp.Models
{
    public class CompareResult
    {
        // root node of the comparison result tree
        public ICompareNode RootNode { get; set; }

        // changed nodes in the comparison result
        public List<ICompareNode> ChangedNodes { get; set; } = new List<ICompareNode>();

        /**
         * AddCountedNode - Adds a node to the list of changed nodes.
         * RemoveCountedNode - Removes a node from the list of changed nodes.
         * ModifiedCountedNode - Modifies a node in the list of changed nodes.
         */
        public int AddedCount => ChangedNodes.Count(n => n.CompareType == CompareItemType.Added);
        public int RemovedCount => ChangedNodes.Count(n => n.CompareType == CompareItemType.Removed);
        public int ModifiedCount => ChangedNodes.Count(n => n.CompareType == CompareItemType.Modified);

    }
}
