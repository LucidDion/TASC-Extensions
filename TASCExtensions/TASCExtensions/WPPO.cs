using QuantaculaCore;
using QuantaculaIndicators;
using System.Collections.Generic;
using System.Drawing;

namespace TASCExtensions
{
    public class WPPO : IndicatorBase
    {
        //constructors
        public WPPO() : base()
        {
        }
        public WPPO(TimeSeries source, int fastPeriod = 60, int slowPeriod = 130) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = fastPeriod;
            Parameters[2].Value = slowPeriod;
            Populate();
        }

        //Name
        public override string Name
        {
            get
            {
                return "Weekly PPO";
            }
        }

        //Abbreviation
        public override string Abbreviation
        {
            get
            {
                return "WPPO";
            }
        }

        //help description
        public override string HelpDescription
        {
            get
            {
                return "Weekly Percentage Price Oscillator, based on the article by Vitali Apirine in the February 2018 issue of Stocks & Commodities magazine.";
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
                return Color.FromArgb(0, 32, 0);
            }
        }

        //thick line
        public override PlotStyles DefaultPlotStyle
        {
            get
            {
                return PlotStyles.ThickLine;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Fast Period (W)", ParameterTypes.Int32, 60);
            AddParameter("Slow Period (W)", ParameterTypes.Int32, 130);
        }

        //populate
        public override void Populate()
        {
            //get parameter values
            TimeSeries source = Parameters[0].AsTimeSeries;
            int fastPeriod = Parameters[1].AsInt;
            int slowPeriod = Parameters[2].AsInt;
            DateTimes = source.DateTimes;

            //calculate
            EMA emaFast = new EMA(source, fastPeriod);
            EMA emaSlow = new EMA(source, slowPeriod);
            TimeSeries result = ((emaFast - emaSlow) / emaSlow) * 100.0;
            Values = result.Values;
        }

        //companions
        public override List<string> Companions
        {
            get
            {
                List<string> c = new List<string>();
                c.Add("WDPPO");
                return c;
            }
        }
    }
}