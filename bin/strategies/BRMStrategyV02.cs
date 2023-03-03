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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class BRMStrategyV02 : Strategy
	{
    private Order long_entry_order      = null;
    private Order long_scalp_order      = null; 
    private Order short_entry_order     = null;
    private Order short_scalp_order     = null; 
    private Order profit_order          = null;
    private Order profit_scalp_order    = null; 
    private Order stop_loss_order       = null;
    private Order stop_loss_scalp_order = null;
    private int sum_filled              = 0;

    private BRM brm;
    private Series<double> BRM_deltas;


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Second version of the BRM strategy that includes trailing stop orders";
				Name										    = "BRMStrategyV02";
				Calculate									  = Calculate.OnBarClose;
				EntriesPerDirection					= 2;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy= true;
				ExitOnSessionCloseSeconds		= 30;
				IsFillLimitOnTouch					= false;
				MaximumBarsLookBack					= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution					= OrderFillResolution.Standard;
				Slippage									  = 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling				= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling					= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade					= 256;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= false;
				// Open_time						        = DateTime.Parse("14:30", System.Globalization.CultureInfo.InvariantCulture);
				// Close_time						      = DateTime.Parse("20:45", System.Globalization.CultureInfo.InvariantCulture);
				Scalp_profit_ticks					= 8;
				Runner_profit_ticks					= 16;
				Scalp_stop_ticks					  = 16;
				Runner_stop_ticks					  = 16;
				Quantity					          = 1;
				BRM_bands_std					      = 2;
				BRM_delta					          = 1;
				Entry_ticks					        = 1;
			}
			else if (State == State.Configure)
			{
        // Long orders
				SetParabolicStop(@"Long_runner_entry", CalculationMode.Ticks, Runner_stop_ticks, false, 0.02, 0.2, 0.02);
				SetProfitTarget(@"Long_runner_entry", CalculationMode.Ticks, Runner_profit_ticks);
				
        SetParabolicStop(@"Long_scalp_entry", CalculationMode.Ticks, Scalp_stop_ticks, false, 0.02, 0.02, 0.2);
				SetProfitTarget(@"Long_scalp_entry", CalculationMode.Ticks, Scalp_profit_ticks);
			
        // Short orders
				SetParabolicStop(@"Short_runner_entry", CalculationMode.Ticks, Runner_stop_ticks, false, 0.02, 0.2, 0.02);
				SetProfitTarget(@"Short_runner_entry", CalculationMode.Ticks, Runner_profit_ticks);
				
        SetParabolicStop(@"Short_scalp_entry", CalculationMode.Ticks, Scalp_stop_ticks, false, 0.02, 0.02, 0.2);
				SetProfitTarget(@"Short_scalp_entry", CalculationMode.Ticks, Scalp_profit_ticks);
      }
      else if (State == State.DataLoaded)
      {
        brm				 = BRM(Close, 14, 14, 3, 10, 12, 26, 9, 14, BRM_bands_std, 256, Brushes.DodgerBlue, Brushes.Red, Brushes.Yellow, 25);
				BRM_deltas = new Series<double>(this); 
      }
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

      BRM_deltas[0]	= brm.BRM_indicator[0] -  brm.BRM_indicator[1];

      if (
        BRM_deltas[2] < 0 
        && 
        BRM_deltas[1] < BRM_deltas[0] 
        && 
        BRM_deltas[0] >= 0 
        && 
        brm.BRM_indicator[0] > brm.BRM_indicator[1]
        &&
        brm.BRM_indicator[1] < brm.BRM_lower_band[1] 
        &&
        brm.BRM_indicator[0] >= brm.BRM_lower_band[0] 
        &&
        brm.BRM_indicator[0] - brm.BRM_indicator[1] >= BRM_delta
        )
      {
				EnterLongStopLimit(Convert.ToInt32(Quantity), (High[0] + Entry_ticks * TickSize) , (High[0] + Entry_ticks * TickSize) , @"Long_runner_entry");
				EnterLongStopLimit(Convert.ToInt32(Quantity), (High[0] + Entry_ticks *TickSize) , (High[0] + Entry_ticks * TickSize) , @"Long_scalp_entry");
        
        // side      = "Long";
        // bar_count = CurrentBar;
      }
      else if (
        BRM_deltas[2] > 0 
        && 
        BRM_deltas[1] > BRM_deltas[0] 
        && 
        BRM_deltas[0] <= 0 
        && 
        brm.BRM_indicator[0] < brm.BRM_indicator[1]
        &&
        brm.BRM_indicator[1] > brm.BRM_upper_band[1]
        &&
        // brm.BRM_indicator[0] <= brm.BRM_upper_band[0]
        // &&
        brm.BRM_indicator[1] - brm.BRM_indicator[0] >= BRM_delta
        )
      {
				EnterShortStopLimit(Convert.ToInt32(Quantity), (Low[0] - Entry_ticks * TickSize) , (Low[0] - Entry_ticks * TickSize) , @"Short_runner_entry");
				EnterShortStopLimit(Convert.ToInt32(Quantity), (Low[0] - Entry_ticks * TickSize) , (Low[0] - Entry_ticks * TickSize) , @"Short_scalp_entry");
        
        // side      = "Short";
        // bar_count = CurrentBar;
      }
			
		}

		#region Properties
    [NinjaScriptProperty]
    [Range(1, int.MaxValue)]
    [Display(Name="Quantity", Description="Quantity", Order=1, GroupName="Parameters")]
    public int Quantity
    { get; set; }

    [NinjaScriptProperty]
    [Range(1, int.MaxValue)]
    [Display(Name="Entry_ticks", Description="Ticks from prev bar", Order=2, GroupName="Parameters")]
    public int Entry_ticks
    { get; set; }

    [NinjaScriptProperty]
    [Range(1, int.MaxValue)]
    [Display(Name="Profit_ticks", Description="Profit ticks", Order=3, GroupName="Parameters")]
    public int Runner_profit_ticks
    { get; set; }

    [NinjaScriptProperty]
    [Range(1, int.MaxValue)]
    [Display(Name="Stop_ticks", Description="Stop ticks", Order=4, GroupName="Parameters")]
    public int Runner_stop_ticks
    { get; set; }

    [NinjaScriptProperty]
    [Range(1, int.MaxValue)]
    [Display(Name="Scalp_profit_ticks", Description="Scalp Profit ticks", Order=5, GroupName="Parameters")]
    public int Scalp_profit_ticks
    { get; set; }

    [NinjaScriptProperty]
    [Range(1, int.MaxValue)]
    [Display(Name="Scalp_stop_ticks", Description="Scalp Stop ticks", Order=6, GroupName="Parameters")]
    public int Scalp_stop_ticks
    { get; set; }

    [NinjaScriptProperty]
    [Range(0.5, double.MaxValue)]
    [Display(Name="BRM_bands_std", Description="Indicator BB Std", Order=7, GroupName="Parameters")]
    public double BRM_bands_std
    { get; set; }

    [NinjaScriptProperty]
    [Range(0.015, double.MaxValue)]
    [Display(Name="BRM_delta", Description="Indicator Delta", Order=8, GroupName="Parameters")]
    public double BRM_delta
    { get; set; }

		// [NinjaScriptProperty]
		// [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		// [Display(Name="Open_time", Description="Open time in GMT", Order=9, GroupName="Parameters")]
		// public DateTime Open_time
		// { get; set; }

		// [NinjaScriptProperty]
		// [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		// [Display(Name="Close_time", Description="Close time in GMT", Order=10, GroupName="Parameters")]
		// public DateTime Close_time
		// { get; set; }
		#endregion

	}
}

