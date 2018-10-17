using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //CyberCycle Indicator class
    public class CyberCycle : IndicatorBase
    {
        //parameterless constructor
        public CyberCycle() : base()
        {
        }

        //for code based construction
        public CyberCycle(TimeSeries source, Double alpha)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = alpha;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Alpha", ParameterTypes.Double, 0.07);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Double alpha = Parameters[1].AsDouble;

            DateTimes = ds.DateTimes;

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + 5;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            if (FirstValidValue <= 0 || ds.Count == 0)
                return;

            var OneLessAlpha = 1 - alpha;
            var OneLessAlphaSq = OneLessAlpha * OneLessAlpha;
            var OneLessHalfAlphaSq = (1 - alpha / 2) * (1 - alpha / 2);

            //Initialize start of series
            double Cycle = 0;
            for (int bar = 0; bar < FirstValidValue; bar++)
            {
                if (bar >= 2) Cycle = (ds[bar] - 2 * ds[bar - 1] + ds[bar - 2]) / 4;
                Values[bar] = Cycle;
            }

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Cycle = OneLessHalfAlphaSq * (ds[bar - 5] - ds[bar - 3] - ds[bar - 2] + ds[bar]) / 6
                      + 2 * OneLessAlpha * Values[bar - 1] - OneLessAlphaSq * Values[bar - 2];
                Values[bar] = Cycle;
            }
        }



        public override string Name => "CyberCycle";

        public override string Abbreviation => "CyberCycle";

        public override string HelpDescription => @"CyberCycle Indicator from the May 2004 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"CyberCycle";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;

    }
}
