using SensorNetworkSimulator.Models;
using System;
using System.Collections.Generic;

using SensorNetworkSimulator.Communication;
using SensorNetworkSimulator.Simulation;



namespace SensorNetworkSimulator.Simulation
{
    /// <summary>
    /// Główna klasa zarządzająca symulacją sieci sensorowej.
    /// Obsługuje transmisję, awarie, detekcję POI i metryki.
    /// </summary>

    public class SensorManager
    {
        /// <summary>Łączna liczba zgubionych pakietów w symulacji.</summary>
        public int TotalPacketLosses { get; private set; } = 0;
        /// <summary>Łączna liczba losowych awarii sensorów.</summary>
        public int TotalFailures { get; private set; } = 0;
        /// <summary>Lista wszystkich sensorów w sieci.</summary>
        public List<Sensor> Sensors { get; set; } = new();
        /// <summary>Lista punktów zainteresowania (POI) w sieci.</summary>
        public List<POI> POIs { get; set; } = new();
        /// <summary>Centrala (sink) odbierająca dane z sieci.</summary>
        public CentralNode SinkNode { get; set; } = null!;
        /// <summary>Aktualnie wybrany protokół routingu.</summary>
        public RoutingProtocol CurrentProtocol { get; set; } = RoutingProtocol.ShortestPath;
        /// <summary>Szansa na zgubienie pakietu w % (np. 5.0).</summary>
        public double PacketLossChancePercent { get; set; } = 5.0;
        /// <summary>Szansa na losową awarię sensora w % (np. 2.0).</summary>
        public double FailureChancePercent { get; set; } = 2.0;

        /// <summary>
        /// Oblicza metryki sieci: pokrycie Q, PDR i średnią latencję.
        /// </summary>
        /// <returns>Krotka (Q, PDR, Latency)</returns>

        public (double Q, double PDR, double AvgLatency) CalculateMetrics()
        {
            int detectedPOIs = 0;
            int totalPOIs = POIs.Count;

            HashSet<int> coveredPOIs = new();
            int successfulTransmissions = 0;
            double totalLatency = 0;
            int countWithPath = 0;

            foreach (var sensor in Sensors)
            {
                if (!sensor.IsAlive || !sensor.HasPOIInRange)
                    continue;

                // Zliczamy Coverage (POI w zasięgu)
                foreach (var poi in POIs)
                {
                    if (Distance(sensor, poi) <= sensor.Range)
                    {
                        coveredPOIs.Add(poi.Id);
                    }
                }

                // Próbujemy przesłać dane
                if (TransmitToSink(sensor, out var path))
                {
                    successfulTransmissions++;
                    totalLatency += path.Count - 1; // -1 bo nie liczymy źródła
                    countWithPath++;
                }
            }

            double Q = totalPOIs > 0 ? (coveredPOIs.Count / (double)totalPOIs) * 100 : 0;
            double PDR = successfulTransmissions > 0 ? (successfulTransmissions / (double)Sensors.Count(s => s.HasPOIInRange)) * 100 : 0;
            double AvgLatency = countWithPath > 0 ? totalLatency / countWithPath : 0;

            return (Q, PDR, AvgLatency);
        }


        private Random rand = new();
        /// <summary>
        /// Generuje losowe pozycje sensorów na polu o zadanych wymiarach i zakresie.
        /// </summary>
        public void GenerateSensors(int count, double fieldWidth, double fieldHeight, double range)
        {
            Sensors.Clear();

            double margin = range + 5; // Zapobiega wychodzeniu poza ekran

            for (int i = 0; i < count; i++)
            {
                var sensor = new Sensor
                {
                    Id = i,
                    X = margin + rand.NextDouble() * (fieldWidth - 2 * margin),
                    Y = margin + rand.NextDouble() * (fieldHeight - 2 * margin),
                    Range = range
                };
                Sensors.Add(sensor);
            }
        }

