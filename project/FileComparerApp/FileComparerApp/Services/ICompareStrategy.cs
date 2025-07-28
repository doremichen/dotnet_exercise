/**
 * Description: This interface defines the contract for comparison strategies in the FileComparer application.
 * 
 * Author: Adam Chen
 * Date: 2025/07/25
 */
using FileComparerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparerApp.Services
{
    public interface ICompareStrategy
    {
        CompareResult Compare(ICompareNode left, ICompareNode right);
    }
}