#region Wizard settings, neither change nor remove
/*@
<?xml version="1.0"?>
<ScriptProperties xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Calculate>OnBarClose</Calculate>
  <ConditionalActions>
    <ConditionalAction>
      <Actions />
      <AnyOrAll>All</AnyOrAll>
      <Conditions />
      <SetName>Set 1</SetName>
      <SetNumber>1</SetNumber>
    </ConditionalAction>
  </ConditionalActions>
  <CustomSeries />
  <DataSeries />
  <Description>Second version of the BRM strategy that includes trailing stop orders</Description>
  <DisplayInDataBox>true</DisplayInDataBox>
  <DrawHorizontalGridLines>true</DrawHorizontalGridLines>
  <DrawOnPricePanel>true</DrawOnPricePanel>
  <DrawVerticalGridLines>true</DrawVerticalGridLines>
  <EntriesPerDirection>1</EntriesPerDirection>
  <EntryHandling>AllEntries</EntryHandling>
  <ExitOnSessionClose>true</ExitOnSessionClose>
  <ExitOnSessionCloseSeconds>30</ExitOnSessionCloseSeconds>
  <FillLimitOrdersOnTouch>false</FillLimitOrdersOnTouch>
  <InputParameters>
    <InputParameter>
      <Default>14:30</Default>
      <Description>Open time in GMT</Description>
      <Name>Open_time</Name>
      <Minimum />
      <Type>time</Type>
    </InputParameter>
    <InputParameter>
      <Default>20:45</Default>
      <Description>Close time in GMT</Description>
      <Name>Close_time</Name>
      <Minimum />
      <Type>time</Type>
    </InputParameter>
  </InputParameters>
  <IsTradingHoursBreakLineVisible>true</IsTradingHoursBreakLineVisible>
  <IsInstantiatedOnEachOptimizationIteration>true</IsInstantiatedOnEachOptimizationIteration>
  <MaximumBarsLookBack>TwoHundredFiftySix</MaximumBarsLookBack>
  <MinimumBarsRequired>20</MinimumBarsRequired>
  <OrderFillResolution>Standard</OrderFillResolution>
  <OrderFillResolutionValue>1</OrderFillResolutionValue>
  <OrderFillResolutionType>Minute</OrderFillResolutionType>
  <OverlayOnPrice>false</OverlayOnPrice>
  <PaintPriceMarkers>true</PaintPriceMarkers>
  <PlotParameters />
  <RealTimeErrorHandling>StopCancelClose</RealTimeErrorHandling>
  <ScaleJustification>Right</ScaleJustification>
  <ScriptType>Strategy</ScriptType>
  <Slippage>0</Slippage>
  <StartBehavior>WaitUntilFlat</StartBehavior>
  <StopsAndTargets>
    <WizardAction>
      <Children />
      <IsExpanded>false</IsExpanded>
      <IsSelected>true</IsSelected>
      <Name>Parabolic stop</Name>
      <OffsetType>Arithmetic</OffsetType>
      <ActionProperties>
        <Acceleration>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">0.02</BindingValue>
          <IsLiteral>true</IsLiteral>
          <LiveValue xsi:type="xsd:string">0.02</LiveValue>
        </Acceleration>
        <AccelerationMax>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">0.2</BindingValue>
          <IsLiteral>true</IsLiteral>
          <LiveValue xsi:type="xsd:string">0.2</LiveValue>
        </AccelerationMax>
        <AccelerationStep>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">0.02</BindingValue>
          <IsLiteral>true</IsLiteral>
          <LiveValue xsi:type="xsd:string">0.02</LiveValue>
        </AccelerationStep>
        <DashStyle>Solid</DashStyle>
        <DivideTimePrice>false</DivideTimePrice>
        <Id />
        <File />
        <FromEntrySignal>
          <SeparatorCharacter> </SeparatorCharacter>
          <Strings>
            <NinjaScriptString>
              <Index>0</Index>
              <StringValue>Runner_entry</StringValue>
            </NinjaScriptString>
          </Strings>
        </FromEntrySignal>
        <IsAutoScale>false</IsAutoScale>
        <IsSimulatedStop>false</IsSimulatedStop>
        <IsStop>false</IsStop>
        <LogLevel>Information</LogLevel>
        <Mode>Ticks</Mode>
        <OffsetType>Currency</OffsetType>
        <Priority>Medium</Priority>
        <Quantity>
          <DefaultValue>0</DefaultValue>
          <IsInt>true</IsInt>
          <BindingValue xsi:type="xsd:string">DefaultQuantity</BindingValue>
          <DynamicValue>
            <Children />
            <IsExpanded>false</IsExpanded>
            <IsSelected>false</IsSelected>
            <Name>Default order quantity</Name>
            <OffsetType>Arithmetic</OffsetType>
            <AssignedCommand>
              <Command>DefaultQuantity</Command>
              <Parameters />
            </AssignedCommand>
            <BarsAgo>0</BarsAgo>
            <CurrencyType>Currency</CurrencyType>
            <Date>2023-02-28T22:03:53.6522686</Date>
            <DayOfWeek>Sunday</DayOfWeek>
            <EndBar>0</EndBar>
            <ForceSeriesIndex>false</ForceSeriesIndex>
            <LookBackPeriod>0</LookBackPeriod>
            <MarketPosition>Long</MarketPosition>
            <Period>0</Period>
            <ReturnType>Number</ReturnType>
            <StartBar>0</StartBar>
            <State>Undefined</State>
            <Time>0001-01-01T00:00:00</Time>
          </DynamicValue>
          <IsLiteral>false</IsLiteral>
          <LiveValue xsi:type="xsd:string">DefaultQuantity</LiveValue>
        </Quantity>
        <ServiceName />
        <ScreenshotPath />
        <SoundLocation />
        <Tag>
          <SeparatorCharacter> </SeparatorCharacter>
          <Strings>
            <NinjaScriptString>
              <Index>0</Index>
              <StringValue>Set Parabolic stop</StringValue>
            </NinjaScriptString>
          </Strings>
        </Tag>
        <TextPosition>BottomLeft</TextPosition>
        <Value>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">Runner_stop_ticks</BindingValue>
          <DynamicValue>
            <Children />
            <IsExpanded>false</IsExpanded>
            <IsSelected>true</IsSelected>
            <Name>Runner_stop_ticks</Name>
            <OffsetType>Arithmetic</OffsetType>
            <AssignedCommand>
              <Command>Runner_stop_ticks</Command>
              <Parameters />
            </AssignedCommand>
            <BarsAgo>0</BarsAgo>
            <CurrencyType>Currency</CurrencyType>
            <Date>2023-02-28T22:16:23.6252186</Date>
            <DayOfWeek>Sunday</DayOfWeek>
            <EndBar>0</EndBar>
            <ForceSeriesIndex>false</ForceSeriesIndex>
            <LookBackPeriod>0</LookBackPeriod>
            <MarketPosition>Long</MarketPosition>
            <Period>0</Period>
            <ReturnType>Number</ReturnType>
            <StartBar>0</StartBar>
            <State>Undefined</State>
            <Time>0001-01-01T00:00:00</Time>
          </DynamicValue>
          <IsLiteral>false</IsLiteral>
          <LiveValue xsi:type="xsd:string">Runner_stop_ticks</LiveValue>
        </Value>
        <VariableDateTime>2023-02-28T22:03:53.6642623</VariableDateTime>
        <VariableBool>false</VariableBool>
      </ActionProperties>
      <ActionType>Misc</ActionType>
      <Command>
        <Command>SetParabolicStop</Command>
        <Parameters>
          <string>fromEntrySignal</string>
          <string>mode</string>
          <string>value</string>
          <string>isSimulatedStop</string>
          <string>acceleration</string>
          <string>accelerationMax</string>
          <string>accelerationStep</string>
        </Parameters>
      </Command>
    </WizardAction>
    <WizardAction>
      <Children />
      <IsExpanded>false</IsExpanded>
      <IsSelected>true</IsSelected>
      <Name>Parabolic stop</Name>
      <OffsetType>Arithmetic</OffsetType>
      <ActionProperties>
        <Acceleration>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">0.02</BindingValue>
          <IsLiteral>true</IsLiteral>
          <LiveValue xsi:type="xsd:string">0.02</LiveValue>
        </Acceleration>
        <AccelerationMax>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">0.02</BindingValue>
          <IsLiteral>true</IsLiteral>
          <LiveValue xsi:type="xsd:string">0.02</LiveValue>
        </AccelerationMax>
        <AccelerationStep>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">0.2</BindingValue>
          <IsLiteral>true</IsLiteral>
          <LiveValue xsi:type="xsd:string">0.2</LiveValue>
        </AccelerationStep>
        <DashStyle>Solid</DashStyle>
        <DivideTimePrice>false</DivideTimePrice>
        <Id />
        <File />
        <FromEntrySignal>
          <SeparatorCharacter> </SeparatorCharacter>
          <Strings>
            <NinjaScriptString>
              <Index>0</Index>
              <StringValue>Scalp_entry</StringValue>
            </NinjaScriptString>
          </Strings>
        </FromEntrySignal>
        <IsAutoScale>false</IsAutoScale>
        <IsSimulatedStop>false</IsSimulatedStop>
        <IsStop>false</IsStop>
        <LogLevel>Information</LogLevel>
        <Mode>Ticks</Mode>
        <OffsetType>Currency</OffsetType>
        <Priority>Medium</Priority>
        <Quantity>
          <DefaultValue>0</DefaultValue>
          <IsInt>true</IsInt>
          <BindingValue xsi:type="xsd:string">DefaultQuantity</BindingValue>
          <DynamicValue>
            <Children />
            <IsExpanded>false</IsExpanded>
            <IsSelected>false</IsSelected>
            <Name>Default order quantity</Name>
            <OffsetType>Arithmetic</OffsetType>
            <AssignedCommand>
              <Command>DefaultQuantity</Command>
              <Parameters />
            </AssignedCommand>
            <BarsAgo>0</BarsAgo>
            <CurrencyType>Currency</CurrencyType>
            <Date>2023-02-28T22:16:49.1641431</Date>
            <DayOfWeek>Sunday</DayOfWeek>
            <EndBar>0</EndBar>
            <ForceSeriesIndex>false</ForceSeriesIndex>
            <LookBackPeriod>0</LookBackPeriod>
            <MarketPosition>Long</MarketPosition>
            <Period>0</Period>
            <ReturnType>Number</ReturnType>
            <StartBar>0</StartBar>
            <State>Undefined</State>
            <Time>0001-01-01T00:00:00</Time>
          </DynamicValue>
          <IsLiteral>false</IsLiteral>
          <LiveValue xsi:type="xsd:string">DefaultQuantity</LiveValue>
        </Quantity>
        <ServiceName />
        <ScreenshotPath />
        <SoundLocation />
        <Tag>
          <SeparatorCharacter> </SeparatorCharacter>
          <Strings>
            <NinjaScriptString>
              <Index>0</Index>
              <StringValue>Set Parabolic stop</StringValue>
            </NinjaScriptString>
          </Strings>
        </Tag>
        <TextPosition>BottomLeft</TextPosition>
        <Value>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">Scalp_stop_ticks</BindingValue>
          <DynamicValue>
            <Children />
            <IsExpanded>false</IsExpanded>
            <IsSelected>true</IsSelected>
            <Name>Scalp_stop_ticks</Name>
            <OffsetType>Arithmetic</OffsetType>
            <AssignedCommand>
              <Command>Scalp_stop_ticks</Command>
              <Parameters />
            </AssignedCommand>
            <BarsAgo>0</BarsAgo>
            <CurrencyType>Currency</CurrencyType>
            <Date>2023-02-28T22:17:28.1085249</Date>
            <DayOfWeek>Sunday</DayOfWeek>
            <EndBar>0</EndBar>
            <ForceSeriesIndex>false</ForceSeriesIndex>
            <LookBackPeriod>0</LookBackPeriod>
            <MarketPosition>Long</MarketPosition>
            <Period>0</Period>
            <ReturnType>Number</ReturnType>
            <StartBar>0</StartBar>
            <State>Undefined</State>
            <Time>0001-01-01T00:00:00</Time>
          </DynamicValue>
          <IsLiteral>false</IsLiteral>
          <LiveValue xsi:type="xsd:string">Scalp_stop_ticks</LiveValue>
        </Value>
        <VariableDateTime>2023-02-28T22:16:49.1651415</VariableDateTime>
        <VariableBool>false</VariableBool>
      </ActionProperties>
      <ActionType>Misc</ActionType>
      <Command>
        <Command>SetParabolicStop</Command>
        <Parameters>
          <string>fromEntrySignal</string>
          <string>mode</string>
          <string>value</string>
          <string>isSimulatedStop</string>
          <string>acceleration</string>
          <string>accelerationMax</string>
          <string>accelerationStep</string>
        </Parameters>
      </Command>
    </WizardAction>
    <WizardAction>
      <Children />
      <IsExpanded>false</IsExpanded>
      <IsSelected>true</IsSelected>
      <Name>Profit target</Name>
      <OffsetType>Arithmetic</OffsetType>
      <ActionProperties>
        <DashStyle>Solid</DashStyle>
        <DivideTimePrice>false</DivideTimePrice>
        <Id />
        <File />
        <FromEntrySignal>
          <SeparatorCharacter> </SeparatorCharacter>
          <Strings>
            <NinjaScriptString>
              <Index>0</Index>
              <StringValue>Runner_entry</StringValue>
            </NinjaScriptString>
          </Strings>
        </FromEntrySignal>
        <IsAutoScale>false</IsAutoScale>
        <IsSimulatedStop>false</IsSimulatedStop>
        <IsStop>false</IsStop>
        <LogLevel>Information</LogLevel>
        <Mode>Ticks</Mode>
        <OffsetType>Currency</OffsetType>
        <Priority>Medium</Priority>
        <Quantity>
          <DefaultValue>0</DefaultValue>
          <IsInt>true</IsInt>
          <BindingValue xsi:type="xsd:string">DefaultQuantity</BindingValue>
          <DynamicValue>
            <Children />
            <IsExpanded>false</IsExpanded>
            <IsSelected>false</IsSelected>
            <Name>Default order quantity</Name>
            <OffsetType>Arithmetic</OffsetType>
            <AssignedCommand>
              <Command>DefaultQuantity</Command>
              <Parameters />
            </AssignedCommand>
            <BarsAgo>0</BarsAgo>
            <CurrencyType>Currency</CurrencyType>
            <Date>2023-02-28T22:17:40.6493071</Date>
            <DayOfWeek>Sunday</DayOfWeek>
            <EndBar>0</EndBar>
            <ForceSeriesIndex>false</ForceSeriesIndex>
            <LookBackPeriod>0</LookBackPeriod>
            <MarketPosition>Long</MarketPosition>
            <Period>0</Period>
            <ReturnType>Number</ReturnType>
            <StartBar>0</StartBar>
            <State>Undefined</State>
            <Time>0001-01-01T00:00:00</Time>
          </DynamicValue>
          <IsLiteral>false</IsLiteral>
          <LiveValue xsi:type="xsd:string">DefaultQuantity</LiveValue>
        </Quantity>
        <ServiceName />
        <ScreenshotPath />
        <SoundLocation />
        <Tag>
          <SeparatorCharacter> </SeparatorCharacter>
          <Strings>
            <NinjaScriptString>
              <Index>0</Index>
              <StringValue>Set Profit target</StringValue>
            </NinjaScriptString>
          </Strings>
        </Tag>
        <TextPosition>BottomLeft</TextPosition>
        <Value>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">Runner_profit_ticks</BindingValue>
          <DynamicValue>
            <Children />
            <IsExpanded>false</IsExpanded>
            <IsSelected>true</IsSelected>
            <Name>Runner_profit_ticks</Name>
            <OffsetType>Arithmetic</OffsetType>
            <AssignedCommand>
              <Command>Runner_profit_ticks</Command>
              <Parameters />
            </AssignedCommand>
            <BarsAgo>0</BarsAgo>
            <CurrencyType>Currency</CurrencyType>
            <Date>2023-02-28T22:17:59.7850042</Date>
            <DayOfWeek>Sunday</DayOfWeek>
            <EndBar>0</EndBar>
            <ForceSeriesIndex>false</ForceSeriesIndex>
            <LookBackPeriod>0</LookBackPeriod>
            <MarketPosition>Long</MarketPosition>
            <Period>0</Period>
            <ReturnType>Number</ReturnType>
            <StartBar>0</StartBar>
            <State>Undefined</State>
            <Time>0001-01-01T00:00:00</Time>
          </DynamicValue>
          <IsLiteral>false</IsLiteral>
          <LiveValue xsi:type="xsd:string">Runner_profit_ticks</LiveValue>
        </Value>
        <VariableDateTime>2023-02-28T22:17:40.6493071</VariableDateTime>
        <VariableBool>false</VariableBool>
      </ActionProperties>
      <ActionType>Misc</ActionType>
      <Command>
        <Command>SetProfitTarget</Command>
        <Parameters>
          <string>fromEntrySignal</string>
          <string>mode</string>
          <string>value</string>
        </Parameters>
      </Command>
    </WizardAction>
    <WizardAction>
      <Children />
      <IsExpanded>false</IsExpanded>
      <IsSelected>true</IsSelected>
      <Name>Profit target</Name>
      <OffsetType>Arithmetic</OffsetType>
      <ActionProperties>
        <DashStyle>Solid</DashStyle>
        <DivideTimePrice>false</DivideTimePrice>
        <Id />
        <File />
        <FromEntrySignal>
          <SeparatorCharacter> </SeparatorCharacter>
          <Strings>
            <NinjaScriptString>
              <Index>0</Index>
              <StringValue>Scalp_entryy</StringValue>
            </NinjaScriptString>
          </Strings>
        </FromEntrySignal>
        <IsAutoScale>false</IsAutoScale>
        <IsSimulatedStop>false</IsSimulatedStop>
        <IsStop>false</IsStop>
        <LogLevel>Information</LogLevel>
        <Mode>Ticks</Mode>
        <OffsetType>Currency</OffsetType>
        <Priority>Medium</Priority>
        <Quantity>
          <DefaultValue>0</DefaultValue>
          <IsInt>true</IsInt>
          <BindingValue xsi:type="xsd:string">DefaultQuantity</BindingValue>
          <DynamicValue>
            <Children />
            <IsExpanded>false</IsExpanded>
            <IsSelected>false</IsSelected>
            <Name>Default order quantity</Name>
            <OffsetType>Arithmetic</OffsetType>
            <AssignedCommand>
              <Command>DefaultQuantity</Command>
              <Parameters />
            </AssignedCommand>
            <BarsAgo>0</BarsAgo>
            <CurrencyType>Currency</CurrencyType>
            <Date>2023-02-28T22:18:11.4171066</Date>
            <DayOfWeek>Sunday</DayOfWeek>
            <EndBar>0</EndBar>
            <ForceSeriesIndex>false</ForceSeriesIndex>
            <LookBackPeriod>0</LookBackPeriod>
            <MarketPosition>Long</MarketPosition>
            <Period>0</Period>
            <ReturnType>Number</ReturnType>
            <StartBar>0</StartBar>
            <State>Undefined</State>
            <Time>0001-01-01T00:00:00</Time>
          </DynamicValue>
          <IsLiteral>false</IsLiteral>
          <LiveValue xsi:type="xsd:string">DefaultQuantity</LiveValue>
        </Quantity>
        <ServiceName />
        <ScreenshotPath />
        <SoundLocation />
        <Tag>
          <SeparatorCharacter> </SeparatorCharacter>
          <Strings>
            <NinjaScriptString>
              <Index>0</Index>
              <StringValue>Set Profit target</StringValue>
            </NinjaScriptString>
          </Strings>
        </Tag>
        <TextPosition>BottomLeft</TextPosition>
        <Value>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">Scalp_Profit_ticks</BindingValue>
          <DynamicValue>
            <Children />
            <IsExpanded>false</IsExpanded>
            <IsSelected>true</IsSelected>
            <Name>Scalp_Profit_ticks</Name>
            <OffsetType>Arithmetic</OffsetType>
            <AssignedCommand>
              <Command>Scalp_Profit_ticks</Command>
              <Parameters />
            </AssignedCommand>
            <BarsAgo>0</BarsAgo>
            <CurrencyType>Currency</CurrencyType>
            <Date>2023-02-28T22:18:27.7564613</Date>
            <DayOfWeek>Sunday</DayOfWeek>
            <EndBar>0</EndBar>
            <ForceSeriesIndex>false</ForceSeriesIndex>
            <LookBackPeriod>0</LookBackPeriod>
            <MarketPosition>Long</MarketPosition>
            <Period>0</Period>
            <ReturnType>Number</ReturnType>
            <StartBar>0</StartBar>
            <State>Undefined</State>
            <Time>0001-01-01T00:00:00</Time>
          </DynamicValue>
          <IsLiteral>false</IsLiteral>
          <LiveValue xsi:type="xsd:string">Scalp_Profit_ticks</LiveValue>
        </Value>
        <VariableDateTime>2023-02-28T22:18:11.4171066</VariableDateTime>
        <VariableBool>false</VariableBool>
      </ActionProperties>
      <ActionType>Misc</ActionType>
      <Command>
        <Command>SetProfitTarget</Command>
        <Parameters>
          <string>fromEntrySignal</string>
          <string>mode</string>
          <string>value</string>
        </Parameters>
      </Command>
    </WizardAction>
  </StopsAndTargets>
  <StopTargetHandling>PerEntryExecution</StopTargetHandling>
  <TimeInForce>Gtc</TimeInForce>
  <TraceOrders>false</TraceOrders>
  <UseOnAddTradeEvent>false</UseOnAddTradeEvent>
  <UseOnAuthorizeAccountEvent>false</UseOnAuthorizeAccountEvent>
  <UseAccountItemUpdate>false</UseAccountItemUpdate>
  <UseOnCalculatePerformanceValuesEvent>true</UseOnCalculatePerformanceValuesEvent>
  <UseOnConnectionEvent>false</UseOnConnectionEvent>
  <UseOnDataPointEvent>true</UseOnDataPointEvent>
  <UseOnFundamentalDataEvent>false</UseOnFundamentalDataEvent>
  <UseOnExecutionEvent>false</UseOnExecutionEvent>
  <UseOnMouseDown>true</UseOnMouseDown>
  <UseOnMouseMove>true</UseOnMouseMove>
  <UseOnMouseUp>true</UseOnMouseUp>
  <UseOnMarketDataEvent>false</UseOnMarketDataEvent>
  <UseOnMarketDepthEvent>false</UseOnMarketDepthEvent>
  <UseOnMergePerformanceMetricEvent>false</UseOnMergePerformanceMetricEvent>
  <UseOnNextDataPointEvent>true</UseOnNextDataPointEvent>
  <UseOnNextInstrumentEvent>true</UseOnNextInstrumentEvent>
  <UseOnOptimizeEvent>true</UseOnOptimizeEvent>
  <UseOnOrderUpdateEvent>false</UseOnOrderUpdateEvent>
  <UseOnPositionUpdateEvent>false</UseOnPositionUpdateEvent>
  <UseOnRenderEvent>true</UseOnRenderEvent>
  <UseOnRestoreValuesEvent>false</UseOnRestoreValuesEvent>
  <UseOnShareEvent>true</UseOnShareEvent>
  <UseOnWindowCreatedEvent>false</UseOnWindowCreatedEvent>
  <UseOnWindowDestroyedEvent>false</UseOnWindowDestroyedEvent>
  <Variables>
    <InputParameter>
      <Default>8</Default>
      <Name>Scalp_Profit_ticks</Name>
      <Type>int</Type>
    </InputParameter>
    <InputParameter>
      <Default>16</Default>
      <Name>Runner_profit_ticks</Name>
      <Type>int</Type>
    </InputParameter>
    <InputParameter>
      <Default>16</Default>
      <Name>Scalp_stop_ticks</Name>
      <Type>int</Type>
    </InputParameter>
    <InputParameter>
      <Default>16</Default>
      <Name>Runner_stop_ticks</Name>
      <Type>int</Type>
    </InputParameter>
    <InputParameter>
      <Default>1</Default>
      <Name>Quantity</Name>
      <Type>int</Type>
    </InputParameter>
    <InputParameter>
      <Default>2</Default>
      <Name>BRM_bands_std</Name>
      <Type>int</Type>
    </InputParameter>
    <InputParameter>
      <Default>1</Default>
      <Name>BRM_delta</Name>
      <Type>double</Type>
    </InputParameter>
    <InputParameter>
      <Default>1</Default>
      <Name>Entry_ticks</Name>
      <Type>int</Type>
    </InputParameter>
  </Variables>
  <Name>BRMStrategyV02</Name>
</ScriptProperties>
@*/
#endregion

