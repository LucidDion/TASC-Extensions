using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //RegEMA Indicator class
    public class RegEMA : IndicatorBase
    {
        //parameterless constructor
        public RegEMA() : base()
        {
        }

        //for code based construction
        public RegEMA(TimeSeries source, Double smoothing, Double regularization)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = smoothing;
            Parameters[2].Value = regularization;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Smoothing", ParameterTypes.Double, 0.91);
            AddParameter("Regularization", ParameterTypes.Double, 0.5);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Double smoothing = Parameters[1].AsDouble;
            Double regularization = Parameters[2].AsDouble;

            DateTimes = ds.DateTimes;

            if (ds.Count == 0)
                return;

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + 2;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series
            for (int bar = 0; bar < FirstValidValue; bar++)
                Values[bar] = ds[bar];

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double term1 = (1 + 2 * regularization) * this[bar - 1];
                double term2 = smoothing * (ds[bar] - this[bar - 1]);
                double term3 = regularization * this[bar - 2];
                Values[bar] = (term1 + term2 - term3) / (1 + regularization);
            }
        }

        public override string Name => "RegEMA";

        public override string Abbreviation => "RegEMA";

        public override string HelpDescription => "Regularized EMA indicator from Chris Satchwell, Ph.D. Published in the July 2003 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

        public override bool IsSmoother => true;
    }
}