        /// <summary>
        /// Generuje losowe pozycje POI (punktów zainteresowania).
        /// </summary>
        public void GeneratePOIs(int count, double fieldWidth, double fieldHeight)
        {
            POIs.Clear();
            for (int i = 0; i < count; i++)
            {
                var poi = new POI
                {
                    Id = i,
                    X = rand.NextDouble() * fieldWidth,
                    Y = rand.NextDouble() * fieldHeight
                };
                POIs.Add(poi);
            }
        }
        /// <summary>
        /// Ustawia centralę (sink) w zadanym miejscu z określonym zasięgiem.
        /// </summary>
        public void SetCentralNode(double x, double y, double range)
        {
            SinkNode = new CentralNode
            {
                Id = -1,
                X = x,
                Y = y,
                Range = range
            };
        }
        /// <summary>
        /// Wykrywa sąsiadów w zasięgu komunikacyjnym dla każdego sensora.
        /// </summary>
        public void DetectNeighbors()
        {
            foreach (var sensor in Sensors)
            {
                sensor.Neighbors.Clear();
                foreach (var other in Sensors)
                {
                    if (sensor == other) continue;
                    double dist = Distance(sensor, other);
                    if (dist <= sensor.Range)
                        sensor.Neighbors.Add(other);
                }
            }
        }

        /// <summary>
        /// Sprawdza, które sensory mają w zasięgu przynajmniej jeden POI.
        /// </summary>
        public void DetectPOIs()
        {
            foreach (var sensor in Sensors)
            {
                if (!sensor.IsAlive)
                {
                    sensor.Status = SensorStatus.Dead;
                    sensor.HasPOIInRange = false;
                    continue;
                }

                sensor.HasPOIInRange = false;
                foreach (var poi in POIs)
                {
                    if (Distance(sensor, poi) <= sensor.Range)
                    {
                        sensor.HasPOIInRange = true;
                        sensor.Status = SensorStatus.Active; // 👈 USTAWIAMY STAN
                        break;
                    }
                }

                if (!sensor.HasPOIInRange)
                    sensor.Status = SensorStatus.Sleeping;
            }
        }

