using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class SVAPO : IndicatorBase
    {
        //parameterless constructor
        public SVAPO() : base()
        {
        }

        //for code based construction
        public SVAPO(BarHistory bars, Int32 period, Double cutoff)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = period;
            Parameters[2].Value = cutoff;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 8);
            AddParameter("Cutoff", ParameterTypes.Double, 1.0);
        }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;
            Double cutoff = Parameters[2].AsDouble;

            DateTimes = bars.DateTimes;

            if (period <= 0 || DateTimes.Count == 0)
                return;

            #region Create Heikin-Ashi series
            BarHistory haBars = new BarHistory(bars.Symbol.ToString() + " (Heikin-Ashi)", HistoryScale.Daily);
            var HO = bars.Open + 0;
            var HH = bars.High + 0;
            var HL = bars.Low + 0;
            var HC = (bars.Open + bars.High + bars.Low + bars.Close) / 4;

            for (int bar = 1; bar < bars.Count; bar++)
            {
                double o1 = HO[bar - 1];
                double c1 = HC[bar - 1];
                HO[bar] = (o1 + c1) / 2;
                HH[bar] = Math.Max(HO[bar], bars.High[bar]);
                HL[bar] = Math.Min(HO[bar], bars.Low[bar]);

                haBars.Add(bars.DateTimes[bar], HO[bar], HH[bar], HL[bar], HC[bar], bars.Volume[bar]);
            }
            #endregion

            //Smooth Heikin-Ashi closing price
            HC = new TEMA_TASC(HC, Math.Max((int)1, Convert.ToInt32(period / 1.6d)));

            //Medium term MA of Volume to limit extremes and division factor
            var vave = new SMA(bars.Volume, period * 5) >> 1;
            var vmax = vave * 2;
            var vc = new TimeSeries(DateTimes);
            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (bars.Volume[bar] < vmax[bar])
                    vc[bar] = bars.Volume[bar];
                else
                    vc[bar] = vmax[bar];
            }

            //Basic volume trend
            var vtr = new TEMA_TASC(new LRSlope(bars.Volume, period), period);

            //SVAPO result of price and volume}
            var tempSeries = new TimeSeries(DateTimes);

            for (int bar = 2; bar < bars.Count; bar++)
            {
                double temp = 0;

                bool hcRising = HC[bar] > (HC[bar - 1] * (1 + cutoff / 1000d));
                bool hcDeclining = HC[bar] < (HC[bar - 1] * (1 - cutoff / 1000d));
                bool lame2PeriodAlert1 = (vtr[bar] >= vtr[bar - 1]) || (vtr[bar - 1] >= vtr[bar - 2]);
                bool lame2PeriodAlert2 = (vtr[bar] > vtr[bar - 1]) || (vtr[bar - 1] > vtr[bar - 2]);

                temp = (hcRising && lame2PeriodAlert1) ? vc[bar] :
                   (hcDeclining && lame2PeriodAlert2) ? -vc[bar] : 0;

                tempSeries[bar] = temp;
            }
            
            var sumSeries = tempSeries.Sum(period);
            var svapo = new TEMA_TASC((sumSeries + vave + 1), period);   

            for (int bar = period; bar < bars.Count; bar++)
            {
                Values[bar] = svapo[bar];
            }
        }

        public override string Name => "SVAPO";

        public override string Abbreviation => "SVAPO";

        public override string HelpDescription => "Sylvain Vervoort's Short-Term Price And Volume Oscillator (SVAPO). See article in November 2007 issue of Stocks and Commodities Magazine.";

        public override string PaneTag => @"SVAPO";

        public override Color DefaultColor => Color.RoyalBlue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
