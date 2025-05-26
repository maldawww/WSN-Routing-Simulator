using SensorNetworkSimulator.Models;
using SensorNetworkSimulator.Simulation;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SensorNetworkSimulator.IO;
using Microsoft.Win32;




namespace SensorNetworkSimulator
{

    /// <summary>
    /// Główne okno aplikacji WPF symulującej działanie sieci sensorowej.
    /// Obsługuje GUI, inicjalizację sieci, przebieg symulacji, wykresy oraz zapis wyników.
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Zarządza całą logiką symulacji (czujniki, POI, routing, energia).
        /// </summary>
        /// 
        private SensorManager manager = new();
        /// <summary>
        /// Szerokość obszaru symulacji.
        /// </summary>
        /// 
        private const double FieldWidth = 600;

        /// <summary>
        /// Wysokosc obszaru symulacji.
        /// </summary>
        private const double FieldHeight = 500;
        private int failureCount = 0;
        private int lostPacketCount = 0;
        private int totalSteps = 0;

        /// <summary>
        /// Inicjalizuje komponenty GUI.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Obsługuje przycisk „Generuj” — tworzy sensory, POI, centralę, ustawia zasięgi.
        /// </summary>
        private void Generate_Click(object sender, RoutedEventArgs e)
        {

            totalSteps = 0;
            TotalStepCounterText.Text = "Tura: 0";


            if (!int.TryParse(SensorCountBox.Text, out int sensorCount)) return;
            if (!int.TryParse(POICountBox.Text, out int poiCount)) return;
            if (!double.TryParse(RangeBox.Text, out double range)) return;

            manager.GenerateSensors(sensorCount, FieldWidth, FieldHeight, range);

            manager.PacketLossChancePercent = PacketLossSlider.Value;
            manager.FailureChancePercent = FailureChanceSlider.Value;

            manager.GeneratePOIs(poiCount, FieldWidth, FieldHeight);
            manager.SetCentralNode(FieldWidth / 2, FieldHeight / 2, range);
            manager.DetectNeighbors();
            manager.DetectPOIs();

            manager.UpdateEnergy();
            DrawNetwork();
        }

        /// <summary>
        /// Rysuje całą sieć na canvasie: sensory, POI, centralę, zasięgi, ścieżki.
        /// Wyświetla symbole awarii i zgubionych pakietów.
        /// </summary>
        private async void DrawNetwork()
        {

            failureCount = 0;
            lostPacketCount = 0;

            SimulationCanvas.Children.Clear();

            // Central Node
            Ellipse central = new()
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Blue
            };
            Canvas.SetLeft(central, manager.SinkNode.X - 5);
            Canvas.SetTop(central, manager.SinkNode.Y - 5);
            SimulationCanvas.Children.Add(central);

