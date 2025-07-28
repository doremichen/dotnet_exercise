/** FileComparerApp - A simple application to compare files and folders.
 * 
 * This code is part of the FileComparerApp project, which allows users to compare files and folders.
 * The FolderNode class represents a folder in the comparison structure.
 * This class is Text Compare Strategy implementation for comparing text files.
 * 
 * Author: Adam Chen
 * Date: 2025/07/25
 */
using FileComparerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileComparerApp.Services
{
    public class TextCompareStrategy : ICompareStrategy
    {
        public CompareResult Compare(ICompareNode left, ICompareNode right)
        {
            // Compare result object to hold the comparison results
            var result = new CompareResult
            {
                RootNode = CompareFolders(left, right)
            };
            CollectDifferences(result.RootNode, result.ChangedNodes);
            return result;
        }

        private void CollectDifferences(ICompareNode node, List<ICompareNode> diffList)
        {
            if (node.CompareType != CompareItemType.Unchanged)
                diffList.Add(node);

            foreach (var child in node.Children)
                CollectDifferences(child, diffList);
        }

        private ICompareNode CompareFolders(ICompareNode left, ICompareNode right)
        {
            // check if both nodes are folders
            if (left is FolderNode leftFolder && right is FolderNode rightFolder)
            {
                // Create a new folder node to hold the comparison result
                var folder = new FolderNode
                {
                    FullPath = leftFolder.FullPath ?? rightFolder.FullPath,
                    CompareType = CompareItemType.Unchanged // Default to unchanged
                };
                // Compare children of both folders
                var leftChildren = leftFolder.Children.ToDictionary(c => c.Name);
                var rightChildren = rightFolder.Children.ToDictionary(c => c.Name);
                // Add logic to compare children and populate resultFolder.Children accordingly
                // This part will involve comparing files and subfolders recursively
                var allNames = new HashSet<string>(leftChildren.Keys.Concat(rightChildren.Keys));

                foreach (var name in allNames)
                {
                    leftChildren.TryGetValue(name, out var leftNode);
                    rightChildren.TryGetValue(name, out var rightNode);

                    if (leftNode == null && rightNode != null)
                    {
                        // 建立右側新增的 FileNode
                        var file = rightNode as FileNode;
                        if (file != null)
                        {
                            folder.Children.Add(new FileNode
                            {
                                LeftPath = null,
                                RightPath = file.FullPath,
                                CompareType = CompareItemType.Added
                            });
                        }
                    }
                    else if (rightNode == null && leftNode != null)
                    {
                        // 建立左側遺失的 FileNode
                        var file = leftNode as FileNode;
                        if (file != null)
                        {
                            folder.Children.Add(new FileNode
                            {
                                LeftPath = file.FullPath,
                                RightPath = null,
                                CompareType = CompareItemType.Removed
                            });
                        }
                    }
                    else if (leftNode.IsDirectory)
                    {
                        var child = CompareFolders(leftNode, rightNode);
                        folder.Children.Add(child);
                        if (child.CompareType != CompareItemType.Unchanged)
                            folder.CompareType = CompareItemType.Modified;
                    }
                    else // 正常比對左右兩檔案內容
                    {
                        var leftFile = leftNode as FileNode;
                        var rightFile = rightNode as FileNode;

                        var leftText = File.Exists(leftFile.FullPath) ? File.ReadAllText(leftFile.FullPath) : "";
                        var rightText = File.Exists(rightFile.FullPath) ? File.ReadAllText(rightFile.FullPath) : "";

                        var isEqual = leftText == rightText;
                        var type = isEqual ? CompareItemType.Unchanged : CompareItemType.Modified;

                        var file = new FileNode
                        {
                            LeftPath = leftFile.FullPath,
                            RightPath = rightFile.FullPath,
                            CompareType = type
                        };

                        folder.Children.Add(file);
                        if (type != CompareItemType.Unchanged)
                            folder.CompareType = CompareItemType.Modified;
                    }
                }


                return folder;
            }
            else
            {
                throw new ArgumentException("Both nodes must be FolderNode instances.");
            }
        }
    }
}
