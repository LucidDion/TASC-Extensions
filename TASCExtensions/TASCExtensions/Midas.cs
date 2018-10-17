using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //Midas Indicator class
    public class Midas : IndicatorBase
    {
        //parameterless constructor
        public Midas() : base()
        {
        }

        //for code based construction
        public Midas(BarHistory source, Int32 startBar)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = startBar;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Start Bar", ParameterTypes.Int32, 10);
        }

        //populate
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            Int32 startBar = Parameters[1].AsInt;

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

            //Initialization before first value
            var ap = (ds.High + ds.Low) / 2;
            double _cumV = 0, _cumPV = 0, _cumVst = 0, _cumPVst;

            for (int bar = 0; bar <= FirstValidValue; bar++)
            {
                Values[bar] = ap[bar];
                _cumV += ds.Volume[bar];
                _cumPV += ap[bar] * ds.Volume[bar];
            }            
            _cumVst = _cumV;
            _cumPVst = _cumPV;                               
           
            for (int bar = FirstValidValue + 1; bar < ds.Count; bar++)
            {
                if (bar < FirstValidValue + 1)
                    Values[bar] = 0d;
                else
                {
                    _cumV += ds.Volume[bar];
                    _cumPV += ap[bar] * ds.Volume[bar];
                    double dV = _cumV - _cumVst;
                    dV = dV == 0 ? 1 : dV;
                    Values[bar] = (_cumPV - _cumPVst) / dV;
                }
            }
        }

        public override string Name => "Midas";

        public override string Abbreviation => "Midas";

        public override string HelpDescription => "Midas (Market Interpretation Data Analysis System) from the September 2008 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.Blue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
