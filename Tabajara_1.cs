 using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PTLRuntime.NETScript;
using PTLRuntime.NETScript.Indicators;
using PTLRuntime.NETScript.Charts;

namespace Taba
{
    /// <summary>
    /// Tabajara_1
    /// 
    /// </summary>
	enum Side
    {
        Sell,
        Buy,
        Near,
        None
    }


    internal class Marker
    {
        public double High { get; private set; }
        public double Low { get; private set; }
        public double Open { get; private set; }
        public double Close { get; private set; }
        public Side side { get; internal set; }
        public DateTime TimePosition { get; private set; }
        public Marker()
        {

        }
        public Marker(DateTime timePosition, double openPrice)
            : this()
        {
            TimePosition = timePosition;
            Open = openPrice;
        }
        internal void SetParams(double high, double low, double close)
        {
            High = high;
            Low = low;
            Close = close;
        }

    }
    
    public class Taba: NETIndicator 
    {
    	// configuração das linhas de media
		[InputParameterAttribute("Linha rapida", 0, 1, 999)]
		public int LineFastPeriod = 10;

		[InputParameterAttribute("Linha média", 0, 1, 999)]
		public int LineMedPeriod = 20;

		[InputParameterAttribute("Linha lenta", 0, 1, 999)]
		public int LineLowPeriod = 200;
		
		// configuração de cores
		[InputParameterAttribute("Barra Compra", 0)]
		public Color CorBarraCompraPadrao = Color.Black;
		
		[InputParameterAttribute("Barra Venda", 0)]
		public Color CorBarraVendaPadrao = Color.Black;

		[InputParameterAttribute("Barra Compra na tendência", 0)]
		public Color CorBarraCompra = Color.Green;
		
		[InputParameterAttribute("Barra Venda na tendência", 0)]
		public Color CorBarraVenda = Color.Red;
		
		
        public Taba()
            : base()
        {
			#region Initialization
            base.Author = "Claudecir Santos";
            base.Comments = "@ClaudecirSantos";
            base.Company = "";
            base.Copyrights = "Claudecir Santos";
            base.DateOfCreation = "24.11.2017";
            base.ExpirationDate = 0;
            base.Version = "1.0";
            base.Password = "66b4a6416f59370e942d353f08a9ae36";
            base.ProjectName = "Taba";
            #endregion 
            
            base.SetIndicatorLine("Linha rápida", Color.Yellow, 1, LineStyle.SimpleChart);
            base.SetIndicatorLine("Linha média", Color.Yellow, 2, LineStyle.SimpleChart);
            base.SetIndicatorLine("Linha lenta", Color.Yellow, 3, LineStyle.SimpleChart);            
            base.SeparateWindow = false;
        }
        
        Indicator SMA10, TMA10_, SMA20, TMA20_, SMA200, TMA200_;
        
        private List<Marker> _markers;			// para o controle de cor dos candles
        BarData candle;
        
        /// <summary>
        /// This function will be called after creating
        /// </summary>
		public override void Init()
		{
			
			// Inicialização da linha rapida
			SMA10 = Indicators.iMA(CurrentData, LineFastPeriod, 0);   // 2=SMMA MAMode
			TMA10_ = Indicators.iMA((x) => { return SMA10.GetValue(0, x); }, LineFastPeriod, 0);	

			// Inicialização da linha media
			SMA20 = Indicators.iMA(CurrentData, LineMedPeriod, 0);   // 2=SMMA MAMode
			TMA20_ = Indicators.iMA((x) => { return SMA20.GetValue(0, x); }, LineMedPeriod, 0);	

			// Inicialização da linha lenta
			SMA200 = Indicators.iMA(CurrentData, LineLowPeriod, 0);   // 2=SMMA MAMode
			TMA200_ = Indicators.iMA((x) => { return SMA200.GetValue(0, x); }, LineLowPeriod, 0);	
			
			// Inicializacao dos controles das cores dos candles		    
			candle = CurrentData as BarData;
			_markers = new List<Marker>();

            var grafico = CurrentChart.GetChartControl();
            grafico.MouseMove += ProcessMouseMove;
            grafico.MouseWheel += ProcessMouseMove;	
            
						
		}        
 
