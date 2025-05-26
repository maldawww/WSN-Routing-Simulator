namespace SensorNetworkSimulator.Simulation
{
    /// <summary>
    /// Enum reprezentujący dostępne protokoły routingu w sieci sensorowej.
    /// </summary>
    public enum RoutingProtocol
    {
        /// <summary>
        /// Protokół wybierający trasę o najmniejszej liczbie przeskoków (hopów).
        /// </summary>
        ShortestPath,

        /// <summary>
        /// Protokół wybierający trasę z najwyższą średnią energią sensorów.
        /// </summary>
        MinimumEnergy
    }
}
