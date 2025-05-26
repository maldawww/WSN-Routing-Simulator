using LiveCharts;
using LiveCharts.Wpf;
using SensorNetworkSimulator.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LiveCharts.Configurations;

namespace SensorNetworkSimulator
{

    /// <summary>
    /// Okno pomocnicze wyświetlające wykresy stanu sieci sensorowej.
    /// Zawiera wykres kołowy dla statusów oraz wykres słupkowy poziomu baterii.
    /// </summary>
    /// 
    public partial class ChartsWindow : Window
    {

        /// <summary>
        /// Kolekcja danych do wykresu kołowego pokazującego stan sensorów.
        /// </summary>
        public SeriesCollection SensorStateSeries { get; set; }

        /// <summary>
        /// Kolekcja danych do wykresu słupkowego przedstawiającego poziom baterii.
        /// </summary>
        public SeriesCollection BatterySeries { get; set; }
        /// <summary>
        /// Oś X wykresu słupkowego (etykiety z nazwami sensorów).
        /// </summary>
        /// 
        public AxesCollection BatteryLabelsX { get; set; }
        /// <summary>
        /// Oś Y wykresu słupkowego (wartości baterii).
        /// </summary>
        public AxesCollection BatteryAxisY { get; set; }
        /// <summary>
        /// Tworzy nowe okno wykresów na podstawie listy sensorów.
        /// Inicjalizuje dane do wykresu kołowego i słupkowego.
        /// </summary>
        /// <param name="sensors">Lista sensorów do wizualizacji</param>
        /// 
        public ChartsWindow(List<Sensor> sensors)
        {
            InitializeComponent();

            // PieChart – stan sensorów
            int active = sensors.Count(s => s.Status == SensorStatus.Active);
            int sleeping = sensors.Count(s => s.Status == SensorStatus.Sleeping);
            int dead = sensors.Count(s => s.Status == SensorStatus.Dead);

            SensorStateSeries = new SeriesCollection
            {
                new PieSeries { Title = "Aktywne", Values = new ChartValues<int> { active } },
                new PieSeries { Title = "Śpiące", Values = new ChartValues<int> { sleeping } },
                new PieSeries { Title = "Martwe", Values = new ChartValues<int> { dead } }
            };

            // ColumnChart – poziom baterii
            var batteryLevels = sensors.OrderBy(s => s.Id).Select(s => s.BatteryLevel).ToList();
            var sensorLabels = sensors.OrderBy(s => s.Id).Select(s => $"S{s.Id}").ToList();

            BatterySeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Bateria",
                    Values = new ChartValues<double>(batteryLevels)
                }
            };

            BatteryLabelsX = new AxesCollection
            {
                new Axis
                {
                    Title = "Sensory",
                    Labels = sensorLabels
                }
            };

            BatteryAxisY = new AxesCollection
            {
                new Axis
                {
                    Title = "Poziom baterii",
                    MinValue = 0,
                    MaxValue = 100
                }
            };

            DataContext = this;
        }
    }
}
