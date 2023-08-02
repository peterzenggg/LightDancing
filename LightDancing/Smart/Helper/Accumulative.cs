using System.Collections.Generic;
using System.Linq;

namespace LightDancing.Smart.Helper
{
    public class Accumulative
    {
        private readonly List<double> values;

        public double PreviousAverage { get; private set; }

        public Accumulative()
        {
            values = new List<double>();
            PreviousAverage = 0;
        }

        /// <summary>
        /// Get the variance with previous average value
        /// </summary>
        /// <returns></returns>
        public double GetVariance()
        {
            return values.Count > 0 ? values.Average() - PreviousAverage : 0;
        }

        /// <summary>
        /// Get current values of the average
        /// </summary>
        /// <returns></returns>
        public double GetCurrentAverage()
        {
            return values.Count > 0 ? values.Average() : 0;
        }

        /// <summary>
        /// Settlement all values
        /// </summary>
        public void CalculateValues()
        {
            PreviousAverage = values.Count > 0 ? values.Average() : 0;
            values.Clear();
        }

        /// <summary>
        /// Add value  
        /// </summary>
        /// <param name="centroid"></param>
        public void Add(double value)
        {
            values.Add(value);
        }

        internal void Reset()
        {
            values.Clear();
            PreviousAverage = 0;
        }
    }
}
