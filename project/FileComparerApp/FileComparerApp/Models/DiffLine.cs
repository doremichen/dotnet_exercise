/** FileComparerApp - A simple file comparison application.
 * 
 * Description: This class is DiffLine, which represents a line in the diff view of the FileComparerApp.
 * 
 * Author: Adam Chen
 * Date: 2025/07/28
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FileComparerApp.Models
{
    public class DiffLine
    {
        public string Text { get; set; }
        public Brush Background { get; set; } = Brushes.Transparent;
    }
}
