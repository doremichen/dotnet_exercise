/** FileComparerApp - A simple file comparison application.
 * 
 * Description: This class implements line-by-line comparison of file contents.
 * Author: Adam Chen
 * Date: 2025/07/28
 */
using FileComparerApp.Models;
using FileComparerApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FileComparerApp.Services
{
    public class LineByLineDiffer : IContentDiffer
    {
        public (IEnumerable<DiffLine> left, IEnumerable<DiffLine> right) Compare(string[] leftLines, string[] rightLines)
        {
            Util.Log($"Comparing {leftLines.Length} left lines with {rightLines.Length} right lines using LineByLineDiffer.");
            var leftResult = new List<DiffLine>();
            var rightResult = new List<DiffLine>();

            // MAX length of the two arrays to handle cases where they are of different lengths
            int max = Math.Max(leftLines.Length, rightLines.Length);
            Util.Log($"Max lines to compare: {max}");
            // Iterate through both arrays and compare line by line
            for (int i = 0; i < max; i++)
            {
                string left = i < leftLines.Length ? leftLines[i] : string.Empty;
                string right = i < rightLines.Length ? rightLines[i] : string.Empty;

                // Check if the lines are different
                bool isDiff = left != right;
                // Set background color based on whether the lines are different
                var bg = isDiff ? Brushes.Red : Brushes.Transparent;

                leftResult.Add(new DiffLine { Text = left, Background = bg });
                rightResult.Add(new DiffLine { Text = right, Background = bg });
            }

            return (leftResult, rightResult);
        }
    }
}
