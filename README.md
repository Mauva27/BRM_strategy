# BRM strategy implemented in Ninjatrader

![](https://github.com/Mauva27/BRM_strategy/blob/main/examples/chart.png)


[![version](https://img.shields.io/github/v/release/Mauva27/BRM_strategy?display_name=release)](https://github.com/Mauva27/BRM_strategy/releases)
[![commits](https://img.shields.io/github/commit-activity/m/Mauva27/BRM_strategy?color=green)]()


Algorithmic trading strategy for Ninjatrader based on the custom ```BRM``` (Basis, RSI, MACD) indicator. 

Reposiroy structure

```
BRM_strategy
│   README.md 
│
└── bin
│    └─── indicators
│	│		BRM.cs
│	│		EMAdeltas.cs
│	│		MACDBBdeltas.cs
│	│		RSIrescaled.cs
│	│		EMA.cs 		(original script from Ninjatrader)
│	│		StdDev.cs	(original script from Ninjatrader)
│	│
│	└─── strategies
│			BRMStrategyV02.cs
└── examples
		chart

```

## Use

:warning: **IMPORTANT:** The use of this or any other stragy represents a risk. Make sure to backtest the strategy using simulated accounts before implementing!

The main strategy uses the indicators from

```
EMA.cs
MACDBBdeltas.cs
RSIrescaled.cs

```

to produce the custom ```BRM``` indicator.

Once imported and compiled in ```Ninjatrader```, the indicator can be plot along with the price chart (see image above)

<span style="color:dodgerblue">Blue</span> dots below the indicator band indicate falling of the price action that could lead to a potential long position :chart_with_upwards_trend:

<span style="color:red">Red</span> dots above the indicator band indicate potential short :chart_with_downwards_trend:

 The <span style="color:yellow">Yellow</span> points indicate entry bars for longs and shorts. 

The main startegy uses such signals to enter the market.

## Trading Parameters

The entry signals can be refined by adjusting 

```
BRM_bands_std
```

where lower values trigger more entries.

The latets version allows 2 entries per direction, giving the option to have ```scalp``` and ```run``` orders. Adjust the position by

```
Profit_ticks 		: 8
Stop_ticks			: 8
Scalp_profit_ticks	: 4
Scalp_stop_ticks	: 4
```


## DISCLAIMER

The content of this strategy does not constitutes professional and/or financial advice for trading, and is not intended to be relied upon by users in making (or refraining from making) any investment decisions.

Appropriate independent advice should be obtained before making any such decision. The use of this strategy is at your sole risk and responsibility.