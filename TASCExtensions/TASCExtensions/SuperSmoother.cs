using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class SuperSmoother : IndicatorBase
    {
        //parameterless constructor
        public SuperSmoother() : base()
        {
        }

        //for code based construction
        public SuperSmoother(TimeSeries source)
            : base()
        {
            Parameters[0].Value = source;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;

            DateTimes = ds.DateTimes;

            if (ds.Count < 2)
                return;

            var FirstValidValue = 2;
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            double Deg2Rad = Math.PI / 180.0;
            double a1 = Math.Exp(-1.414 * Math.PI / 10d);
            //double b1 = 2.0 * a1 * Math.Cos(1.414 * 180d / 10d);
            double b1 = 2.0 * a1 * Math.Cos((1.414 * 180d / 10d) * Deg2Rad);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1 - c2 - c3;

            Values[0] = Values[1] = 0d;

            for (int bar = 2; bar < ds.Count; bar++)
            {
                double Filt = c1 * (ds[bar] + ds[bar - 1]) / 2 + c2 * Values[bar - 1] + c3 * Values[bar - 2];
                Values[bar] = Filt;
            }
        }

        public override string Name => "SuperSmoother";

        public override string Abbreviation => "SuperSmoother";

        public override string HelpDescription => "Created by John Ehlers (see article in January 2014 issue of Stocks and Commodities Magazine), the SuperSmoother filter is superior to moving averages for removing aliasing noise as well as other general smoothing applications.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.DarkRed;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
