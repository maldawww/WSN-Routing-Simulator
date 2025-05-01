using SensorNetworkSimulator.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SensorNetworkSimulator.IO
{
    public static class ConfigLoader
    {
        public static void LoadFromFile(string path, out List<Sensor> sensors, out List<POI> pois, out CentralNode sink)
        {
            sensors = new();
            pois = new();
            sink = null!;

            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

                switch (parts[0])
                {
                    case "SENSOR":
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
                        double px = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        double py = double.Parse(parts[2], CultureInfo.InvariantCulture);
                        pois.Add(new POI { Id = pois.Count, X = px, Y = py });
                        break;

                    case "SINK":
                        double sx = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        double sy = double.Parse(parts[2], CultureInfo.InvariantCulture);
                        double srange = double.Parse(parts[3], CultureInfo.InvariantCulture);
                        sink = new CentralNode { Id = -1, X = sx, Y = sy, Range = srange };
                        break;
                }
            }
        }
    }
}
