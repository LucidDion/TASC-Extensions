using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{    
    // Alpha Indicator Class
    public class Alpha : IndicatorBase
    {        
        //parameterless constructor
        public Alpha() : base()
        {
        }

        //for code based construction
        public Alpha(TimeSeries source, Int32 stddevperiod, Int32 linearregperiod)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = stddevperiod;
            Parameters[2].Value = linearregperiod;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("StdDev Period", ParameterTypes.Int32, 7);
            AddParameter("LinReg Period", ParameterTypes.Int32, 3);
        }

        //name
        public override string Name
        {
            get
            {
                return "Alpha";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "Alpha";
            }
        }

        //description
        public override string HelpDescription
        {
            get
            {
                return "Based on an article by Rick Martinelli, published in the June 2006 issue of Stocks and Commodities Magazine. The Alpha indicator is a measure of how likely tomorrow's price will be away from normal distributed prices.";
            }
        }

        //price pane
        public override string PaneTag
        {
            get
            {
                return "Alpha";
            }
        }

        //default color
        public override Color DefaultColor
        {
            get
            {
                return Color.FromArgb(255, 0, 128, 0);
            }
        }

        //default plot style
        public override PlotStyles DefaultPlotStyle
        {
            get
            {
                return PlotStyles.ThickLine;
            }
        }

        //Constructor
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 stddevperiod = Parameters[1].AsInt;
            Int32 linearregperiod = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            //Remember parameters
            var k2 = (linearregperiod + 1) / 3.0;
            var k1 = linearregperiod / 2.0;
            //standard deviation of price changes
            var sd = new StdDev(new Momentum(ds, 1), stddevperiod);

            //Avoid exception errors
            if (linearregperiod < 3 || linearregperiod > ds.Count + 1) linearregperiod = ds.Count + 1;

            //Assign first bar that contains indicator data
            //FirstValidValue = ds.FirstValidValue + Math.Max(linearregperiod, sdper) - 1;

            //Initialize start of series with zeroes, and begin accumulating values
            var s2 = 3 * ds[0];
            var s1 = 2 * ds[0];
            for (int bar = 0; bar < linearregperiod - 1; bar++)
            {
                s2 = s2 + (2 * ds[bar] - s1) / k2;
                s1 = s1 + (ds[bar] - ds[0]) / k1;
            }

            s2 = s2 + (2 * ds[linearregperiod - 1] - s1) / k2;
            s1 = s1 + (ds[linearregperiod - 1] - ds[0]) / k1;
            double predict = ((linearregperiod + 1) * s2 - (linearregperiod + 2) * s1) / (linearregperiod - 1); 
            if (sd[linearregperiod - 1] > 0)
                Values[linearregperiod - 1] = (predict - ds[linearregperiod - 1]) / sd[linearregperiod - 1];

            //Average rest of series
            for (int bar = linearregperiod; bar < ds.Count; bar++)
            {
                s2 = s2 + (2 * ds[bar] - s1) / k2;
                s1 = s1 + (ds[bar] - ds[bar - linearregperiod]) / k1;
                predict = ((linearregperiod + 1) * s2 - (linearregperiod + 2) * s1) / (linearregperiod - 1);
                if (sd[bar] > 0)
                    Values[bar] = (predict - ds[bar]) / sd[bar];
            }
        }        
    }
}
