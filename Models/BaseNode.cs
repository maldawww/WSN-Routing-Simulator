namespace SensorNetworkSimulator.Models
{
    /// <summary>
    /// Bazowa klasa dla węzłów w sieci sensorowej.
    /// Zawiera podstawowe właściwości wspólne dla sensorów i centrali.
    /// </summary>
    public class BaseNode
    {
        /// <summary>
        /// Unikalny identyfikator węzła.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Pozycja X węzła w przestrzeni 2D.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Pozycja Y węzła w przestrzeni 2D.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Zasięg komunikacyjny węzła.
        /// </summary>
        public double Range { get; set; }

        /// <summary>
        /// Status aktywności węzła (true = działa, false = uszkodzony).
        /// </summary>
        public bool IsAlive { get; set; } = true;
    }
}
