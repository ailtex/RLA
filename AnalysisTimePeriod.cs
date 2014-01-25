using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectedLightAnalysis
{
    class AnalysisTimePeriod
    {
        /// <summary>
        /// 日期
        /// </summary>
        private DateTime date;

        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime startTime;

        public DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        private DateTime endTime;

        public DateTime EndTime
        {
            get { return endTime; }
            set { endTime = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        private double intervalInHour;

        public double IntervalInHour
        {
            get { return intervalInHour; }
            set { intervalInHour = value; }
        }
        
    }
}
