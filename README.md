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

The main strategy uses the indicators from

```
EMA.cs
MACDBBdeltas.cs
RSIrescaled.cs

```

to produce the custom ```BRM``` indicator.

Once imported and compiled in ```Ninjatrader```, the indicator can be plot along with the price chart (see image above)

<span style="color:dodgerblue">Blue</span> dots below the indicator band indicate falling of the price action that could lead to a potential long position. <span style="color:ref">Red</span> dots above the indicator band indicate potential short. The <span style="color:yellow">Yellow</span> points indicate entry bars for longs and shorts. 
