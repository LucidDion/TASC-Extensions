using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //FVE Indicator class
    public class FVE : IndicatorBase
    {
        //parameterless constructor
        public FVE() : base()
        {
        }

        //for code based construction
        public FVE(BarHistory source, Int32 period)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 30);
        }

        //populate
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;
            
            //Avoid exception errors
            if (period < 1 || period > ds.Count + 1) period = ds.Count + 1;

            //Assign first bar that contains indicator data
            var FirstValidValue = period;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            var MFSer = new TimeSeries(DateTimes);
            MFSer.Description = string.Format("MFSer({0},{1})",ds.Symbol,period);
            //for (int i = 0; i < ds.Count; i++)
                //MFSer[i] = 0d;

            for (int bar = 1; bar < ds.Count; bar++)
            {
                double MF = 8 * ds.Close[bar] - (ds.High[bar] + ds.Low[bar]) 
                          - 2 * (ds.Close[bar - 1] + ds.High[bar - 1] + ds.Low[bar - 1]);
                if      (MF > +0.018 * ds.Close[bar]) MFSer[bar] = +ds.Volume[bar];
                else if (MF < -0.018 * ds.Close[bar]) MFSer[bar] = -ds.Volume[bar];
                else                                  MFSer[bar] = 0;  
            }

            var SMAMFSer = new SMA(MFSer, period);
            var SMAVol = new SMA(ds.Volume, period);
            for (int bar = 0; bar < ds.Count; bar++)
                if (SMAVol[bar] > 0)
                    Values[bar] = 100 * SMAMFSer[bar] / SMAVol[bar];
                //else
                    //Values[bar] = 0;
        }

        public override string Name => "FVE";

        public override string Abbreviation => "FVE";

        public override string HelpDescription => "Finite Volume Elements Indicator from the April 2003 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"FVE";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }    
}
