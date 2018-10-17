using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //InverseFisherRSI Indicator class
    public class InverseFisherRSI : IndicatorBase
    {
        //parameterless constructor
        public InverseFisherRSI() : base()
        {
        }

        //for code based construction
        public InverseFisherRSI(TimeSeries source, Int32 emaPeriod, Int32 rsiPeriod)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = emaPeriod;
            Parameters[1].Value = rsiPeriod;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("RSI Period", ParameterTypes.Int32, 4);
            AddParameter("EMA Period", ParameterTypes.Int32, 4);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 emaPeriod = Parameters[1].AsInt;
            Int32 rsiPeriod = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            if (emaPeriod <= 0 || rsiPeriod <= 0 || ds.Count == 0)
                return;

            //Assign first bar that contains indicator data
            var FirstValidValue = Math.Max(ds.FirstValidIndex, Math.Max(emaPeriod, rsiPeriod));
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            var rbw = new WMA(ds, 2);
            var sve = 5 * rbw;
            
            for (int w = 4; w >= -4; w--)
            {
                rbw = new WMA(rbw, 2);
                if (w > 1) sve += (w * rbw);
                else sve += rbw;
            }
            sve /= 20d;     // sve.Description = "SVE Rainbow";

            var x = 0.1 * (new RSI(sve, rsiPeriod) - 50);
            var e1 = new EMA(x, emaPeriod);
            var e2 = new EMA(e1, emaPeriod);
            var z1ema = e1 + (e1 - e2);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                Values[bar] = (InverseFisher.Calculate(bar, z1ema) + 1) * 50;
            }
        }
        
        public override string Name => "InverseFisherRSI";

        public override string Abbreviation => "InverseFisherRSI";

        public override string HelpDescription => "InverseFisherRSI by Sylvain Vervoot from the October 2010 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"InverseFisherRSI";

        public override Color DefaultColor => Color.Navy;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }
}
