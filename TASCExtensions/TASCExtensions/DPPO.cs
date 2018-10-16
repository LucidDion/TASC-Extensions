using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCExtensions
{
    public class DPPO : IndicatorBase
    {
        //constructors
        public DPPO() : base()
        {
        }
        public DPPO(TimeSeries source, int fastPeriod = 12, int slowPeriod = 26, int weeklySlowPeriod = 130) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = fastPeriod;
            Parameters[2].Value = slowPeriod;
            Parameters[3].Value = weeklySlowPeriod;
            Populate();
        }

        //Name
        public override string Name
        {
            get
            {
                return "Daily PPO";
            }
        }

        //Abbreviation
        public override string Abbreviation
        {
            get
            {
                return "DPPO";
            }
        }

        //help description
        public override string HelpDescription
        {
            get
            {
                return "Daily Percentage Price Oscillator, based on the article by Vitali Apirine in the February 2018 issue of Stocks & Commodities magazine.";
            }
        }

        //pane tag
        public override string PaneTag
        {
            get
            {
                return "W+DPPO";
            }
        }

        //default color
        public override Color DefaultColor
        {
            get
            {
                return Color.Silver;
            }
        }

        //default plot style
        public override PlotStyles DefaultPlotStyle
        {
            get
            {
                return PlotStyles.DashedLine;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Fast Period (D)", ParameterTypes.Int32, 12);
            AddParameter("Slow Period (D)", ParameterTypes.Int32, 26);
            AddParameter("Slow Period (W)", ParameterTypes.Int32, 130);
        }

        //populate
        public override void Populate()
        {
            //get parameter values
            TimeSeries source = Parameters[0].AsTimeSeries;
            int fastPeriod = Parameters[1].AsInt;
            int slowPeriod = Parameters[2].AsInt;
            int slowPeriodWeekly = Parameters[3].AsInt;
            DateTimes = source.DateTimes;

            //calculate
            EMA emaFast = new EMA(source, fastPeriod);
            EMA emaSlow = new EMA(source, slowPeriod);
            EMA emaSlowWeekly = new EMA(source, slowPeriodWeekly);
            TimeSeries result = ((emaFast - emaSlow) / emaSlowWeekly) * 100.0;
            Values = result.Values;
        }
    }
}