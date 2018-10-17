using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCIndicators
{
    public class SZO : IndicatorBase
    {
        //parameterless constructor
        public SZO() : base()
        {
        }

        //for code based construction
        public SZO(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 14);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = period * 3;

            if (ds == null || ds.Count == 0) return;

            var R = new TimeSeries(DateTimes);
            var SP = new TimeSeries(DateTimes);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar < period)
                    R[bar] = 0d;
                else
                    R[bar] = (ds[bar] > ds[bar - 1]) ? 1 : -1;
            }

            var sp = new TEMA(R, period);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= FirstValidValue)
                    Values[bar] = 100 * (sp[bar] / period);
                else
                    Values[bar] = 0d;
            }
        }

        public override string Name => "SZO";

        public override string Abbreviation => "SZO";

        public override string HelpDescription => "The Sentiment Zone Oscillator by W.Khalil measures extreme bearishness and bullishness to help identify a change in sentiment.";

        public override string PaneTag => @"SZO";

        public override Color DefaultColor => Color.Black;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

        public override double OverboughtLevel => 7;

        public override double OversoldLevel => -7;
    }    
}