            // POIs (kwadraty)
            foreach (var poi in manager.POIs)
            {
                Rectangle rect = new()
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.Red
                };
                Canvas.SetLeft(rect, poi.X - 4);
                Canvas.SetTop(rect, poi.Y - 4);
                SimulationCanvas.Children.Add(rect);
            }



            // Sensors
            foreach (var sensor in manager.Sensors.ToList())
            {

                if (!sensor.IsAlive)
                    failureCount++;

                if (sensor.LostPacketLastStep)
                    lostPacketCount++;


                // zasięg
                Ellipse rangeCircle = new()
                {
                    Width = sensor.Range * 2,
                    Height = sensor.Range * 2,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1,
                    Opacity = 0.2
                };
                Canvas.SetLeft(rangeCircle, sensor.X - sensor.Range);
                Canvas.SetTop(rangeCircle, sensor.Y - sensor.Range);
                SimulationCanvas.Children.Add(rangeCircle);
                // Tekst z poziomem baterii
                var batteryText = new TextBlock
                {
                    Text = $"{sensor.BatteryLevel:F1}", // np. 97.8
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                };

                // Pozycja tekstu nad sensorem
                Canvas.SetLeft(batteryText, sensor.X - 10);
                Canvas.SetTop(batteryText, sensor.Y - 20);

                SimulationCanvas.Children.Add(batteryText);


                // czujnik
                Brush fillBrush;

                if (!sensor.IsAlive && sensor.DiedByFailure)
                    fillBrush = Brushes.OrangeRed; // 💥 losowa awaria
                else if (!sensor.IsAlive)
                    fillBrush = Brushes.DarkGray; // 🔋 normalna śmierć (bateria = 0)
                else if (sensor.Status == SensorStatus.Active)
                    fillBrush = Brushes.Green;
                else if (sensor.Status == SensorStatus.Sleeping)
                    fillBrush = Brushes.Black;
                else
                    fillBrush = Brushes.Gray;

                if (!sensor.IsAlive && sensor.DiedByFailure)
                {
                    // ❌ Awaria
                    var failureLabel = new TextBlock
                    {
                        Text = "X",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.DarkRed
                    };
                    Canvas.SetLeft(failureLabel, sensor.X + 8); 
                    Canvas.SetTop(failureLabel, sensor.Y - 5);
                    SimulationCanvas.Children.Add(failureLabel);
                }
                else if (sensor.LostPacketLastStep)
                {
                    // ❗ Zgubiony pakiet
                    var lostLabel = new TextBlock
                    {
                        Text = "!",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Orange
                    };
                    Canvas.SetLeft(lostLabel, sensor.X + 8);    
                    Canvas.SetTop(lostLabel, sensor.Y - 5);     
                    SimulationCanvas.Children.Add(lostLabel);
                }


                var sensorEllipse = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = fillBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.5,
                    IsHitTestVisible = true
                };


                // ToolTip jako obiekt
                var tooltip = new ToolTip
                {
                    Content = $"ID: {sensor.Id}\nStatus: {sensor.Status}\nBateria: {sensor.BatteryLevel:F2}\nSąsiedzi: {sensor.Neighbors.Count}"
                };

                ToolTipService.SetToolTip(sensorEllipse, tooltip);

                // ustawienie na canvas
                Canvas.SetLeft(sensorEllipse, sensor.X - 5);
                Canvas.SetTop(sensorEllipse, sensor.Y - 5);
                SimulationCanvas.Children.Add(sensorEllipse);  
            }

            // Komunikacja – próba przesłania danych z sensora do centrali

            foreach (var sensor in manager.Sensors.ToList())
            {
                if (sensor.HasPOIInRange && sensor.IsAlive)
                {
                    if (sensor.LostPacketLastStep)
                    {
                        Console.WriteLine($"Sensor {sensor.Id}: Pakiet zgubiony – nie rysuję linii");
                        continue;
                    }


                    if (manager.TransmitToSink(sensor, out var path))
                    {
                        Console.WriteLine($"Sensor {sensor.Id} przesłał dane do centrali. Ścieżka: {string.Join(" → ", path)}");

                        // Rysowanie ścieżki jako niebieskie linie
                        var drawnEdges = new HashSet<string>();

                        for(int i = 0; i < path.Count - 1; i++)
{
                            int fromId = path[i];
                            int toId = path[i + 1];

                            BaseNode from = fromId == -1 ? manager.SinkNode : manager.Sensors.Find(s => s.Id == fromId);
                            BaseNode to = toId == -1 ? manager.SinkNode : manager.Sensors.Find(s => s.Id == toId);

                            // Tworzymy unikalny klucz dla krawędzi (np. "3-5" lub "5-3")
                            string edgeKey = $"{Math.Min(fromId, toId)}-{Math.Max(fromId, toId)}";

                            // Rysuj linię tylko jeśli jeszcze nie była rysowana
                            if (!drawnEdges.Contains(edgeKey))
                            {
                                drawnEdges.Add(edgeKey);

                                var line = new System.Windows.Shapes.Line
                                {
                                    Stroke = Brushes.Blue,
                                    StrokeThickness = 2,
                                    X1 = from.X,
                                    Y1 = from.Y,
                                    X2 = to.X,
                                    Y2 = to.Y,
                                    StrokeDashArray = new DoubleCollection { 2, 2 },
                                    Opacity = 0.8
                                };

                                SimulationCanvas.Children.Add(line);
                                await Task.Delay(150);
                            }
                        }


                    }
                    else
                    {
                        Console.WriteLine($"Sensor {sensor.Id} nie może przesłać danych do centrali.");
                    }
                }
            }
            FailuresCountText.Text = manager.TotalFailures.ToString();
            PacketLossCountText.Text = manager.TotalPacketLosses.ToString();


        }
        /// <summary>
        /// Obsługuje przycisk „Następny krok” — wykonuje jeden krok symulacji.
        /// </summary>
        private void NextStep_Click(object sender, RoutedEventArgs e)
        {



            foreach (var sensor in manager.Sensors)
            {
                sensor.LostPacketLastStep = false;
                sensor.AlreadyTransmittedThisStep = false;

            }


            // Aktualizuj status sensorów (czy nadal widzą POI)
            manager.DetectPOIs();
            foreach (var s in manager.Sensors){
                s.LostPacketLastStep = false;
                s.AlreadyTransmittedThisStep = false;
            }

            // Komunikacja (dla aktywnych)
            foreach (var sensor in manager.Sensors)
            {
                if (sensor.HasPOIInRange && sensor.IsAlive)
                {
                    manager.TransmitToSink(sensor, out _);
                }
            }

            // Zużycie energii
            manager.UpdateEnergy();
            manager.DetectNeighbors();

            // Odśwież wizualizację
            DrawNetwork();
            totalSteps++;
            TotalStepCounterText.Text = $"Tura: {totalSteps}";


        }
        /// <summary>
        /// Obsługuje przycisk „Zapisz wyniki” — zapisuje statystyki i stan sieci do pliku.
        /// </summary>
        private void SaveResults_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Plik tekstowy (*.txt)|*.txt",
                Title = "Zapisz wyniki symulacji",
                FileName = $"wyniki_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var metrics = manager.CalculateMetrics();
                ResultWriter.WriteResultsToFile(manager.Sensors, saveFileDialog.FileName, metrics);
                MessageBox.Show("Wyniki zapisane pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }
        /// <summary>
        /// Otwiera okno wykresów (wykres kołowy i słupkowy).
        /// </summary>
    
        private void ShowCharts_Click(object sender, RoutedEventArgs e)
        {
            var window = new ChartsWindow(manager.Sensors);
            window.Show();
        }
        /// <summary>
        /// Wczytuje konfigurację sieci z pliku tekstowego.
        /// </summary>

        private void LoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Pliki tekstowe (*.txt)|*.txt",
                Title = "Wybierz plik z konfiguracją"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ConfigLoader.LoadFromFile(dialog.FileName, out var sensors, out var pois, out var sink);

                    manager.Sensors = sensors;
                    manager.POIs = pois;
                    manager.SinkNode = sink;

                    manager.DetectNeighbors();
                    manager.DetectPOIs();

                    DrawNetwork();
                }
                catch (FormatException fe)
                {
                    MessageBox.Show("Błąd formatu pliku konfiguracyjnego:\n" + fe.Message,
                                    "Nieprawidłowy format pliku",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Wystąpił nieoczekiwany błąd podczas wczytywania pliku:\n" + ex.Message,
                                    "Błąd",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Uruchamia automatyczną symulację.
        /// </summary>

        private async void AutoSimulate_Click(object sender, RoutedEventArgs e)
        {
            const int steps = 10;

            for (int i = 0; i < steps; i++)
            {
                int remaining = steps - i;
                AutoSimulationCounterText.Text = $"Pozostało tur: {remaining}";
                totalSteps++;
                TotalStepCounterText.Text = $"Tura: {totalSteps}";

                manager.DetectPOIs();

                foreach (var s in manager.Sensors)
                {
                    s.AlreadyTransmittedThisStep = false;
                    s.LostPacketLastStep = false;
                }


                foreach (var sensor in manager.Sensors.ToList())
                {
                    if (sensor.HasPOIInRange && sensor.IsAlive)
                    {
                        manager.TransmitToSink(sensor, out _);
                    }
                }

                manager.UpdateEnergy();
                DrawNetwork();

                await Task.Delay(1000);
            }

            AutoSimulationCounterText.Text = "Auto-symulacja zakończona.";
        }
        /// <summary>
        /// Obsługuje zmianę wybranego protokołu routingu.
        /// </summary>

        private void RoutingSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (manager == null) return;

            if (RoutingSelector.SelectedIndex == 0)
                manager.CurrentProtocol = RoutingProtocol.ShortestPath;
            else
                manager.CurrentProtocol = RoutingProtocol.MinimumEnergy;
        }

                /// <summary>
        /// Losuje nowe poziomy energii dla wszystkich sensorów.
        /// </summary>

        private void RandomizeEnergy_Click(object sender, RoutedEventArgs e)
        {
            if (manager?.Sensors == null || manager.Sensors.Count == 0)
            {
                MessageBox.Show("Najpierw wygeneruj sieć.");
                return;
            }

            Random rand = new Random();

            foreach (var sensor in manager.Sensors)
            {
                sensor.BatteryLevel = rand.Next(40, 101);
            }
            var avg = manager.Sensors.Average(s => s.BatteryLevel);
            MessageBox.Show($"Średnia energia: {avg:F2}");

            MessageBox.Show("Energia sensorów została losowo przydzielona.");
            DrawNetwork();

        }



    }



}

