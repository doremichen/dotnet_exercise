/** FileComparerApp - A simple file comparison application.
 * 
 * Description: This class is Tree loader, which is responsible for loading file and directory structures into a tree format for comparison.
 * Author: Adam Chen
 * Date: 2025/07/28
 */
using FileComparerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparerApp.Utils
{
    public static  class TreeLoader
    {

        public static ICompareNode LoadFromPath(string path)
        {
            if (Directory.Exists(path))
                return LoadDirectory(path);
            else if (File.Exists(path))
                return LoadFile(path);
            else
                throw new FileNotFoundException($"找不到路徑: {path}");
        }

        private static ICompareNode LoadDirectory(string path)
        {
            var folder = new FolderNode
            {
                FullPath = path
            };

            foreach (var dir in Directory.GetDirectories(path))
                folder.Children.Add(LoadDirectory(dir));

            foreach (var file in Directory.GetFiles(path))
                folder.Children.Add(LoadFile(file));

            return folder;
        }

        private static ICompareNode LoadFile(string path)
        {
            var fileInfo = new FileInfo(path);

            return new FileNode
            {
                LeftPath = path,
                LeftModified = fileInfo.LastWriteTime,
                LeftSize = fileInfo.Length,
                //LeftHash = Util.CalculateHash(path), // 如果你有 Hash 工具
                CompareType = CompareItemType.Unchanged // 預設先放 Unchanged
            };
        }
    }
}
