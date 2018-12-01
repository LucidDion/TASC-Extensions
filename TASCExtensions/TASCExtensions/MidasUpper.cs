using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //MidasUpper Indicator class
    public class MidasUpper : IndicatorBase
    {
        //parameterless constructor
        public MidasUpper() : base()
        {
        }

        //for code based construction
        public MidasUpper(BarHistory source, Int32 startBar, int barsToSwingHigh)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = startBar;
            Parameters[2].Value = barsToSwingHigh;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Start Bar", ParameterTypes.Int32, 10);
            AddParameter("Bars from Start", ParameterTypes.Int32, 10);
        }

        //populate
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            Int32 startBar = Parameters[1].AsInt;
            Int32 barsToSwingHigh = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            if (startBar <= 0 || ds.Count == 0)
                return;

            //Assign first bar that contains indicator data
            var FirstValidValue = startBar;
            if (FirstValidValue > ds.Count || FirstValidValue < 0)
            {
                FirstValidValue = ds.Count;
                return;
            }

            var _midas = new Midas(ds, startBar);
            double _swingHighPct = 0;

            int b = startBar + barsToSwingHigh;
            if (b >= ds.Count - 1)
                _swingHighPct = 0d;
            else
                _swingHighPct = ds.High[b] / _midas[b];

            // Although the indicator is calculated to the startBar for display purposes, it is valid only at and beyond the swing Low Bar
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Values[bar] = _swingHighPct * _midas[bar];
            }
        }

        public override string Name => "MidasUpper";

        public override string Abbreviation => "MidasUpper";

        public override string HelpDescription => "The Upper Midas (Market Interpretation Data Analysis System) displacement channel from the July 2010 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => "Price";

        public override Color DefaultColor => Color.DarkGray;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Bands;

        public override List<string> Companions => new List<string>() { "MidasLower" };
    }
}
