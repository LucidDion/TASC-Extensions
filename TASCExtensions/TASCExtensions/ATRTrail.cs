using QuantaculaCore;
using QuantaculaIndicators;
using System;
using System.Drawing;

namespace TASCExtensions
{
    public class ATRTrail : IndicatorBase
    {
        //parameterless constructor
        public ATRTrail() : base()
        {
        }

        //for code based construction
        public ATRTrail(BarHistory source, Int32 period, Double factor)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = factor;
            Populate();
        }

        //name
        public override string Name
        {
            get
            {
                return "ATRTrail";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "ATRTrail";
            }
        }

        //description
        public override string HelpDescription
        {
            get
            {
                return "ATR Trailing Stop by Sylvain Vervoort.";
            }
        }

        //price pane
        public override string PaneTag
        {
            get
            {
                return "Price";
            }
        }

        //default color
        public override Color DefaultColor
        {
            get
            {
                return Color.FromArgb(255, 128, 0, 0);
            }
        }

        //default plot style
        public override PlotStyles DefaultPlotStyle
        {
            get
            {
                return PlotStyles.Line;
            }
        }

        //populate
        public override void Populate()
        {
            BarHistory source = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;
            Double factor = Parameters[2].AsDouble;
            DateTimes = source.DateTimes;

            //ATR
            ATR atr = new ATR(source, period);

            //calculate ATR Trailing Stop
            for (int n = period; n < source.Count; n++)
            {
                double loss = factor * atr[n];
                if (source.Close[n] > Values[n - 1] && source.Close[n - 1] > Values[n - 1])
                    Values[n] = Math.Max(Values[n - 1], source.Close[n] - loss);
                else if (source.Close[n] < Values[n - 1] && source.Close[n - 1] < Values[n - 1])
                    Values[n] = Math.Min(Values[n - 1], source.Close[n] + loss);
                else
                    Values[n] = source.Close[n] > Values[n - 1] ? source.Close[n] - loss : source.Close[n] + loss;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 5);
            AddParameter("Factor", ParameterTypes.Double, 3.6);
        }
    }
}