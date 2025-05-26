using SensorNetworkSimulator.Models;
using System.Collections.Generic;

namespace SensorNetworkSimulator.Communication
{
    /// <summary>
    /// Reprezentuje pakiet danych przesyłany przez sieć sensorową.
    /// Zawiera informacje o źródle oraz trasie przesyłu.
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// Identyfikator sensora, który wysłał pakiet.
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// Zbiór identyfikatorów sensorów, które już przetworzyły pakiet.
        /// Służy do unikania zapętleń w trasie.
        /// </summary>
        public HashSet<int> Visited { get; set; } = new();

        /// <summary>
        /// Kolejność węzłów, przez które przeszedł pakiet, zakończona centralą (-1).
        /// /// </summary>
        public List<int> Path { get; set; } = new();
    }
}
