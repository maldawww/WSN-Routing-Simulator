using SensorNetworkSimulator.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SensorNetworkSimulator.IO
{
    /// <summary>
    /// Klasa odpowiedzialna za wczytywanie konfiguracji sieci z pliku tekstowego.
    /// Obsługuje czujniki, punkty zainteresowania oraz centralę.
    /// </summary>
    public static class ConfigLoader
    {
        /// <summary>
        /// Wczytuje dane sieci z pliku tekstowego i inicjalizuje listy czujników, POI oraz centrali.
        /// Obsługiwane wpisy to: SENSOR, POI, SINK.
        /// </summary>
        /// <param name="path">Ścieżka do pliku konfiguracyjnego</param>
        /// <param name="sensors">Zwracana lista sensorów</param>
        /// <param name="pois">Zwracana lista punktów zainteresowania (POI)</param>
        /// <param name="sink">Zwracany obiekt centrali (CentralNode)</param>
        public static void LoadFromFile(string path, out List<Sensor> sensors, out List<POI> pois, out CentralNode sink)
        {
            sensors = new();
            pois = new();
            sink = null!;

            int lineNumber = 0;

            foreach (var line in File.ReadLines(path))
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    switch (parts[0])
                    {
                        case "SENSOR":
                            if (parts.Length != 6)
                                throw new FormatException($"Linia {lineNumber}: SENSOR powinien mieć 6 elementów.");

                            int id = int.Parse(parts[1]);
                            double x = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            double y = double.Parse(parts[3], CultureInfo.InvariantCulture);
                            double range = double.Parse(parts[4], CultureInfo.InvariantCulture);
                            double battery = double.Parse(parts[5], CultureInfo.InvariantCulture);

                            var sensor = new Sensor
                            {
                                Id = id,
                                X = x,
                                Y = y,
                                Range = range,
                                BatteryLevel = battery
                            };
                            sensors.Add(sensor);
                            break;

                        case "POI":
                            if (parts.Length != 3)
                                throw new FormatException($"Linia {lineNumber}: POI powinien mieć 3 elementy.");

                            double px = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double py = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            pois.Add(new POI { Id = pois.Count, X = px, Y = py });
                            break;

                        case "SINK":
                            if (parts.Length != 4)
                                throw new FormatException($"Linia {lineNumber}: SINK powinien mieć 4 elementy.");

                            double sx = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double sy = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            double srange = double.Parse(parts[3], CultureInfo.InvariantCulture);
                            sink = new CentralNode { Id = -1, X = sx, Y = sy, Range = srange };
                            break;

                        default:
                            throw new FormatException($"Linia {lineNumber}: Nieznany typ wpisu \"{parts[0]}\".");
                    }
                }
                catch (FormatException fe)
                {
                    throw new FormatException($"Błąd w linii {lineNumber}: {line}\n{fe.Message}");
                }
                catch (Exception ex)
                {
                    throw new FormatException($"Nieoczekiwany błąd w linii {lineNumber}: {line}\n{ex.Message}");
                }
            }
        }
    }
}
