using System.Collections.Generic;

namespace SensorNetworkSimulator.Models
{
    public enum SensorStatus { Active, Sleeping, Dead }

    public class Sensor : BaseNode
    {
        public SensorStatus Status { get; set; } = SensorStatus.Sleeping;
        public double BatteryLevel { get; set; } = 100.0;
        public List<Sensor> Neighbors { get; set; } = new();
        public bool HasPOIInRange { get; set; } = false;

        public void UpdateBattery(double amount)
        {
            BatteryLevel -= amount;
            if (BatteryLevel <= 0)
            {
                BatteryLevel = 0;
                Status = SensorStatus.Dead;
                IsAlive = false;
            }
        }
        public void ConsumeEnergy(double amount)
        {
            BatteryLevel -= amount;

            // Log do debugowania
            Console.WriteLine($"Sensor {Id} | Status: {Status} | -{amount} | Pozostało: {BatteryLevel:F1}");

            if (BatteryLevel <= 0)
            {
                BatteryLevel = 0;
                Status = SensorStatus.Dead;
                IsAlive = false;
                Console.WriteLine($"Sensor {Id} umarł ⚰️");
            }
        }

        public bool AlreadyTransmittedThisStep { get; set; } = false;
        public bool DiedByFailure { get; set; } = false;

        public bool LostPacketLastStep { get; set; } = false;





    }
}
