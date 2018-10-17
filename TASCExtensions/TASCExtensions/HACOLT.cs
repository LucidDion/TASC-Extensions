using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCIndicators
{
    public class HACOLT : IndicatorBase
    {
        //parameterless constructor
        public HACOLT() : base()
        {
        }

        //for code based construction
        public HACOLT(BarHistory source, Int32 period, Double factor, Int32 timeout, Int32 average)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = factor;
            Parameters[3].Value = timeout;
            Parameters[4].Value = average;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 34);
            AddParameter("Candle Size factor", ParameterTypes.Double, 1.1);
            AddParameter("Setup timeout", ParameterTypes.Int32, 2);
            AddParameter("Shorting LT Average", ParameterTypes.Int32, 60);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;
            Double CandleSize = Parameters[2].AsDouble;
            Int32 setuptimeout = Parameters[3].AsInt;
            Int32 ShLTAvg = Parameters[4].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            var FirstValidValue = Math.Max(ShLTAvg, period);
            //base.FirstValidValue = Math.Max(ShLTAvg, period) * 3;

            int setup1bar = -1; int setup2bar = -1;
            bool keep1 = false; bool keep2 = false; bool keep3 = false;
            bool keeping = false; bool keepall = false; int keepallBar = -1;
            bool keep1d = false; bool keep2d = false; bool keep3d = false;
            bool keepingd = false; bool keepalld = false;
            int keepallBard = -1; int keepingdBar = -1; int keepingBar = -1;
            bool utr = false; int utrBar = -1; int dtrBar = -1; bool dtr = false;
            bool upw = true; bool dnw = true; int lastOccurenceForLTResult = -1;
            int i_result = 0; int LTResult = 0; int final_result = 0;

            /* Create LT Average */

            EMA LTAverage = new EMA(bars.Close, ShLTAvg);

            /* Create Heikin-Ashi candles */

            var HO = bars.Open + 0;
            var HH = bars.High + 0;
            var HL = bars.Low + 0;
            var HC = (bars.Open + bars.High + bars.Low + bars.Close) / 4;
            var haC = HC + 0;
            //haC.Description = "Heikin-Ashi Close";

            for (int bar = 1; bar < bars.Count; bar++)
            {
                double o1 = HO[bar - 1];
                double c1 = HC[bar - 1];
                HO[bar] = (o1 + c1) / 2;
                HH[bar] = Math.Max(HO[bar], bars.High[bar]);
                HL[bar] = Math.Min(HO[bar], bars.Low[bar]);
                haC[bar] = (HC[bar] + HO[bar] + HH[bar] + HL[bar]) / 4;
            }

            /* Create "Crossover Formula" */

            var TMA1 = new TEMA(haC, period);
            var TMA2 = new TEMA(TMA1, period);
            var Diff = TMA1 - TMA2;
            var ZlHa = TMA1 + Diff;
            var AveragePrice = (bars.High + bars.Low) / 2;
            TMA1 = new TEMA(AveragePrice, period);
            TMA2 = new TEMA(TMA1, period);
            Diff = TMA1 - TMA2;
            var ZlCl = TMA1 + Diff;
            var ZlDif = ZlCl - ZlHa;
            //ZlDif.Description = "Crossover formula (" + period + ")";

            for (int bar = FirstValidValue; bar < bars.Count; bar++)
            {
                /* Create green candle */

                // Metastock Alert function resource:
                // http://www.meta-formula.com/metastock-alert-function.html

                if (!keep1)
                {
                    bool newCondition = (bars.Close[bar] > haC[bar]) || (bars.High[bar] > bars.High[bar - 1] || bars.Low[bar] > bars.Low[bar - 1]);

                    if (haC[bar] >= HO[bar] || newCondition)
                    {
                        keep1 = true;
                        setup1bar = bar;
                    }
                }
                if (keep1)
                {
                    keep1 = bar + 1 - setup1bar < setuptimeout;
                }

                keep2 = (ZlDif[bar] >= 0);
                keeping = (keep1 || keep2);

                // Save bar when "keeping" is true
                if (keeping) keepingBar = bar; else keepingBar = 0;

                keepall = keeping || (
                    // Implements Ref(keeping,-1)
                    ((keepingBar > 0) & (bar == keepingBar + 1)) &
                    (bars.Close[bar] >= bars.Open[bar]) | (bars.Close[bar] >= bars.Close[bar - 1]));

                // Save bar when "keepall" is true
                if (keepall == true) keepallBar = bar; else keepallBar = 0;

                keep3 = ((bars.Close - bars.Open).Abs()[bar] < ((bars.High[bar] - bars.Low[bar]) * CandleSize)) &
                    (bars.High[bar] >= bars.Low[bar - 1]);

                utr = keepall ||
                    // Implements Ref(keepall,-1)
                    ((keepallBar > 0) & (bar == keepallBar + 1) & keep3);

                if (utr == true) utrBar = bar; else utrBar = 0;


                /* Create red candle */

                // Metastock Alert function resource:
                // http://www.meta-formula.com/metastock-alert-function.html

                if (!keep1d)
                {
                    if (haC[bar] < HO[bar])
                    {
                        keep1d = true;
                        setup2bar = bar;
                    }
                }
                if (keep1d)
                {
                    keep1d = bar + 1 - setup2bar < setuptimeout;
                }

                keep2d = (ZlDif[bar] < 0);
                keep3d = ((bars.Close - bars.Open).Abs()[bar] < ((bars.High[bar] - bars.Low[bar]) * CandleSize)) &
                    (bars.Low[bar] <= bars.High[bar - 1]);

                keepingd = (keep1d || keep2d);

                // Save bar when "keeping" is true
                if (keepingd) keepingdBar = bar; else keepingdBar = 0;

                keepalld = keepingd || (
                    // Implements Ref(keeping,-1)
                    ((keepingdBar > 0) & (bar == keepingdBar + 1)) &
                    (bars.Close[bar] < bars.Open[bar]) | (bars.Close[bar] < bars.Close[bar - 1]));

                // Save bar when "keepall" is true
                if (keepalld == true) keepallBard = bar; else keepallBard = 0;

                dtr = keepalld ||
                    // Implements Ref(keepall,-1)
                    ((keepallBard > 0) & ((bar == keepallBard + 1) & keep3d));

                if (dtr == true) dtrBar = bar; else dtrBar = 0;

                upw = !dtr || (((dtrBar > 0) & (bar == dtrBar + 1)) & utr);
                dnw = !utr || (((utrBar > 0) & (bar == utrBar + 1)) & dtr);

                /* HACOLT */

                //ValueWhen
                //http://www.meta-formula.com/metastock-valuewhen-function.html
                //Result:=If(upw,1, If(dnw,0, ValueWhen(1,upw+dnw,If(upw,1,0))));
                
                if (upw)
                {
                    i_result = 1;
                    lastOccurenceForLTResult = 1;
                }
                else
                    if (dnw)
                    {
                        i_result = 0;
                        lastOccurenceForLTResult = 0;
                    }
                    else
                    {
                        if (lastOccurenceForLTResult > -1)
                            i_result = lastOccurenceForLTResult;
                    }

                bool LTSell = bars.Close[bar] < LTAverage[bar];
                LTResult = (i_result == 1) ? 1 : (i_result == 0 && LTSell) ? 0 : LTResult;
                final_result = (i_result == 1) ? 100 : (i_result == 0 && LTResult == 1) ? 50 : (i_result == 0 && LTResult == 0) ? 0 : final_result;

                Values[bar] = final_result;
            }
        }


        public override string Name => "HACOLT";

        public override string Abbreviation => "HACOLT";

        public override string HelpDescription => "HACOLT (Heikin-Ashi Candlestick Oscillator Long-Term) by Sylvain Vervoort from the July 2012 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"HACO";

        public override Color DefaultColor => Color.BlueViolet;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}