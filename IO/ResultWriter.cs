using SensorNetworkSimulator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SensorNetworkSimulator.IO
{
    /// <summary>
    /// Klasa pomocnicza odpowiedzialna za zapisywanie wyników symulacji do pliku tekstowego.
    /// Zawiera informacje o stanie czujników oraz metrykach sieci.
    /// </summary>
    public static class ResultWriter
    {
        /// <summary>
        /// Zapisuje wyniki symulacji do pliku tekstowego.
        /// Uwzględnia liczbę czujników w różnych stanach, dane szczegółowe oraz metryki sieci.
        /// </summary>
        /// <param name="sensors">Lista wszystkich sensorów w sieci</param>
        /// <param name="filePath">Ścieżka do pliku wynikowego</param>
        /// <param name="metrics">Krotka zawierająca metryki: pokrycie Q, PDR i latencję</param>
        public static void WriteResultsToFile(List<Sensor> sensors, string filePath, (double Q, double PDR, double Latency) metrics)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Wyniki symulacji – {DateTime.Now}");
            sb.AppendLine();

            int active = sensors.Count(s => s.Status == SensorStatus.Active);
            int sleeping = sensors.Count(s => s.Status == SensorStatus.Sleeping);
            int dead = sensors.Count(s => s.Status == SensorStatus.Dead);

            sb.AppendLine($"Liczba sensorów aktywnych: {active}");
            sb.AppendLine($"Liczba sensorów śpiących: {sleeping}");
            sb.AppendLine($"Liczba sensorów martwych: {dead}");
            sb.AppendLine();

            foreach (var sensor in sensors)
            {
                sb.AppendLine($"Sensor ID: {sensor.Id}");
                sb.AppendLine($"  Status: {sensor.Status}");
                sb.AppendLine($"  Bateria: {sensor.BatteryLevel:F2}");
                sb.AppendLine($"  Sąsiedzi ({sensor.Neighbors.Count}): {string.Join(", ", sensor.Neighbors.Select(n => n.Id))}");
                sb.AppendLine();
            }

            sb.AppendLine("------ METRYKI SIECI ------");
            sb.AppendLine($"Coverage (Q): {metrics.Q:F2} %");
            sb.AppendLine($"Packet Delivery Ratio (PDR): {metrics.PDR:F2} %");
            sb.AppendLine($"Średnia latencja (skoki): {metrics.Latency:F2}");

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
