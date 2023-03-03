#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class BRM : Indicator
	{	
		public Series<double> Basis;
		public Series<double> Basis_delta;
		public Series<double> Basis_norm;
		public Series<double> BRM_deltas;
		public Series<double> RSI_rescaled;
		public Series<double> RSI_norm;
		public Series<double> MACDBB;
		public Series<double> MACDBB_delta;
		public Series<double> MACDBB_norm;
		public Series<double> Basis_MACDBB_RSI;
		public Series<double> Basis_MACDBB_RSI_abs;
		public Series<double> EMA12;
		public Series<double> EMA26;
		public Series<double> Band_avg;
		public Series<double> Band_std;
		public Series<double> Band_avg_norm;
		public Series<double> Band_std_norm;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Momentum indicator based on a moving avergage basis, MACD and RSI contributions";
				Name						= "BRM";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= false;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive	= true;
				Basis_period				= 14;
				RSI_period					= 14;
				RSI_smooth					= 3;
				MACD_period					= 10;
				MACD_fast					= 12;
				MACD_slow					= 26;
				MACD_smooth					= 9;
				Bands_period				= 14;
				Bands_std					= 1;
				N_bars						= 256;
				
				UpColor						= Brushes.DodgerBlue;
				DownColor					= Brushes.Red;
				FillColor					= Brushes.Yellow;
				Band_oppacity				= 25;

				
				AddPlot(new Stroke(Brushes.Gray, 5), PlotStyle.Dot, "BRM_indicator");
				AddPlot(Brushes.Silver, "BRM_upper_band");
				AddPlot(Brushes.Silver, "BRM_lower_band");
				AddLine(Brushes.White, 0, "ZeroLine");				
			}
			
			else if (State == State.DataLoaded)
			{
				Band_avg			= new Series<double>(this);
				Band_std			= new Series<double>(this);
				Band_avg_norm		= new Series<double>(this);
				Band_std_norm		= new Series<double>(this);
				Basis				= new Series<double>(this);
				Basis_delta 		= new Series<double>(this);
				Basis_norm 			= new Series<double>(this);
				Basis_MACDBB_RSI 	= new Series<double>(this);
				Basis_MACDBB_RSI_abs= new Series<double>(this);
				BRM_deltas			= new Series<double>(this);
				EMA12 				= new Series<double>(this);
				EMA26 				= new Series<double>(this);	
				MACDBB 				= new Series<double>(this);
				MACDBB_delta 		= new Series<double>(this);
				MACDBB_norm 		= new Series<double>(this);
				RSI_rescaled 		= new Series<double>(this);
				RSI_norm 			= new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < 1) 
			{
				return;
			}
			else 
			{
				Draw.Region(this, "BRM_BB", CurrentBar, 0, BRM_lower_band, BRM_upper_band, null, FillColor, Band_oppacity,0);
				
				EMA12[0] 				= EMA(Input,MACD_fast)[0];
				EMA26[0] 				= EMA(Input,MACD_slow)[0];
				MACDBB[0] 				= MACD(Input,MACD_fast,MACD_slow,MACD_smooth)[0];

				Basis[0]				= EMAdeltas(Input,Basis_period)[0];
				Basis_delta[0] 			= EMAdeltas(Basis_period)[0];
				Basis_norm[0] 			= Basis_delta[0] / MAX(Basis_delta,N_bars)[0];

				MACDBB_delta[0] 		= MACDBBdelta(MACD_slow,MACD_fast,MACD_smooth)[0];
				MACDBB_norm[0] 			= MACDBB_delta[0] / MAX(MACDBB_delta,N_bars)[0];

				RSI_rescaled[0] 		= RSIrescaled(RSI_period,RSI_smooth,N_bars)[0];

				Basis_MACDBB_RSI[0] 	= RSI_rescaled[0] + Basis_norm[0] +  MACDBB_norm[0];
				Basis_MACDBB_RSI_abs[0] = Math.Abs(Basis_MACDBB_RSI[0]);
				BRM_indicator[0] 		= Basis_MACDBB_RSI[0] / MAX(Basis_MACDBB_RSI_abs,N_bars)[0];
				BRM_deltas[0]			= BRM_indicator[0] -  BRM_indicator[1];

				Band_avg[0]				= EMA(Basis_norm ,Bands_period)[0];
				Band_std[0]				= StdDev(Basis_norm ,Bands_period)[0];
				
				Band_avg_norm[0]		= Band_avg[0] / MAX(Band_avg,N_bars)[0];
				Band_std_norm[0]		= Band_std[0] / MAX(Band_std,N_bars)[0];
				
				BRM_upper_band[0]		= Band_avg[0] + (Band_std[0] * Bands_std);
				BRM_lower_band[0]		= Band_avg[0] - (Band_std[0] * Bands_std);
			
				
				// Plot indicator signals

				if ( //Sigal when slope becomes positive and indicator crosses lower band
					BRM_deltas[2] 		< 0 
					&& 
					BRM_deltas[1] 		< BRM_deltas[0] 
					&& 
					BRM_deltas[0] 		>= 0 
					&& 
					BRM_indicator[0] 	> BRM_indicator[1]
					&&
					BRM_lower_band[1] 	> BRM_indicator[1]
					)
				{
					PlotBrushes[0][0] 	= FillColor;
				}
				else if ( //Sigal when slope becomes negative and indicator crosses upper band
					BRM_deltas[2] 		> 0 
					&& 
					BRM_deltas[1] 		> BRM_deltas[0] 
					&& 
					BRM_deltas[0] 		<= 0 
					&& 
					BRM_indicator[0] 	< BRM_indicator[1]
					&&
					BRM_upper_band[1] 	< BRM_indicator[1]
					)
				{
					PlotBrushes[0][0] 	= FillColor;
				}
				else if (BRM_indicator[0]> BRM_upper_band[0])
				{
					PlotBrushes[0][0] 	= DownColor;
				}
				else if (BRM_indicator[0] < BRM_lower_band[0])
				{
					PlotBrushes[0][0] 	= UpColor;
				}
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Basis_period", Order=1, GroupName="Parameters")]
		public int Basis_period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI_period", Order=2, GroupName="Parameters")]
		public int RSI_period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI_smooth", Order=3, GroupName="Parameters")]
		public int RSI_smooth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACD_period", Order=4, GroupName="Parameters")]
		public int MACD_period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACD_fast", Order=5, GroupName="Parameters")]
		public int MACD_fast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACD_slow", Order=6, GroupName="Parameters")]
		public int MACD_slow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACD_smooth", Order=7, GroupName="Parameters")]
		public int MACD_smooth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Bands_period", Order=8, GroupName="Parameters")]
		public int Bands_period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="Bands_std", Order=9, GroupName="Parameters")]
		public double Bands_std
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="N_bars", Order=10, GroupName="Parameters")]
		public int N_bars
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="UpColor", Order=2, GroupName="Plots")]
		public Brush UpColor
		{ get; set; }

		[Browsable(false)]
		public string UpColorSerializable
		{
			get { return Serialize.BrushToString(UpColor); }
			set { UpColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="DownColor", Order=3, GroupName="Plots")]
		public Brush DownColor
		{ get; set; }

		[Browsable(false)]
		public string DownColorSerializable
		{
			get { return Serialize.BrushToString(DownColor); }
			set { DownColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Alert Color", Order=4, GroupName="Plots")]
		public Brush FillColor
		{ get; set; }

		[Browsable(false)]
		public string FillColorSerializable
		{
			get { return Serialize.BrushToString(FillColor); }
			set { FillColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Band_oppacity", Order=5, GroupName="Plots")]
		public int Band_oppacity
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BRM_indicator
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BRM_upper_band
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BRM_lower_band
		{
			get { return Values[2]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BRM[] cacheBRM;
		public BRM BRM(int basis_period, int rSI_period, int rSI_smooth, int mACD_period, int mACD_fast, int mACD_slow, int mACD_smooth, int bands_period, double bands_std, int n_bars, Brush upColor, Brush downColor, Brush fillColor, int band_oppacity)
		{
			return BRM(Input, basis_period, rSI_period, rSI_smooth, mACD_period, mACD_fast, mACD_slow, mACD_smooth, bands_period, bands_std, n_bars, upColor, downColor, fillColor, band_oppacity);
		}

		public BRM BRM(ISeries<double> input, int basis_period, int rSI_period, int rSI_smooth, int mACD_period, int mACD_fast, int mACD_slow, int mACD_smooth, int bands_period, double bands_std, int n_bars, Brush upColor, Brush downColor, Brush fillColor, int band_oppacity)
		{
			if (cacheBRM != null)
				for (int idx = 0; idx < cacheBRM.Length; idx++)
					if (cacheBRM[idx] != null && cacheBRM[idx].Basis_period == basis_period && cacheBRM[idx].RSI_period == rSI_period && cacheBRM[idx].RSI_smooth == rSI_smooth && cacheBRM[idx].MACD_period == mACD_period && cacheBRM[idx].MACD_fast == mACD_fast && cacheBRM[idx].MACD_slow == mACD_slow && cacheBRM[idx].MACD_smooth == mACD_smooth && cacheBRM[idx].Bands_period == bands_period && cacheBRM[idx].Bands_std == bands_std && cacheBRM[idx].N_bars == n_bars && cacheBRM[idx].UpColor == upColor && cacheBRM[idx].DownColor == downColor && cacheBRM[idx].FillColor == fillColor && cacheBRM[idx].Band_oppacity == band_oppacity && cacheBRM[idx].EqualsInput(input))
						return cacheBRM[idx];
			return CacheIndicator<BRM>(new BRM(){ Basis_period = basis_period, RSI_period = rSI_period, RSI_smooth = rSI_smooth, MACD_period = mACD_period, MACD_fast = mACD_fast, MACD_slow = mACD_slow, MACD_smooth = mACD_smooth, Bands_period = bands_period, Bands_std = bands_std, N_bars = n_bars, UpColor = upColor, DownColor = downColor, FillColor = fillColor, Band_oppacity = band_oppacity }, input, ref cacheBRM);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BRM BRM(int basis_period, int rSI_period, int rSI_smooth, int mACD_period, int mACD_fast, int mACD_slow, int mACD_smooth, int bands_period, double bands_std, int n_bars, Brush upColor, Brush downColor, Brush fillColor, int band_oppacity)
		{
			return indicator.BRM(Input, basis_period, rSI_period, rSI_smooth, mACD_period, mACD_fast, mACD_slow, mACD_smooth, bands_period, bands_std, n_bars, upColor, downColor, fillColor, band_oppacity);
		}

		public Indicators.BRM BRM(ISeries<double> input , int basis_period, int rSI_period, int rSI_smooth, int mACD_period, int mACD_fast, int mACD_slow, int mACD_smooth, int bands_period, double bands_std, int n_bars, Brush upColor, Brush downColor, Brush fillColor, int band_oppacity)
		{
			return indicator.BRM(input, basis_period, rSI_period, rSI_smooth, mACD_period, mACD_fast, mACD_slow, mACD_smooth, bands_period, bands_std, n_bars, upColor, downColor, fillColor, band_oppacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BRM BRM(int basis_period, int rSI_period, int rSI_smooth, int mACD_period, int mACD_fast, int mACD_slow, int mACD_smooth, int bands_period, double bands_std, int n_bars, Brush upColor, Brush downColor, Brush fillColor, int band_oppacity)
		{
			return indicator.BRM(Input, basis_period, rSI_period, rSI_smooth, mACD_period, mACD_fast, mACD_slow, mACD_smooth, bands_period, bands_std, n_bars, upColor, downColor, fillColor, band_oppacity);
		}

		public Indicators.BRM BRM(ISeries<double> input , int basis_period, int rSI_period, int rSI_smooth, int mACD_period, int mACD_fast, int mACD_slow, int mACD_smooth, int bands_period, double bands_std, int n_bars, Brush upColor, Brush downColor, Brush fillColor, int band_oppacity)
		{
			return indicator.BRM(input, basis_period, rSI_period, rSI_smooth, mACD_period, mACD_fast, mACD_slow, mACD_smooth, bands_period, bands_std, n_bars, upColor, downColor, fillColor, band_oppacity);
		}
	}
}

#endregion

