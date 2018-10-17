using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class ARSI : IndicatorBase
    {
        //parameterless constructor
        public ARSI() : base()
        {
        }

        //for code based construction
        public ARSI(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 14);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            // Create Up/DnCount Series using some fancy TimeSeries math
            Momentum roc = new Momentum(ds, 1);
            TimeSeries _pc = new ConsecUp(roc, 1);
            _pc /= _pc;
            TimeSeries UpCount = _pc.Sum(period);
            TimeSeries DnCount = period - UpCount;

            /* Derivation of the EMA Exponent use for ARSI 

                exponent (updated method):
                expnt = 2 / ( 1 + Periods )			
    			
                ARSI Wilder smoothing periods: 
                Periods = 2 * Counter - 1
    			
                consequently,
                expnt = 2 / ( 1 + ( 2 * Counter - 1 )  ), so,     		
                expnt = 1 / Counter
    			
                EMA calculation:
                diff = expnt * ( ds[bar] - CumSum ), so, 
                diff = ( ds[bar] - CumSum ) / Counter
                CumSum := CumSum + Diff;
            */

            Values[0] = ds[0];
            double UpMove = 0d;
            double DnMove = 0d;
            double RS = 0d;
            double val = 0d;

            // initialize sums
            for (int bar = 1; bar < period - 1; bar++)
            {
                if (roc[bar] >= 0)
                    UpMove = UpMove + roc[bar];
                else
                    DnMove = DnMove + Math.Abs(roc[bar]);
            }
            UpMove = UpMove / period;
            DnMove = DnMove / period;
            RS = UpMove / DnMove;
            Values[period - 1] = 100 - (100 / (1 + RS));

            for (int bar = period; bar < ds.Count; bar++)
            {                
                val = roc[bar] >= 0 ? roc[bar] : 0d;
                //expnt = 1 / UpCount[bar];
                //diff = expnt * (val - UpMove);  ->  diff = (val - UpMove) / UpCount[bar];   
                if (UpCount[bar] != 0)
                    UpMove += (val - UpMove) / UpCount[bar];
                
                val = roc[bar] < 0 ? Math.Abs(roc[bar]) : 0d;
                if (DnCount[bar] != 0)
                    DnMove += (val - DnMove) / DnCount[bar]; ;

                double den = DnMove != 0 ? DnMove : 1;
                RS = UpMove / den;
                Values[bar] = 100 - (100 / (1 + RS));

                if (Double.IsNaN(Values[bar]))
                    RS = 0d;
            }

            //TODO Save final values for partial-bar calculation
            //_upMove = UpMove;
            //_dnMove = DnMove;
        }



        public override string Name => "ARSI";

        public override string Abbreviation => "ARSI";

        public override string HelpDescription => @"ARSI (Asymmetric Relative Strength Index) from the October 2008 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"RSI";

        public override Color DefaultColor => Color.RoyalBlue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }
}
