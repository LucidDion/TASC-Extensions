using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCExtensions
{
    //Stiffness indicator - November 2018
    public class Stiffness : IndicatorBase
    {
        //constructors
        public Stiffness() : base()
        {
        }
        public Stiffness(TimeSeries source, int maPeriod, int period) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = maPeriod;
            Parameters[2].Value = period;
            Populate();
        }

        //Name
        public override string Name
        {
            get
            {
                return "Stiffness";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "Stiffness";
            }
        }

        //description
        public override string HelpDescription
        {
            get
            {
                return "The Stiffness indicator from the November 2018 issue of Technical Analysis of Stocks & Commodities magazine.";
            }
        }

        //plot in its own pane
        public override string PaneTag
        {
            get
            {
                return "Stiffness";
            }
        }

        //default histogram
        public override PlotStyles DefaultPlotStyle
        {
            get
            {
                return PlotStyles.Histogram;
            }
        }

        //default color
        public override Color DefaultColor
        {
            get
            {
                return Color.DarkOrange;
            }
        }

        //zero axis
        public override bool UseZeroOrigin
        {
            get
            {
                return true;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("MA Period", ParameterTypes.Int32, 100);
            AddParameter("Period", ParameterTypes.Int32, 60);
        }

        //populate
        public override void Populate()
        {
            //resolve parameters
            TimeSeries source = Parameters[0].AsTimeSeries;
            int maPeriod = Parameters[1].AsInt;
            int period = Parameters[2].AsInt;

            //calculate
            SMA sma = new SMA(source, maPeriod);
            StdDev stdDev = new StdDev(source, maPeriod);
            TimeSeries ma2 = sma - 0.2 * stdDev;
            TimeSeries gt = new TimeSeries(source.DateTimes);
            for (int n = 0; n < source.Count; n++)
                gt[n] = source[n] > ma2[n] ? 1 : 0;
            TimeSeries p = gt.Sum(period);
            p = p * 100 / 60;
            Values = p.Values;
        }
    }
}