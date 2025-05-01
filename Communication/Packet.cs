using SensorNetworkSimulator.Models;
using System.Collections.Generic;

namespace SensorNetworkSimulator.Communication
{
    public class Packet
    {
        public int SourceId { get; set; }
        public HashSet<int> Visited { get; set; } = new();
        public List<int> Path { get; set; } = new();  // np. [2, 7, 12, Sink]
    }
}
