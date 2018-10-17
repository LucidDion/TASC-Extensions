using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public enum WhichMA { EMA, SMA }

    public class MHLMA : IndicatorBase
    {
        //parameterless constructor
        public MHLMA() : base()
        {
        }

        //for code based construction
        public MHLMA(BarHistory source, Int32 periodHigh, Int32 periodMA, WhichMA choice)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = periodHigh;
            Parameters[2].Value = periodMA;
            Parameters[3].Value = choice;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Highest/Lowest Period", ParameterTypes.Int32, 15);
            AddParameter("MA Period", ParameterTypes.Int32, 50);
            Parameter p = AddParameter("EMA or SMA?", ParameterTypes.StringChoice, "EMA");
            p.Choices.Add("EMA");
            p.Choices.Add("SMA");
            p.TypeName = "WhichMA";
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 highPeriod = Parameters[1].AsInt;
            Int32 movPeriod = Parameters[2].AsInt;
            WhichMA option = (WhichMA)Enum.Parse(typeof(WhichMA), Parameters[3].AsString);

            DateTimes = bars.DateTimes;

            var period = Math.Max(highPeriod, movPeriod);
            if (period <= 0 || bars.Count == 0)
                return;

            var tempMHL = new TimeSeries(DateTimes);
            var HH = new Highest(bars.High,highPeriod);
            var LL = new Lowest(bars.Low,highPeriod);
            tempMHL = (HH + LL) / 2;

            for (int bar = period; bar < bars.Count; bar++)
            {
                Values[bar] = tempMHL[bar];

                if (option == WhichMA.EMA) //EMA
                    Values[bar] = new EMA(tempMHL, movPeriod)[bar];
                else
                    Values[bar] = new SMA(tempMHL, movPeriod)[bar];
            }
        }

        public override bool IsSmoother => true;

        public override string Name => "MHLMA";

        public override string Abbreviation => "MHLMA";

        public override string HelpDescription => "Created by V. Apirine, the MHL MA is a moving average applied to the middle of the high-low range.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.Green;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
