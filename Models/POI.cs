namespace SensorNetworkSimulator.Models
{
    /// <summary>
    /// Klasa reprezentująca punkt zainteresowania (POI) w symulacji.
    /// Czujniki wykrywają POI, jeśli znajdują się w ich zasięgu.
    /// </summary>
    public class POI
    {
        /// <summary>
        /// Unikalny identyfikator punktu zainteresowania.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Pozycja X punktu zainteresowania w przestrzeni 2D.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Pozycja Y punktu zainteresowania w przestrzeni 2D.
        /// </summary>
        public double Y { get; set; }
    }
}
