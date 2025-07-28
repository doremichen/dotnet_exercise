/** FileComparerApp - A simple application to compare files and folders.
 * 
 * This code is part of the FileComparerApp project, which allows users to compare files and folders.
 * The FolderNode class represents a folder in the comparison structure.
 * This class is CompareStrategyFactory implementation for creating compare strategies based on the selected mode.
 * 
 * Author: Adam Chen
 * Date: 2025/07/25
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparerApp.Services
{
    public enum CompareMode
    {
        Text,
        Binary
    }

    public static class CompareStrategyFactory
    {

        public static ICompareStrategy Create(CompareMode? mode)
        {
            return mode switch
            {
                CompareMode.Text => new TextCompareStrategy(),
                CompareMode.Binary => new BinaryCompareStrategy(),
                _ => throw new ArgumentException("Invalid compare mode", nameof(mode)),
            };
        }


    }
}
