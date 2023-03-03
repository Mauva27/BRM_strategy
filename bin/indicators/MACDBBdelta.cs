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
	public class MACDBBdelta : Indicator
	{
		public	Series<double>		macs;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MACDBBdelta";
				Calculate									= Calculate.OnEachTick;
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
				Fast					= 12;
				Slow					= 26;
				Smoothing				= 9;
				AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Square, "Deltas");
				AddLine(Brushes.DarkGray,0, "Zeroline");
				AddLine(Brushes.Teal,0.25, "Upthres");
				AddLine(Brushes.Teal,-0.25, "LowThres");
			}
			else if (State == State.DataLoaded)
			{
				macs = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < 1) return;
			double mac 	= MACD(Input, Fast, Slow, Smoothing)[0];
			macs[0] 	= mac;
			Deltas[0] 	= macs[0] - macs[1];
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast", Order=1, GroupName="Parameters")]
		public int Fast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow", Order=2, GroupName="Parameters")]
		public int Slow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smoothing", Order=3, GroupName="Parameters")]
		public int Smoothing
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Deltas
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Zeroline
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upthres
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowThres
		{
			get { return Values[3]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MACDBBdelta[] cacheMACDBBdelta;
		public MACDBBdelta MACDBBdelta(int fast, int slow, int smoothing)
		{
			return MACDBBdelta(Input, fast, slow, smoothing);
		}

		public MACDBBdelta MACDBBdelta(ISeries<double> input, int fast, int slow, int smoothing)
		{
			if (cacheMACDBBdelta != null)
				for (int idx = 0; idx < cacheMACDBBdelta.Length; idx++)
					if (cacheMACDBBdelta[idx] != null && cacheMACDBBdelta[idx].Fast == fast && cacheMACDBBdelta[idx].Slow == slow && cacheMACDBBdelta[idx].Smoothing == smoothing && cacheMACDBBdelta[idx].EqualsInput(input))
						return cacheMACDBBdelta[idx];
			return CacheIndicator<MACDBBdelta>(new MACDBBdelta(){ Fast = fast, Slow = slow, Smoothing = smoothing }, input, ref cacheMACDBBdelta);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACDBBdelta MACDBBdelta(int fast, int slow, int smoothing)
		{
			return indicator.MACDBBdelta(Input, fast, slow, smoothing);
		}

		public Indicators.MACDBBdelta MACDBBdelta(ISeries<double> input , int fast, int slow, int smoothing)
		{
			return indicator.MACDBBdelta(input, fast, slow, smoothing);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACDBBdelta MACDBBdelta(int fast, int slow, int smoothing)
		{
			return indicator.MACDBBdelta(Input, fast, slow, smoothing);
		}

		public Indicators.MACDBBdelta MACDBBdelta(ISeries<double> input , int fast, int slow, int smoothing)
		{
			return indicator.MACDBBdelta(input, fast, slow, smoothing);
		}
	}
}

#endregion

