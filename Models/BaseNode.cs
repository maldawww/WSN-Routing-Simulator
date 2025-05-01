namespace SensorNetworkSimulator.Models
{
    public class BaseNode
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Range { get; set; }
        public bool IsAlive { get; set; } = true;
    }
}
