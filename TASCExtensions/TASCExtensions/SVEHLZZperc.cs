using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCIndicators
{
    public enum SVEHLZZperc_Type { Percent, ATR, Combined, Point }

    public class SVEHLZZperc : IndicatorBase
    {
        public override string Name => "SVEHLZZperc";

        public override string Abbreviation => "SVEHLZZperc";

        public override string HelpDescription => "Sylvain Vervoort's SVEHLZZperc zigzag indicator from June 2013 issue of Stocks & Commodities magazine is a trailing reverse indicator that is based on a percent price change between high and low prices, or uses an ATR volatility factor, or both of them combined.";

        public override string PaneTag => @"SVEHLZZperc";

        public override Color DefaultColor => Color.Blue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;

        //parameterless constructor
        public SVEHLZZperc() : base()
        {
        }

        //for code based construction
        public SVEHLZZperc(BarHistory bars, double change, int period, double factor, SVEHLZZperc_Type type)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = change;
            Parameters[2].Value = period;
            Parameters[3].Value = factor;
            Parameters[4].Value = type;

            Populate();            
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Price Change %", ParameterTypes.Double, 5);
            AddParameter("ATR Period", ParameterTypes.Int32, 5);
            AddParameter("ATR Factor", ParameterTypes.Double, 1.5);
            Parameter p = AddParameter("Type", ParameterTypes.StringChoice, "ATR");
            p.Choices.Add("Percent");
            p.Choices.Add("ATR");
            p.Choices.Add("Combined");
            p.TypeName = "SVEHLZZperc_Type";
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Double change = Parameters[1].AsDouble;
            Int32 period = Parameters[2].AsInt;
            Double factor = Parameters[3].AsDouble;
            SVEHLZZperc_Type type = (SVEHLZZperc_Type)Enum.Parse(typeof(SVEHLZZperc_Type), Parameters[4].AsString);

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;
            
            int Trend = 0;
            double Reverse = 0, HPrice = 0, LPrice = 0;

            ATR atr = new ATR(bars, period);

            for (int bar = period; bar < bars.Count; bar++)
            {
                double atrValue = atr[bar] * factor;

                if (Trend >= 0)
                {
                    HPrice = Math.Max(bars.High[bar], HPrice);
                    switch (type)
                    {
                        case SVEHLZZperc_Type.Percent:
                            Reverse = HPrice * (1 - change * 0.01);
                            break;
                        case SVEHLZZperc_Type.ATR:
                            Reverse = HPrice - atrValue;
                            break;
                        case SVEHLZZperc_Type.Combined:
                            Reverse = HPrice - (HPrice * (change * 0.01) + atrValue);
                            break;
                        case SVEHLZZperc_Type.Point:
                            Reverse = HPrice - change * bars.TickSize;
                            break;
                        default:
                            break;
                    }

                    if (bars.Low[bar] <= Reverse)
                    {
                        Trend = -1;
                        LPrice = bars.Low[bar];
                        
                        switch (type)
                        {
                            case SVEHLZZperc_Type.Percent:
                                Reverse = LPrice * (1 + change * 0.01);
                                break;
                            case SVEHLZZperc_Type.ATR:
                                Reverse = LPrice + atrValue;
                                break;
                            case SVEHLZZperc_Type.Combined:
                                Reverse = LPrice + (atrValue + LPrice * (change * 0.01));
                                break;
                            case SVEHLZZperc_Type.Point:
                                Reverse = LPrice + change * bars.TickSize;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (Trend <= 0)
                {
                    LPrice = Math.Min(bars.Low[bar], LPrice);
                    switch (type)
                    {
                        case SVEHLZZperc_Type.Percent:
                            Reverse = LPrice * (1 + change * 0.01);
                            break;
                        case SVEHLZZperc_Type.ATR:
                            Reverse = LPrice + atrValue;
                            break;
                        case SVEHLZZperc_Type.Combined:
                            Reverse = LPrice + (atrValue + LPrice * (change * 0.01));
                            break;
                        case SVEHLZZperc_Type.Point:
                            Reverse = LPrice + change * bars.TickSize;
                            break;
                        default:
                            break;
                    }
                    
                    if (bars.High[bar] >= Reverse)
                    {
                        Trend = 1;
                        HPrice = bars.High[bar];

                        switch (type)
                        {
                            case SVEHLZZperc_Type.Percent:
                                Reverse = HPrice * (1 - change * 0.01);
                                break;
                            case SVEHLZZperc_Type.ATR:
                                Reverse = HPrice - atrValue;
                                break;
                            case SVEHLZZperc_Type.Combined:
                                Reverse = HPrice - (HPrice * (change * 0.01) + atrValue);
                                break;
                            case SVEHLZZperc_Type.Point:
                                Reverse = HPrice - change * bars.TickSize;
                                break;
                            default:
                                break;
                        }
                    }
                }
                Values[bar] = Reverse;
            }
        }
    }
}