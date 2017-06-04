using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralVis
{

    class LinechartManager
    {
        SeriesCollection seriesCol;
        LineSeries series;
        List<Object> buffer;

        public LinechartManager(SeriesCollection sc)
        {
            this.seriesCol = sc;
            this.buffer = new List<object>(500);
        }

        public void add(Object o) {

            if (series.Values.Count < 1000)
            { 
                series.Values.Add(o);
            }
            else
            {
                buffer.Add(o);
                if (buffer.Count > 100)
                {
                    series.Values.AddRange((IEnumerable<Object>)buffer);
                    buffer.Clear();
                }
            }

        }

        public void clear()
        {
            seriesCol.Clear();
        }

        public void pushBuffer()
        {
            if (buffer.Count > 0)
                series.Values.AddRange((IEnumerable<Object>)buffer);
            buffer.Clear();
        }

        public void newSeries()
        {
            this.series = new LineSeries
            {
                PointGeometry = null,
                Values = new ChartValues<double> { }
            };

            seriesCol.Add(series);
        }
    }
}
