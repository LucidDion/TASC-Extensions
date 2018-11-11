using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCExtensions
{
    public class PFE : IndicatorBase
    {
        //constructors
        public PFE() : base()
        {
        }
        public PFE(TimeSeries source, int period, int smoothing) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = smoothing;
            Populate();
        }

        //Name
        public override string Name => "Polarized Fractal Efficiency";

        //Abbreviation
        public override string Abbreviation => "PFE";

        //description
        public override string HelpDescription => "Polarized Fractal Efficiency from the January 1994 issue of Stocks & Commodities magazine.";

        //plot in its own pane
        public override string PaneTag => "PFE";

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 9);
            AddParameter("Smoothing", ParameterTypes.Int32, 5);
        }

        //default color
        public override Color DefaultColor => Color.CornflowerBlue;

        //populate
        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            int period = Parameters[1].AsInt;
            int smoothing = Parameters[2].AsInt;
            TimeSeries pfe = Sqrt(Pow(source - (source >> period), 2.0) + 100.0);
            TimeSeries c2c = Sum(Sqrt(Pow(source - (source >> 1), 2.0) + 1.0), period);
            TimeSeries xFrac = BooleanTest(source - (source >> period), Round(pfe / c2c * 100.0), Round(pfe / c2c * -100.0));
            TimeSeries ema = EMA.Series(xFrac, smoothing);
            Values = ema.Values;
        }
    }
}