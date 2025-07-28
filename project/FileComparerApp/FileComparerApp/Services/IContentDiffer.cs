
using FileComparerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparerApp.Services
{
    public interface IContentDiffer
    {
        (IEnumerable<DiffLine> left, IEnumerable<DiffLine> right) Compare(string[] leftLines, string[] rightLines);
    }
}
