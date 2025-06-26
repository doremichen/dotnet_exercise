/**
 * Description: This class is a StatisticsViewModel for the Student Grade Manager application
 * Author: Adam Chen
 * Date: 2025-06-26
 */
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager.ViewModels
{
    public class StatisticsViewModel
    {
        public List<string> StudentNames { get; set; }
        public SeriesCollection AverageSeries { get; set; }
        public SeriesCollection MaxSeries { get; set; }
        public SeriesCollection MinSeries { get; set; }

        public StatisticsViewModel(List<string> names)
        {
            StudentNames = names;
            AverageSeries = new SeriesCollection();
            MaxSeries = new SeriesCollection();
            MinSeries = new SeriesCollection();
        }

    }
}
