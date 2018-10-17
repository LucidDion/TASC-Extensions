using System.Drawing;
using QuantaculaCore;
using System;

namespace QuantaculaIndicators
{
    public class RevEngSMA_TC : IndicatorBase
    {
        //parameterless constructor
        public RevEngSMA_TC() : base()
        {
        }

        //for code based construction
        public RevEngSMA_TC(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //name
        public override string Name
        {
            get
            {
                return "Reverse Engineered SMA Tomorrow's Close";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "RevEngSMA_TC";
            }
        }

        //description
        public override string HelpDescription
        {
            get
            {
                return "The RevEngSMA_TC indicator was derived by Tsokakis in the February 2007 issue of Stocks & Commodities "
                    + "magazine. 'TC' stands for 'tomorrow's close'. Tsokakis demonstrated that crossovers of this indicator "
                    + "with the source series occurred one bar earlier than crossovers of SMA indicators of the specified "
                    + "periods a very high percentage of the time.  \r\n\r\nFor example, create line plots of bars.Close (C) and "
                    + "RevEngSMA_TC(C, 20, 30). You can expect that C crosses under RevEngSMA_TC one bar before "
                    + "SMA(C, 20) crosses below SMA(C, 30) and vice-versa for cross over.";
            }
        }

        //price pane
        public override string PaneTag
        {
            get
            {
                return "RevEngSMA_TC";
            }
        }

        //default color
        public override Color DefaultColor
        {
            get
            {
                return Color.FromArgb(255, 0, 0, 0);
            }
        }

        //default plot style
        public override PlotStyles DefaultPlotStyle
        {
            get
            {
                return PlotStyles.Line;
            }
        }

        //populate
        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;

            DateTimes = source.DateTimes;

            SMA sma1 = new SMA(source, period1 - 1);
            SMA sma2 = new SMA(source, period2 - 1);

            if (period1 < 1 || period1 > source.Count + 1) period1 = source.Count + 1;
            if (period2 < 1 || period2 > source.Count + 1) period2 = source.Count + 1;

            int  firstValidIndex = source.FirstValidIndex + Math.Max(period1, period2) - 2;

            if (period2 == period1) return;

            for (int n = firstValidIndex; n < source.Count; n++)
            {
                Values[n] = (sma2[n] * period1 * (period2 - 1) - sma1[n] * period2 * (period1 - 1)) / (period2 - period1);
            }
        }

        public static double Calculate(int idx, TimeSeries source, int period1, int period2)
        {
            if (period1 < 1 || period1 > source.Count + 1) period1 = source.Count + 1;
            if (period2 < 1 || period2 > source.Count + 1) period2 = source.Count + 1;

            return (SMA.Calculate(idx, source, period2) * period1 * (period2 - 1) 
                  - SMA.Calculate(idx, source, period1) * period2 * (period1 - 1)) 
                  / (period2 - period1);            
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("period1", ParameterTypes.Int32, 20);
            AddParameter("period2", ParameterTypes.Int32, 50);

        }
    }
}
