using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MagazynLaptopowWPF.Controls
{
    public partial class BarcodeControl : UserControl
    {
        public static readonly DependencyProperty BarcodeValueProperty =
            DependencyProperty.Register(
                "BarcodeValue",
                typeof(string),
                typeof(BarcodeControl),
                new PropertyMetadata(string.Empty, OnBarcodeValueChanged));

        public string BarcodeValue
        {
            get => (string)GetValue(BarcodeValueProperty);
            set => SetValue(BarcodeValueProperty, value);
        }

        private static void OnBarcodeValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BarcodeControl control)
            {
                control.GenerateBarcode();
            }
        }

        public BarcodeControl()
        {
            InitializeComponent();
        }

        public void GenerateBarcode()
        {
            BarcodeCanvas.Children.Clear();

            string barcodeValue = BarcodeValue?.Trim() ?? string.Empty;

            // Sprawdź, czy kod ma poprawny format dla EAN-13
            if (string.IsNullOrEmpty(barcodeValue) || barcodeValue.Length != 13 || !long.TryParse(barcodeValue, out _))
            {
                BarcodeText.Text = "Nieprawidłowy kod EAN-13";
                return;
            }

            // Ustaw kod pod obrazkiem
            BarcodeText.Text = barcodeValue;

            // Rysuj kod EAN-13
            DrawEan13(barcodeValue);
        }

        private void DrawEan13(string code)
        {
            // Stałe encodingu EAN-13
            string[] eanEncoding = {
                // L-encoding dla cyfr 0-9
                "0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011",
                // G-encoding dla cyfr 0-9
                "0100111", "0110011", "0011011", "0100001", "0011101", "0111001", "0000101", "0010001", "0001001", "0010111",
                // R-encoding dla cyfr 0-9
                "1110010", "1100110", "1101100", "1000010", "1011100", "1001110", "1010000", "1000100", "1001000", "1110100"
            };

            // Wzorce pierwszych 6 cyfr (dla pierwszej cyfry od 0 do 9)
            string[] firstGroupPattern = {
                "LLLLLL", "LLGLGG", "LLGGLG", "LLGGGL", "LGLLGG", "LGGLLG", "LGGGLL", "LGLGLG", "LGLGGL", "LGGLGL"
            };

            double barWidth = 2; // Szerokość pojedynczego paska
            double height = BarcodeCanvas.Height;
            double xPos = 10; // Początkowa pozycja x

            // Rysuj kod kreskowy

            // Lewy znacznik (start)
            DrawBar(xPos, 0, barWidth, height, Brushes.Black); // 1
            xPos += barWidth;
            DrawBar(xPos, 0, barWidth, height, Brushes.White); // 0
            xPos += barWidth;
            DrawBar(xPos, 0, barWidth, height, Brushes.Black); // 1
            xPos += barWidth;

            // Pierwsza cyfra (tylko jako indeks do wzoru pierwszej grupy)
            int firstDigit = int.Parse(code[0].ToString());
            string pattern = firstGroupPattern[firstDigit];

            // Grupa lewa (6 cyfr)
            for (int i = 1; i <= 6; i++)
            {
                int digit = int.Parse(code[i].ToString());
                char encoding = pattern[i - 1];

                // Wybierz właściwe kodowanie (L lub G)
                string bars = encoding == 'L' ? eanEncoding[digit] : eanEncoding[digit + 10];

                // Rysuj zakodowaną cyfrę
                foreach (char bar in bars)
                {
                    Brush brush = bar == '1' ? Brushes.Black : Brushes.White;
                    DrawBar(xPos, 0, barWidth, height, brush);
                    xPos += barWidth;
                }
            }

            // Środkowy znacznik
            DrawBar(xPos, 0, barWidth, height, Brushes.White); // 0
            xPos += barWidth;
            DrawBar(xPos, 0, barWidth, height, Brushes.Black); // 1
            xPos += barWidth;
            DrawBar(xPos, 0, barWidth, height, Brushes.White); // 0
            xPos += barWidth;
            DrawBar(xPos, 0, barWidth, height, Brushes.Black); // 1
            xPos += barWidth;
            DrawBar(xPos, 0, barWidth, height, Brushes.White); // 0
            xPos += barWidth;

            // Grupa prawa (6 cyfr)
            for (int i = 7; i <= 12; i++)
            {
                int digit = int.Parse(code[i].ToString());
                string bars = eanEncoding[digit + 20]; // Zawsze używaj R-kodowania dla prawej grupy

                // Rysuj zakodowaną cyfrę
                foreach (char bar in bars)
                {
                    Brush brush = bar == '1' ? Brushes.Black : Brushes.White;
                    DrawBar(xPos, 0, barWidth, height, brush);
                    xPos += barWidth;
                }
            }

            // Prawy znacznik (koniec)
            DrawBar(xPos, 0, barWidth, height, Brushes.Black); // 1
            xPos += barWidth;
            DrawBar(xPos, 0, barWidth, height, Brushes.White); // 0
            xPos += barWidth;
            DrawBar(xPos, 0, barWidth, height, Brushes.Black); // 1
            xPos += barWidth;
        }

        private void DrawBar(double x, double y, double width, double height, Brush brush)
        {
            Rectangle rect = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = brush
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);

            BarcodeCanvas.Children.Add(rect);
        }
    }
}