//+----------------------------------------------------------------------------------------------+
//| Copyright © <2017>  <LizardIndicators.com - powered by AlderLab UG>
//
//| This program is free software: you can redistribute it and/or modify
//| it under the terms of the GNU General Public License as published by
//| the Free Software Foundation, either version 3 of the License, or
//| any later version.
//|
//| This program is distributed in the hope that it will be useful,
//| but WITHOUT ANY WARRANTY; without even the implied warranty of
//| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//| GNU General Public License for more details.
//|
//| By installing this software you confirm acceptance of the GNU
//| General Public License terms. You may find a copy of the license
//| here; http://www.gnu.org/licenses/
//+----------------------------------------------------------------------------------------------+

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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The ADXVMA is a volatility based moving average with the volatility being determined by the value of the ADX. 
	/// The ADXVMA provides levels of support during uptrends and resistance during downtrends.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 0)]
	[Gui.CategoryOrder("Display Options", 10)]
	[Gui.CategoryOrder("Data Series", 20)]
	[Gui.CategoryOrder("Set up", 30)]
	[Gui.CategoryOrder("Visual", 40)]
	[Gui.CategoryOrder("Plots", 50)]
	[Gui.CategoryOrder("Paint Bars", 60)]
	[Gui.CategoryOrder("Sound Alerts", 70)]
	[Gui.CategoryOrder("Version", 80)]
	public class amaADXVMA : Indicator
	{
		private int 					period 						= 8;
		private int						displacement				= 0;
 		private int						totalBarsRequiredToPlot		= 0;
		private double					multiplier					= 0.5;
		private double					k							= 0.0;
		private double					delta						= 0.0;
		private double					deltaFactor					= 0.0;
		private double					refValue					= 0.0;
		private double					currentUp					= 0.0;
		private double					currentDown					= 0.0;
		private double					sum							= 0.0;
		private double					fractionUp					= 0.0;
		private double					fractionDown				= 0.0;
		private double 					epsilon						= 0.0;
		private double					normDiff					= 0.0;
		private double					normSum						= 0.0;
		private double					normFraction				= 0.0;
		private double					hhp							= 0.0;
		private double					llp							= 0.0;
		private double					hhv							= 0.0;
		private double					llv							= 0.0;
		private double					vDiff						= 0.0;
		private double					vIndex						= 0.0;
		private bool					showPlot					= true;
		private bool					showTrendColors				= true;
		private bool					showPaintBars				= true;
		private bool					soundAlerts					= false;
		private bool					indicatorIsOnPricePanel		= true;
		private SessionIterator			sessionIterator				= null;
		private int 					plot0Width 					= 2;
		private PlotStyle 				plot0Style					= PlotStyle.Line;
		private DashStyleHelper			dash0Style					= DashStyleHelper.Solid;
		private Brush					defaultBrush				= Brushes.Navy;
		private Brush					upBrush						= Brushes.Blue;
		private Brush					downBrush					= Brushes.Red;
		private Brush					neutralBrush				= Brushes.Goldenrod;
		private Brush					upBrushUp					= Brushes.Blue;
		private Brush					upBrushDown					= Brushes.LightSkyBlue;
		private Brush					downBrushUp					= Brushes.LightCoral;
		private Brush					downBrushDown				= Brushes.Red;
		private Brush					neutralBrushUp				= Brushes.Goldenrod;
		private Brush					neutralBrushDown			= Brushes.Gold;
		private Brush					upBrushOutline				= Brushes.Black;
		private Brush					downBrushOutline			= Brushes.Black;
		private Brush					neutralBrushOutline			= Brushes.Black;
		private Brush					trendBrush					= null;
		private Brush					priorTrendBrush				= null;
		private Brush					transparentBrush			= Brushes.Transparent;
		private Brush					alertBackBrush				= Brushes.Black;
		private int						rearmTime					= 30;
		private string 					newUptrend					= "newuptrend.wav";
		private string 					newDowntrend				= "newdowntrend.wav";
		private string					newNeutraltrend				= "newneutraltrend.wav";
		private string					pathNewUptrend				= "";
		private string					pathNewDowntrend			= "";
		private string					pathNewNeutraltrend			= "";
		private string					versionString				= "v 1.2  -  November 14, 2017";
		private Series<double> 			up;
		private Series<double> 			down;
		private Series<double> 			ups;
		private Series<double> 			downs;
		private Series<double> 			index;
		private Series<double>			trend;
		private amaMAX					maxIndex;
		private amaMIN					minIndex;
		private amaATR 					volatility; 
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe ADXVMA is a volatility based moving average with the volatility being determined by the value of the ADX. "
												+ "The ADXVMA provides levels of support during uptrends and resistance during downtrends.";
				Name						= "amaADXVMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				ArePlotsConfigurable		= false;
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "ADXVMA");	
			}
			else if (State == State.Configure)
			{
				displacement = Displacement;
				BarsRequiredToPlot = Math.Max(40 * Convert.ToInt32(Math.Sqrt(period)), -displacement);
				totalBarsRequiredToPlot = Math.Max(BarsRequiredToPlot, BarsRequiredToPlot + displacement);
				Plots[0].Brush = defaultBrush;
				Plots[0].PlotStyle = plot0Style;
				Plots[0].DashStyleHelper = dash0Style;			
				Plots[0].Width = plot0Width;
			}
			else if (State == State.DataLoaded)
			{
				up = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				down = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				ups = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				downs = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				index = new Series<double>(this, period < 250 ? MaximumBarsLookBack.TwoHundredFiftySix : MaximumBarsLookBack.Infinite);
				trend = new Series<double>(this, MaximumBarsLookBack.Infinite);
				maxIndex = amaMAX(index, period);
				minIndex = amaMIN(index, period);
				volatility = amaATR(Inputs[0], amaATRCalcMode.Wilder, 10*period);
		    	sessionIterator = new SessionIterator(Bars);
			}
			else if (State == State.Historical)
			{
				k = 1.0/(double)period;
				deltaFactor = multiplier / (10.0*Math.Sqrt(period));
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
				pathNewUptrend = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, newUptrend);
				pathNewDowntrend = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, newDowntrend);
				pathNewNeutraltrend = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, newNeutraltrend);
			}
		}

		protected override void OnBarUpdate()
		{
			if( CurrentBar < 2 )
			{
				up[0] = 0.0;
				down[0] = 0.0;
				ups[0] = 0.0;
				downs[0] = 0.0;
				index[0] = 0.0;
				trend[0] = 0.0;
				ADXVMA[0] = Input[0];
				return;
			}
			currentUp = Math.Max(Input[0] - Input[1], 0);
         	currentDown = Math.Max(Input[1] - Input[0], 0);
		    up[0] = (1-k)*up[1] + k*currentUp;
			down[0] = (1-k)*down[1] + k*currentDown;
			
			sum = up[0] + down[0];
			fractionUp = 0.0;
			fractionDown = 0.0;
			if(sum > double.Epsilon)
			{
				fractionUp = up[0]/sum;
				fractionDown = down[0]/sum;
			}
			ups[0] = (1-k)*ups[1] + k*fractionUp;
			downs[0] = (1-k)*downs[1] + k*fractionDown;
			
			normDiff = Math.Abs(ups[0] - downs[0]);
			normSum = ups[0] + downs[0];
			normFraction = 0.0;
			if(normSum > double.Epsilon)
				normFraction = normDiff/normSum;
			index[0] = (1-k)*index[1] + k*normFraction;
			
        	if(IsFirstTickOfBar)
			{
				refValue = ADXVMA[1] + ADXVMA[2];
				delta = deltaFactor * volatility[1];
				hhp = maxIndex[1];
				llp = minIndex[1];
			}	
			hhv = Math.Max(index[0], hhp);
			llv = Math.Min(index[0], llp);
			
			vDiff = hhv-llv;
			vIndex = 0;
			if(vDiff > double.Epsilon)
				vIndex = (index[0] - llv)/vDiff;
			
			ADXVMA[0] = (1 - k*vIndex)*ADXVMA[1] + k*vIndex*Input[0];
			
			if (CurrentBar < BarsRequiredToPlot)
			{	
				trend[0] = 0.0;
				return;
			}
			
			if(trend[1] > -0.5 && 2*ADXVMA[0] > refValue + 3*delta)
			{
				trend[0] = 1.0;
					if(showPlot && showTrendColors)
						PlotBrushes[0][0] = upBrush; 
					else if(!showPlot)
						PlotBrushes[0][0] = Brushes.Transparent;
			}
			else if(trend[1] < 0.5 && 2*ADXVMA[0] < refValue - 3*delta)
			{
				trend[0] = -1.0;
					if(showPlot && showTrendColors)
						PlotBrushes[0][0] = downBrush; 
					else if(!showPlot)
						PlotBrushes[0][0] = Brushes.Transparent;
			}
			else
			{
				trend[0] = 0.0;
					if(showPlot && showTrendColors)
						PlotBrushes[0][0] = neutralBrush; 
					else if(!showPlot)
						PlotBrushes[0][0] = Brushes.Transparent;
			}
			
			if(showPaintBars && CurrentBar >= totalBarsRequiredToPlot)
			{
				if(displacement >= 0)
				{
					if(trend[displacement] > 0.5)
					{ 
						if(Open[0] < Close[0])
							BarBrushes[0] = upBrushUp;
						else
							BarBrushes[0] = upBrushDown;
						CandleOutlineBrushes[0] = upBrushOutline;
					}	
					else if(trend[displacement] < -0.5)
					{
						if(Open[0] < Close[0])
							BarBrushes[0] = downBrushUp;
						else
							BarBrushes[0] = downBrushDown;
						CandleOutlineBrushes[0] = downBrushOutline;
					}	
					else
					{
						if(Open[0] < Close[0])
							BarBrushes[0] = neutralBrushUp;
						else
							BarBrushes[0] = neutralBrushDown;
						CandleOutlineBrushes[0] = neutralBrushOutline;
					}	
				}
				else 
				{
					if(trend[0] > 0.5)
					{
						if(Open[-displacement] < Close[-displacement])
							BarBrushes[-displacement] = upBrushUp;
						else
							BarBrushes[-displacement] = upBrushDown;
						CandleOutlineBrushes[-displacement] = upBrushOutline;
					}	
					else if(trend[0] < -0.5)
					{
						if(Open[-displacement] < Close[-displacement])
							BarBrushes[-displacement] = downBrushUp;
						else
							BarBrushes[-displacement] = downBrushDown;
						CandleOutlineBrushes[-displacement] = downBrushOutline;
					}	
					else
					{
						if(Open[-displacement] < Close[-displacement])
							BarBrushes[-displacement] = neutralBrushUp;
						else
							BarBrushes[-displacement] = neutralBrushDown;
						CandleOutlineBrushes[-displacement] = neutralBrushOutline;
					}
				}
			}	
			
			if (soundAlerts && State == State.Realtime && IsConnected())
			{
				if(trend[0] > 0.5 && trend[1] < 0.5)		
				{
					try
					{
							Alert("New_Uptrend", Priority.Medium,"New Uptrend", pathNewUptrend, rearmTime, alertBackBrush, upBrush);
					}
					catch{}
				}
				else if(trend[0] < -0.5 && trend[1] > -0.5)		
				{
					try
					{
							Alert("New_Downtrend", Priority.Medium,"New Downtrend", pathNewDowntrend, rearmTime, alertBackBrush, downBrush);
					}
					catch{}
				}
				else if(trend[0] > -0.5 && trend[0] < 0.5 && (trend[1] > 0.5 || trend[1] < -0.5))		
				{
					try
					{
							Alert("New_Neutraltrend", Priority.Medium,"New neutral trend", pathNewNeutraltrend, rearmTime, alertBackBrush, neutralBrush);
					}
					catch{}
				}
			}
		}
		
		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ADXVMA
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Trend
		{
			get { return trend; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "Input Parameters", Order = 0)]
		public int Period
		{	
            get { return period; }
            set { period = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show plot", GroupName = "Display Options", Order = 0)]
        public bool ShowPlot
        {
            get { return showPlot; }
            set { showPlot = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show trend colors", GroupName = "Display Options", Order = 1)]
        public bool ShowTrendColors
        {
            get { return showTrendColors; }
            set { showTrendColors = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show paint bars", GroupName = "Display Options", Order = 2)]
        public bool ShowPaintBars
        {
            get { return showPaintBars; }
            set { showPaintBars = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Default color", Description = "Sets the default color for the ADXVMA plot", GroupName = "Plots", Order = 0)]
		public Brush DefaultBrush
		{ 
			get {return defaultBrush;}
			set {defaultBrush = value;}
		}

		[Browsable(false)]
		public string DefaultBrushSerializable
		{
			get { return Serialize.BrushToString(defaultBrush); }
			set { defaultBrush = Serialize.StringToBrush(value); }
		}					
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish color", Description = "Sets the bullish color for the ADXVMA plot", GroupName = "Plots", Order = 1)]
		public Brush UpBrush
		{ 
			get {return upBrush;}
			set {upBrush = value;}
		}

		[Browsable(false)]
		public string UpBrushSerializable
		{
			get { return Serialize.BrushToString(upBrush); }
			set { upBrush = Serialize.StringToBrush(value); }
		}					
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish color", Description = "Sets the bearish color for the ADXVMA plot", GroupName = "Plots", Order = 2)]
		public Brush DownBrush
		{ 
			get {return downBrush;}
			set {downBrush = value;}
		}

		[Browsable(false)]
		public string DownBrushSerializable
		{
			get { return Serialize.BrushToString(downBrush); }
			set { downBrush = Serialize.StringToBrush(value); }
		}					
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral color", Description = "Sets the neutral color for the ADXVMA plot", GroupName = "Plots", Order = 3)]
		public Brush NeutralBrush
		{ 
			get {return neutralBrush;}
			set {neutralBrush = value;}
		}

		[Browsable(false)]
		public string NeutralBrushSerializable
		{
			get { return Serialize.BrushToString(neutralBrush); }
			set { neutralBrush = Serialize.StringToBrush(value); }
		}					
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plotstyle", Description = "Sets the plot style for the indicator plot", GroupName = "Plots", Order = 4)]
		public PlotStyle Plot0Style
		{	
            get { return plot0Style; }
            set { plot0Style = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dashstyle", Description = "Sets the dash style for the indicator plot", GroupName = "Plots", Order = 5)]
		public DashStyleHelper Dash0Style
		{
			get { return dash0Style; }
			set { dash0Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width", Description = "Sets the plot width for the indicator plot", GroupName = "Plots", Order = 6)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish Upclose", Description = "Sets the color for a bullish trend", GroupName = "Paint Bars", Order = 0)]
		public Brush UpBrushUp
		{ 
			get {return upBrushUp;}
			set {upBrushUp = value;}
		}

		[Browsable(false)]
		public string UpBrushUpSerializable
		{
			get { return Serialize.BrushToString(upBrushUp); }
			set { upBrushUp = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish Downclose", Description = "Sets the color for a bullish trend", GroupName = "Paint Bars", Order = 1)]
		public Brush UpBrushDown
		{ 
			get {return upBrushDown;}
			set {upBrushDown = value;}
		}

		[Browsable(false)]
		public string UpBrushDownSerializable
		{
			get { return Serialize.BrushToString(upBrushDown); }
			set { upBrushDown = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish candle outline", Description = "Sets the color for candle outlines", GroupName = "Paint Bars", Order = 2)]
		public Brush UpBrushOutline
		{ 
			get {return upBrushOutline;}
			set {upBrushOutline = value;}
		}

		[Browsable(false)]
		public string UpBrushOutlineSerializable
		{
			get { return Serialize.BrushToString(upBrushOutline); }
			set { upBrushOutline = Serialize.StringToBrush(value); }
		}					
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish Upclose", Description = "Sets the color for a bearish trend", GroupName = "Paint Bars", Order = 3)]
		public Brush DownBrushUp
		{ 
			get {return downBrushUp;}
			set {downBrushUp = value;}
		}

		[Browsable(false)]
		public string DownBrushUpSerializable
		{
			get { return Serialize.BrushToString(downBrushUp); }
			set { downBrushUp = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish Downclose", Description = "Sets the color for a bearish trend", GroupName = "Paint Bars", Order = 4)]
		public Brush DownBrushDown
		{ 
			get {return downBrushDown;}
			set {downBrushDown = value;}
		}

		[Browsable(false)]
		public string DownBrushDownSerializable
		{
			get { return Serialize.BrushToString(downBrushDown); }
			set { downBrushDown = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish candle outline", Description = "Sets the color for candle outlines", GroupName = "Paint Bars", Order = 5)]
		public Brush DownBrushOutline
		{ 
			get {return downBrushOutline;}
			set {downBrushOutline = value;}
		}

		[Browsable(false)]
		public string DownBrushOutlineSerializable
		{
			get { return Serialize.BrushToString(downBrushOutline); }
			set { downBrushOutline = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral Upclose", Description = "Sets the color for a neutral trend", GroupName = "Paint Bars", Order = 6)]
		public Brush NeutralBrushUp
		{ 
			get {return neutralBrushUp;}
			set {neutralBrushUp = value;}
		}

		[Browsable(false)]
		public string NeutralBrushUpSerializable
		{
			get { return Serialize.BrushToString(neutralBrushUp); }
			set { neutralBrushUp = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral downclose", Description = "Sets the color for a neutral trend", GroupName = "Paint Bars", Order = 7)]
		public Brush NeutralBrushDown
		{ 
			get {return neutralBrushDown;}
			set {neutralBrushDown = value;}
		}

		[Browsable(false)]
		public string NeutralBrushDownSerializable
		{
			get { return Serialize.BrushToString(neutralBrushDown); }
			set { neutralBrushDown = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral candle outline", Description = "Sets the color for candle outlines", GroupName = "Paint Bars", Order = 8)]
		public Brush NeutralBrushOutline
		{ 
			get {return neutralBrushOutline;}
			set {neutralBrushOutline = value;}
		}

		[Browsable(false)]
		public string NeutralBrushOutlineSerializable
		{
			get { return Serialize.BrushToString(neutralBrushOutline); }
			set { neutralBrushOutline = Serialize.StringToBrush(value); }
		}

		[Display(ResourceType = typeof(Custom.Resource), Name = "Sound alerts", GroupName = "Sound Alerts", Order = 0)]
        public bool SoundAlerts
        {
            get { return soundAlerts; }
            set { soundAlerts = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "New uptrend", Description = "Sound file for confirmed new uptrend", GroupName = "Sound Alerts", Order = 1)]
		public string NewUptrend
		{	
            get { return newUptrend; }
            set { newUptrend = value; }
		}		
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "New downtrend", Description = "Sound file for confirmed new downtrend", GroupName = "Sound Alerts", Order = 2)]
		public string NewDowntrend
		{	
            get { return newDowntrend; }
            set { newDowntrend = value; }
		}		
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "New neutral trend", Description = "Sound file for potential new uptrend", GroupName = "Sound Alerts", Order = 3)]
		public string NewNeutraltrend
		{	
            get { return newNeutraltrend; }
            set { newNeutraltrend = value; }
		}		
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Rearm time", Description = "Rearm time for alerts in seconds", GroupName = "Sound Alerts", Order = 4)]
		public int RearmTime
		{	
            get { return rearmTime; }
            set { rearmTime = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Release and date", Description = "Release and date", GroupName = "Version", Order = 0)]
		public string VersionString
		{	
            get { return versionString; }
            set { ; }
		}
		#endregion
		
		#region Miscellaneous

		public override string FormatPriceMarker(double price)
		{
			if(indicatorIsOnPricePanel)
				return Instrument.MasterInstrument.FormatPrice(Instrument.MasterInstrument.RoundToTickSize(price));
			else
				return base.FormatPriceMarker(price);
		}
		
		private bool IsConnected()
        {
			if ( Bars != null && Bars.Instrument.GetMarketDataConnection().PriceStatus == NinjaTrader.Cbi.ConnectionStatus.Connected
					&& sessionIterator.IsInSession(Now, true, true))
				return true;
			else
            	return false;
        }
		
		private DateTime Now
		{
          get 
			{ 
				DateTime now = (Bars.Instrument.GetMarketDataConnection().Options.Provider == NinjaTrader.Cbi.Provider.Playback ? Bars.Instrument.GetMarketDataConnection().Now : DateTime.Now); 

				if (now.Millisecond > 0)
					now = NinjaTrader.Core.Globals.MinDate.AddSeconds((long) System.Math.Floor(now.Subtract(NinjaTrader.Core.Globals.MinDate).TotalSeconds));

				return now;
			}
		}
		#endregion	
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaADXVMA[] cacheamaADXVMA;
		public LizardIndicators.amaADXVMA amaADXVMA(int period)
		{
			return amaADXVMA(Input, period);
		}

		public LizardIndicators.amaADXVMA amaADXVMA(ISeries<double> input, int period)
		{
			if (cacheamaADXVMA != null)
				for (int idx = 0; idx < cacheamaADXVMA.Length; idx++)
					if (cacheamaADXVMA[idx] != null && cacheamaADXVMA[idx].Period == period && cacheamaADXVMA[idx].EqualsInput(input))
						return cacheamaADXVMA[idx];
			return CacheIndicator<LizardIndicators.amaADXVMA>(new LizardIndicators.amaADXVMA(){ Period = period }, input, ref cacheamaADXVMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaADXVMA amaADXVMA(int period)
		{
			return indicator.amaADXVMA(Input, period);
		}

		public Indicators.LizardIndicators.amaADXVMA amaADXVMA(ISeries<double> input , int period)
		{
			return indicator.amaADXVMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaADXVMA amaADXVMA(int period)
		{
			return indicator.amaADXVMA(Input, period);
		}

		public Indicators.LizardIndicators.amaADXVMA amaADXVMA(ISeries<double> input , int period)
		{
			return indicator.amaADXVMA(input, period);
		}
	}
}

#endregion
