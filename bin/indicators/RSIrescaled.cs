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
	public class RSIrescaled : Indicator
	{
		public Series<double> RSI_series;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "RSIrescaled";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event.
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Period					= 14;
				Smoothing					= 3;
				N_bars					= 255;
				AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Square, "RSI_rescaled");
			}
			else if (State == State.DataLoaded)
			{
				RSI_series = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar <  1) return;
			RSI_series[0] = RSI(Input, Period, Smoothing)[0];
			RSI_series[0] = RSI_series[0] - (SUM(RSI(Input, Period, Smoothing),N_bars)[0] / N_bars);

			RSI_rescaled[0] = RSI_series[0] / MAX(RSI_series,N_bars)[0];
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smoothing", Order=2, GroupName="Parameters")]
		public int Smoothing
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="N_bars", Order=3, GroupName="Parameters")]
		public int N_bars
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RSI_rescaled
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RSIrescaled[] cacheRSIrescaled;
		public RSIrescaled RSIrescaled(int period, int smoothing, int n_bars)
		{
			return RSIrescaled(Input, period, smoothing, n_bars);
		}

		public RSIrescaled RSIrescaled(ISeries<double> input, int period, int smoothing, int n_bars)
		{
			if (cacheRSIrescaled != null)
				for (int idx = 0; idx < cacheRSIrescaled.Length; idx++)
					if (cacheRSIrescaled[idx] != null && cacheRSIrescaled[idx].Period == period && cacheRSIrescaled[idx].Smoothing == smoothing && cacheRSIrescaled[idx].N_bars == n_bars && cacheRSIrescaled[idx].EqualsInput(input))
						return cacheRSIrescaled[idx];
			return CacheIndicator<RSIrescaled>(new RSIrescaled(){ Period = period, Smoothing = smoothing, N_bars = n_bars }, input, ref cacheRSIrescaled);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RSIrescaled RSIrescaled(int period, int smoothing, int n_bars)
		{
			return indicator.RSIrescaled(Input, period, smoothing, n_bars);
		}

		public Indicators.RSIrescaled RSIrescaled(ISeries<double> input , int period, int smoothing, int n_bars)
		{
			return indicator.RSIrescaled(input, period, smoothing, n_bars);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RSIrescaled RSIrescaled(int period, int smoothing, int n_bars)
		{
			return indicator.RSIrescaled(Input, period, smoothing, n_bars);
		}

		public Indicators.RSIrescaled RSIrescaled(ISeries<double> input , int period, int smoothing, int n_bars)
		{
			return indicator.RSIrescaled(input, period, smoothing, n_bars);
		}
	}
}

#endregion