        /// <summary>
        /// Oblicza odległość między dwoma węzłami.
        /// </summary>
        private double Distance(BaseNode a, BaseNode b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>
        /// Oblicza odległość między węzłem a punktem POI.
        /// </summary>
        private double Distance(BaseNode a, POI b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>
        /// Przeprowadza transmisję danych z podanego sensora do centrali, zgodnie z wybranym protokołem.
        /// </summary>
        /// <param name="start">Sensor źródłowy</param>
        /// <param name="path">Zwracana ścieżka do centrali</param>
        /// <returns>True, jeśli transmisja zakończyła się sukcesem</returns>

        public bool TransmitToSink(Sensor start, out List<int> path)
        {
            path = new List<int>();

            if (CurrentProtocol == RoutingProtocol.MinimumEnergy)
            {
                return TransmitUsingMinimumEnergy(start, out path);
            }

            if (!start.IsAlive)
                return false;

            if (start.LostPacketLastStep)
            {
                Console.WriteLine($"Sensor {start.Id}: Zgubił pakiet – nie próbuję wysyłać.");
                return false;
            }

            var visited = new HashSet<int>();
            var queue = new Queue<Packet>();

            queue.Enqueue(new Packet
            {
                SourceId = start.Id,
                Visited = new HashSet<int> { start.Id },
                Path = new List<int> { start.Id }
            });

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int lastId = current.Path.Last();

                BaseNode currentNode = lastId == -1 ? SinkNode : Sensors.Find(s => s.Id == lastId);

                if (Distance(currentNode, SinkNode) <= currentNode.Range)
                {
                    current.Path.Add(-1);
                    path = current.Path;

                    // 🔴 Dopiero teraz losujemy zgubienie
                    if (rand.NextDouble() < PacketLossChancePercent / 100.0)
                    {
                        Console.WriteLine($"Sensor {start.Id}: Pakiet zgubiony ❌");
                        start.LostPacketLastStep = true;
                        TotalPacketLosses++;

                        return false;
                    }

                    Console.WriteLine($"[{start.Id}] ➜ ŚCIEŻKA: {string.Join(" -> ", path)}");

                    foreach (int id in path)
                    {
                        if (id >= 0)
                        {
                            var sensor = Sensors.Find(s => s.Id == id);
                            if (sensor != null && sensor.IsAlive && !sensor.AlreadyTransmittedThisStep)
                            {
                                sensor.ConsumeEnergy(2.0);
                                sensor.AlreadyTransmittedThisStep = true;
                            }
                        }
                    }

                    return true;
                }

                if (currentNode is Sensor currentSensor)
                {
                    foreach (var neighbor in currentSensor.Neighbors)
                    {
                        if (!current.Visited.Contains(neighbor.Id) && neighbor.IsAlive)
                        {
                            queue.Enqueue(new Packet
                            {
                                SourceId = current.SourceId,
                                Visited = new HashSet<int>(current.Visited) { neighbor.Id },
                                Path = new List<int>(current.Path) { neighbor.Id }
                            });
                        }
                    }
                }




            }




            return false;
        }

        /// <summary>
        /// Wariant transmisji oparty na średniej energii (Minimum Energy Routing).
        /// </summary>
        private bool TransmitUsingMinimumEnergy(Sensor start, out List<int> path)
        {
            path = new List<int>();

            if (!start.IsAlive || start.LostPacketLastStep)
                return false;

            var visited = new HashSet<int>();
            var queue = new Queue<Packet>();

            queue.Enqueue(new Packet
            {
                SourceId = start.Id,
                Visited = new HashSet<int> { start.Id },
                Path = new List<int> { start.Id }
            });

            List<int> bestPath = null;
            double bestAvgEnergy = -1;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int lastId = current.Path.Last();

                BaseNode currentNode = lastId == -1 ? SinkNode : Sensors.Find(s => s.Id == lastId);

                if (Distance(currentNode, SinkNode) <= currentNode.Range)
                {
                    var fullPath = new List<int>(current.Path) { -1 };
                    var energyList = fullPath
                        .Where(id => id >= 0)
                        .Select(id => Sensors.Find(s => s.Id == id)?.BatteryLevel ?? 0)
                        .ToList();

                    double avgEnergy = energyList.Count > 0 ? energyList.Average() : 0;

                    if (avgEnergy > bestAvgEnergy)
                    {
                        bestAvgEnergy = avgEnergy;
                        bestPath = fullPath;
                    }

                    continue;
                }

                if (currentNode is Sensor currentSensor)
                {
                    var sortedNeighbors = currentSensor.Neighbors
                        .Where(n => n.IsAlive && !visited.Contains(n.Id)) 
                        .OrderByDescending(n => n.BatteryLevel);

                    foreach (var neighbor in sortedNeighbors)
                    {
                        visited.Add(neighbor.Id); 
                        queue.Enqueue(new Packet
                        {
                            SourceId = current.SourceId,
                            Visited = null, 
                            Path = new List<int>(current.Path) { neighbor.Id }
                        });
                    }
                }

            }

            if (bestPath == null)
            {
                Console.WriteLine($"Sensor {start.Id}: Nie znaleziono ścieżki do centrali ❌");
                return false;
            }

            path = bestPath;

            if (rand.NextDouble() < PacketLossChancePercent / 100.0)
            {
                Console.WriteLine($"Sensor {start.Id}: Pakiet zgubiony ❌");
                start.LostPacketLastStep = true;
                TotalPacketLosses++;
                return false;
            }

            Console.WriteLine($"[{start.Id}] ➜ ENERGIA ŚCIEŻKA: {string.Join(" -> ", path)}");

            foreach (int id in path)
            {
                if (id >= 0)
                {
                    var sensor = Sensors.Find(s => s.Id == id);
                    if (sensor != null && sensor.IsAlive && !sensor.AlreadyTransmittedThisStep)
                    {
                        sensor.ConsumeEnergy(2.0);
                        sensor.AlreadyTransmittedThisStep = true;
                    }
                }
            }

            return true;
        }



        /// <summary>
        /// Aktualizuje poziom energii dla każdego sensora w zależności od jego statusu.
        /// Obsługuje również losowe awarie.
        /// </summary>


        public void UpdateEnergy()
        {
            foreach (var sensor in Sensors)
            {
                if (!sensor.IsAlive) continue;

                switch (sensor.Status)
                {
                    case SensorStatus.Sleeping:
                        sensor.ConsumeEnergy(0.1);
                        break;

                    case SensorStatus.Active:
                        sensor.ConsumeEnergy(1.0);
                        break;

                    case SensorStatus.Dead:
                        // nic nie robimy
                        break;
                }

                //  2% szans na awarię sensora
                if (rand.NextDouble() < FailureChancePercent / 100.0)
                {
                    sensor.IsAlive = false;
                    sensor.Status = SensorStatus.Dead;
                    sensor.BatteryLevel = 0;
                    sensor.DiedByFailure = true; //    ustawiamy flagę
                    TotalFailures++;

                    Console.WriteLine($"Sensor {sensor.Id} uległ losowej awarii 💥");
                }

            }
        }



    }


}

