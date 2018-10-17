using System.Drawing;
using QuantaculaCore;
using System;
using System.Collections.Generic;

namespace QuantaculaIndicators
{
    public class SARSILower : IndicatorBase
    {
        //parameterless constructor
        public SARSILower() : base()
        {
        }

        //for code based construction
        public SARSILower(TimeSeries source, Int32 period = 14, Double multiplier = 2.0, Boolean useStdDevVersion = false)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = multiplier;
            Parameters[3].Value  = useStdDevVersion;

            Populate();
        }

        //name
        public override string Name
        {
            get
            {
                return "Self-Adjusting RSI Lower";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "SARSILower";
            }
        }

        //description
        public override string HelpDescription
        {
            get
            {
                return ("David Sepiashvili's Self-Adjusting RSI from the February 2006 issue of Stocks & Commodities magazine exhibits a technique "
                           + "to adjust the traditional RSI overbought and oversold thresholds to ensure that 70-80% of RSI values fall between the two thresholds. "
                        + "There are 2 versions of the indicator, the second using a StdDev calculation by passing true to the last parameter.  The multiplier suggested for the SD version is 1.8.");
            }
        }

        //price pane
        public override string PaneTag
        {
            get
            {
                return "RSI";
            }
        }

        //default color
        public override Color DefaultColor
        {
            get
            {
                return Color.FromArgb(255, 0, 128, 255);
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
            TimeSeries source = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double multiplier = Parameters[2].AsDouble;
            bool useSD = Parameters[3].AsBoolean;

            DateTimes = source.DateTimes;
            TimeSeries overbought = null;

            RSI rsi = new RSI(source, period);
            if (useSD)
            {
                StdDev sd = new StdDev(rsi, period);
                overbought = 50 - multiplier * sd;
            }
            else
            {
                SMA sma = new SMA(rsi, period);
                overbought = new SMA((rsi - sma).Abs(), period);
                overbought = 50 - multiplier * overbought;
            }

            //modify the code below to implement your own indicator calculation
            for (int n = 0; n < source.Count; n++)
            {
                Values[n] = overbought[n];
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("period", ParameterTypes.Int32, 14);
            AddParameter("multiplier", ParameterTypes.Double, 2.0);
            AddParameter("Use StdDev", ParameterTypes.Boolean, false);
        }

        //companions
        public override List<string> Companions
        {
            get
            {
                List<string> c = new List<string>();
                c.Add("SARSIUpper");
                return c;
            }
        }

    }
}
