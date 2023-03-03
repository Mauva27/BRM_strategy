# BRM strategy implemented in Ninjatrader

[![version](https://img.shields.io/badge/releases-v0.2.1-blue)(...)]

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