        /// <summary>
        /// Entry point. This function is called when new quote comes 
        /// </summary>
        public override void OnQuote()
        {
						
        	// traça linha lenta
            SetValue(0, TMA10_.GetValue());

            _markers[0].SetParams(CurrentData.GetPrice(PriceType.High),
                                   CurrentData.GetPrice(PriceType.Low),
                                   CurrentData.GetPrice(PriceType.Close));
            
			if ((CurrentData.GetPrice(PriceType.Low) > TMA10_.GetValue()) && (CurrentData.GetPrice(PriceType.High) > TMA10_.GetValue()))
			{
				SetMarker(0, 0, Color.Green);
				_markers[0].side = Side.Buy;

		
			}
			else if ((CurrentData.GetPrice(PriceType.Low) < TMA10_.GetValue()) && (CurrentData.GetPrice(PriceType.High) < TMA10_.GetValue()))
			{
				SetMarker(0, 0, Color.Red);
				_markers[0].side = Side.Sell;				
	
			}
			else
			{
				SetMarker(0, 0, Color.Yellow);
				_markers[0].side = Side.None;
				// colorir candles

			}
		     
        	SetValue(0, TMA10_.GetValue());
        	
			// traça linha media
			if ((CurrentData.GetPrice(PriceType.Close) > TMA20_.GetValue()) && (CurrentData.GetPrice(PriceType.Open) > TMA20_.GetValue()))
				SetMarker(1, 1, Color.Green);
			else if ((CurrentData.GetPrice(PriceType.Close) < TMA20_.GetValue()) && (CurrentData.GetPrice(PriceType.Open) < TMA20_.GetValue()))
				SetMarker(1, 1, Color.Red);
			else
				SetMarker(1, 1, Color.Yellow);			
						
        	SetValue(1, TMA20_.GetValue());
        	
			// traça linha lenta
			if ((CurrentData.GetPrice(PriceType.Close) > TMA200_.GetValue()) && (CurrentData.GetPrice(PriceType.Open) > TMA200_.GetValue()))
				SetMarker(2, 2, Color.Green);
			else if ((CurrentData.GetPrice(PriceType.Close) < TMA200_.GetValue()) && (CurrentData.GetPrice(PriceType.Open) < TMA200_.GetValue()))
				SetMarker(2, 2, Color.Red);
			else
				SetMarker(2, 2, Color.Yellow);			
			
			SetValue(2, TMA200_.GetValue());
			

        }
        
		public override void NextBar()
        {
          
			// colocar as cores do grafico padrão igual ao fundo, assim o gráfico não é mostrado				
			CurrentChart.BodyBottomColor = CurrentChart.BackgroundBottomColor;
			CurrentChart.BodyUpColor = CurrentChart.BackgroundBottomColor;


      
			// mostra o novo candle com o padrão de cores tabajara						
            _markers.Insert(0, new Marker(CurrentData.Time(), CurrentData.GetPrice(PriceType.Open)));

        }
		
        public override void OnPaintChart(object sender, PaintChartEventArgs args)
        {

            args.Graphics.SetClip(CurrentChart.MainWindow.WindowRectangle);
            var barWidth = CurrentChart.GetChartPoint(CurrentData.Time(), 0).X - CurrentChart.GetChartPoint(CurrentData.Time(1), 0).X;
            var obj = new object();
            
            lock (obj)
            {
                foreach (Marker marker in _markers)
                {
                    if (CurrentChart.GetTimePrice(args.Rectangle.X, args.Rectangle.Y).Time <= marker.TimePosition && CurrentChart.GetTimePrice(args.Rectangle.X + args.Rectangle.Width, args.Rectangle.Y).Time > marker.TimePosition)
                    {
                        // determina a Cor do Candle de acordo com regra "OnQuote" e cores definidas na configuração
                        // CorBarraCompra, CorBarraVenda
                        // Logica para cor da barra
                        // se estiver dentro da tendencia, colore com as CorBarra, e estiver fora da tendencia colore com as CorBarraPadrao
                        // var pen = new Pen(new SolidBrush(marker.side == Side.Sell ? CorBarraVenda : CorBarraCompra), barWidth / 20);  linha original
                        var pen = new Pen(new SolidBrush(marker.side == Side.Sell ? CorBarraVenda : (marker.side == Side.Buy ? CorBarraCompra : CorBarraCompraPadrao)), barWidth / 20);

                        var startPos = CurrentChart.GetChartPoint(marker.TimePosition, marker.Close >= marker.Open ? marker.Open : marker.Close);
                        var finishPos = CurrentChart.GetChartPoint(marker.TimePosition, marker.Close >= marker.Open ? marker.Close : marker.Open);
                        var height = Math.Abs(startPos.Y - finishPos.Y + 1);
                        //var rect = new RectangleF(startPos.X + barWidth / 20, finishPos.Y - 1, barWidth - barWidth / 10, height);    original
                        //var rect = new RectangleF(startPos.X + barWidth / 4, finishPos.Y - 1, barWidth-4, height);
						var rect = new RectangleF(startPos.X + barWidth / 5, finishPos.Y - 1, barWidth-5, height);	
						
                        args.Graphics.FillRectangle(pen.Brush, rect);
                        var highPos = CurrentChart.GetChartPoint(marker.TimePosition, marker.High);
                        var lowPos = CurrentChart.GetChartPoint(marker.TimePosition, marker.Low);
                        args.Graphics.DrawLine(pen, new PointF(rect.X + rect.Width / 2, highPos.Y - 1), new PointF(rect.X + rect.Width / 2, lowPos.Y));
                    }
                }
                base.OnPaintChart(sender, args);
            }
        }
		

        void ProcessMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            CurrentChart.Refresh();
        }
 	
        
        /// <summary>
        /// This function will be called before removing
        /// </summary>

        public override void Complete()
        {
          
            var grafico = CurrentChart.GetChartControl();
            grafico.MouseMove -= ProcessMouseMove;
            grafico.MouseWheel -= ProcessMouseMove;
        }
        
     }
    
}
