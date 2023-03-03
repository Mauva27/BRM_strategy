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
	public class EMAdeltas : Indicator
	{
		public	Series<double>		emad;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "EMAdeltas";
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
				Period										= 50;
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Square, "EMADelta");
			}
			else if (State == State.Configure)
			{
				emad = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < 1) return;
			double ema 	= EMA(Input,Period)[0];
			emad[0] 	= ema;
			EMADelta[0] = emad[0] - emad[1];
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMADelta
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
		private EMAdeltas[] cacheEMAdeltas;
		public EMAdeltas EMAdeltas(int period)
		{
			return EMAdeltas(Input, period);
		}

		public EMAdeltas EMAdeltas(ISeries<double> input, int period)
		{
			if (cacheEMAdeltas != null)
				for (int idx = 0; idx < cacheEMAdeltas.Length; idx++)
					if (cacheEMAdeltas[idx] != null && cacheEMAdeltas[idx].Period == period && cacheEMAdeltas[idx].EqualsInput(input))
						return cacheEMAdeltas[idx];
			return CacheIndicator<EMAdeltas>(new EMAdeltas(){ Period = period }, input, ref cacheEMAdeltas);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.EMAdeltas EMAdeltas(int period)
		{
			return indicator.EMAdeltas(Input, period);
		}

		public Indicators.EMAdeltas EMAdeltas(ISeries<double> input , int period)
		{
			return indicator.EMAdeltas(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.EMAdeltas EMAdeltas(int period)
		{
			return indicator.EMAdeltas(Input, period);
		}

		public Indicators.EMAdeltas EMAdeltas(ISeries<double> input , int period)
		{
			return indicator.EMAdeltas(input, period);
		}
	}
}

#endregion

