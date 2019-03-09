using QuantaculaCore;
using System;
using System.Drawing;

namespace QuantaculaIndicators
{
    public class ZScore : IndicatorBase
    {
        //parameterless constructor
        public ZScore() : base()
        {
        }

        //for code based construction
        public ZScore(TimeSeries ds, Int32 period)
            : base()
        {
			Parameters[0].Value = ds;
			Parameters[1].Value = period;

            Populate();
        }

		public override string Name => "ZScore";

		public override string Abbreviation => "ZScore";

		public override string HelpDescription => @"Z-Score from the May 2019 issue of Technical Analysis of Stocks & Commodities magazine.";

		public override string PaneTag => @"ZScore";

		public override Color DefaultColor => Color.DarkViolet;

		public override PlotStyles DefaultPlotStyle => PlotStyles.ThickHistogram;

        //populate
        public override void Populate()
        {
			TimeSeries ds = Parameters[0].AsTimeSeries;
			Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

			if (period <= 0 || ds.Count == 0)
				return;
	        
			var sma = SMA.Series(ds, period);
			StdDev sd = StdDev.Series(ds, period);
			TimeSeries zScore = (ds - sma) / sd;

			for (int bar = 0; bar < ds.Count; bar++)
			{
				Values[bar] = zScore[bar];
			}
        }

        //generate parameters
        protected override void GenerateParameters()
        {
			AddParameter("Data Series", ParameterTypes.TimeSeries, PriceComponents.Close);
			AddParameter("Lookback period", ParameterTypes.Int32, 10);
        }
    }
}
