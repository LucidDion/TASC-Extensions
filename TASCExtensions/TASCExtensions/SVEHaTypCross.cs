using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCIndicators
{
    public class SVEHaTypCross : IndicatorBase
    {
        //parameterless constructor
        public SVEHaTypCross() : base()
        {
        }

        //for code based construction
        public SVEHaTypCross(BarHistory bars, Int32 haPeriod, Int32 typPeriod)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = haPeriod;
            Parameters[2].Value = typPeriod;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("Heikin-Ashi Period", ParameterTypes.Int32, 8);
            AddParameter("Typical period", ParameterTypes.Int32, 5);
        }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 haPeriod = Parameters[1].AsInt;
            Int32 typPeriod = Parameters[2].AsInt;

            DateTimes = bars.DateTimes;
            var period = Math.Max(haPeriod, typPeriod);

            if (period <= 0 || DateTimes.Count == 0 || bars.Count < period)
                return;

            var b = new BarHistory(bars.Symbol.ToString() + " (Heikin-Ashi)", bars.Scale);

            // Heikin-Ashi series
            TimeSeries HO = bars.Open + 0, HH = bars.High + 0, HL = bars.Low + 0;
            TimeSeries HC = (bars.Open + bars.High + bars.Low + bars.Close) / 4;

            // Build the BarHistory object
            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (bar > 0)
                {
                    double o1 = HO[bar - 1];
                    double c1 = HC[bar - 1];
                    HO[bar] = (o1 + c1) / 2;
                    HH[bar] = Math.Max(HO[bar], bars.High[bar]);
                    HL[bar] = Math.Min(HO[bar], bars.Low[bar]);
                    b.Add(bars.DateTimes[bar], HO[bar], HH[bar], HL[bar], HC[bar], bars.Volume[bar]);
                }
                else
                    b.Add(bars.DateTimes[0], bars.Open[0], bars.High[0], bars.Low[0], bars.Close[0], bars.Volume[0]);
            }

            //TODO b = BarScaleConverter.Synchronize(b, bars);

            // Build EMA of Heikin-Ashi close and EMA of Typical price
            var haEma = new EMA(b.Close, haPeriod);
            var tpEma = new EMA(bars.AveragePriceHLC, typPeriod);

            double cross = 0;

            for (int bar = period; bar < bars.Count; bar++)
            {
                if (bars.Close[bar] > bars.Open[bar] && tpEma[bar] > haEma[bar])
                    cross = 1;
                else
                    if (bars.Close[bar] < bars.Open[bar] && tpEma[bar] < haEma[bar])
                        cross = 0;
                    else
                        cross = Values[bar - 1];

                Values[bar] = cross;
            }
        }

        public override string Name => "SVEHaTypCross";

        public override string Abbreviation => "SVEHaTypCross";

        public override string HelpDescription => "Sylvain Vervoort's SVEHaTypCross indicator from October 2013 issue of Stocks & Commodities magazine is a trend-following indicator based on moving average crossovers on prices smoothed by using a blend of Heikin Ashi and typical price.";

        public override string PaneTag => @"SVEHaTypCross";

        public override Color DefaultColor => Color.DodgerBlue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

    }    
}
