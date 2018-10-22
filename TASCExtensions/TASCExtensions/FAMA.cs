using QuantaculaCore;
using QuantaculaIndicators;
using System.Collections.Generic;
using System.Drawing;

namespace TASCExtensions
{
    public class FAMA : IndicatorBase
    {
        //constructors
        public FAMA() : base()
        {
        }
        public FAMA(TimeSeries source, double fastLimit = 0.5, double slowLimit = 0.05) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = fastLimit;
            Parameters[2].Value = slowLimit;
            Populate();
        }

        //name
        public override string Name => "Following Adaptive Moving Average";

        //abbreviation
        public override string Abbreviation => "FAMA";

        //description
        public override string HelpDescription => "Following Adaptive Moving Average by John Ehlers from the September 2001 issue of Technical Analysis of Stocks & Commodities magazine";

        //plot in price pane
        public override string PaneTag => "Price";

        //default color
        public override Color DefaultColor => Color.Blue;

        //it's a smoother
        public override bool IsSmoother => true;

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Fast Limit", ParameterTypes.Double, 0.5);
            AddParameter("Slow Limit", ParameterTypes.Double, 0.05);
        }

        //companion
        public override List<string> Companions
        {
            get
            {
                List<string> c = new List<string>();
                c.Add("MAMA");
                return c;
            }
        }

        //populate
        public override void Populate()
        {
            //gather parameters values
            TimeSeries source = Parameters[0].AsTimeSeries;
            double fast = Parameters[1].AsDouble;
            double slow = Parameters[2].AsDouble;
            DateTimes = source.DateTimes;

            //look for cached calculator
            string key = "FAMA(" + fast + "," + slow + ")";
            if (!source.Cache.ContainsKey(key))
                source.Cache[key] = new MamaFamaCalculator(source, fast, slow);
            MamaFamaCalculator mfc = source.Cache[key] as MamaFamaCalculator;
            Values = mfc.FAMA.Values;
        }
    }
}