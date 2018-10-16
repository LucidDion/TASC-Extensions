using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;
using System.Collections.Generic;

namespace TASCExtensions
{
    public class WDPPO : IndicatorBase
    {
        //constructors
        public WDPPO() : base()
        {
        }
        public WDPPO(TimeSeries source, int fastPeriod = 12, int slowPeriod = 26, int weeklyFastPeriod = 60, int weeklySlowPeriod = 130) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = fastPeriod;
            Parameters[2].Value = slowPeriod;
            Parameters[3].Value = weeklyFastPeriod;
            Parameters[4].Value = weeklySlowPeriod;
            Populate();
        }

        //Name
        public override string Name
        {
            get
            {
                return "Relative Daily PPO";
            }
        }

        //Abbreviation
        public override string Abbreviation
        {
            get
            {
                return "W+DPPO";
            }
        }

        //help description
        public override string HelpDescription
        {
            get
            {
                return "Relative Daily Percentage Price Oscillator, based on the article by Vitali Apirine in the February 2018 issue of Stocks & Commodities magazine.";
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
                return Color.Green;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Fast Period (D)", ParameterTypes.Int32, 12);
            AddParameter("Slow Period (D)", ParameterTypes.Int32, 26);
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
            int fastPeriodWeekly = Parameters[3].AsInt;
            int slowPeriodWeekly = Parameters[4].AsInt;
            DateTimes = source.DateTimes;

            //calculate
            DPPO dppo = new DPPO(source, fastPeriod, slowPeriod, slowPeriodWeekly);
            WPPO wppo = new WPPO(source, fastPeriodWeekly, slowPeriodWeekly);
            TimeSeries result = dppo + wppo;
            Values = result.Values;
        }

        //companions
        public override List<string> Companions
        {
            get
            {
                List<string> c = new List<string>();
                c.Add("WPPO");
                return c;
            }
        }
    }
}