using System;
using System.Collections.Generic;

namespace SensorNetworkSimulator.Models
{
    /// <summary>
    /// Status sensora w danym kroku symulacji.
    /// </summary>
    public enum SensorStatus
    {
        /// <summary>
        /// Sensor aktywny – wykrył POI i uczestniczy w transmisji.
        /// </summary>
        Active,

        /// <summary>
        /// Sensor uśpiony – nie wykrył POI.
        /// </summary>
        Sleeping,

        /// <summary>
        /// Sensor martwy – nie ma energii lub uległ awarii.
        /// </summary>
        Dead
    }

    /// <summary>
    /// Klasa reprezentująca sensor (czujnik) w sieci.
    /// Dziedziczy z BaseNode i zawiera rozszerzone właściwości oraz metody związane z energią i komunikacją.
    /// </summary>
    public class Sensor : BaseNode
    {
        /// <summary>
        /// Aktualny status sensora (aktywny, śpiący lub martwy).
        /// </summary>
        public SensorStatus Status { get; set; } = SensorStatus.Sleeping;

        /// <summary>
        /// Aktualny poziom baterii sensora.
        /// </summary>
        public double BatteryLevel { get; set; } = 100.0;

        /// <summary>
        /// Lista sąsiadujących sensorów znajdujących się w zasięgu komunikacyjnym.
        /// </summary>
        public List<Sensor> Neighbors { get; set; } = new();

        /// <summary>
        /// Flaga wskazująca, czy sensor wykrył jakikolwiek POI w bieżącym kroku symulacji.
        /// </summary>
        public bool HasPOIInRange { get; set; } = false;

        /// <summary>
        /// Zużywa określoną ilość energii z baterii i aktualizuje status sensora.
        /// </summary>
        /// <param name="amount">Ilość energii do odjęcia</param>
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

        /// <summary>
        /// Zużywa energię oraz wypisuje debugową informację o stanie sensora.
        /// Aktualizuje status na Dead, jeśli energia spadnie do zera.
        /// </summary>
        /// <param name="amount">Ilość zużytej energii</param>
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

        /// <summary>
        /// Flaga oznaczająca, czy sensor już transmitował w bieżącym kroku symulacji.
        /// </summary>
        public bool AlreadyTransmittedThisStep { get; set; } = false;

        /// <summary>
        /// Flaga oznaczająca, czy sensor uległ losowej awarii (nie przez wyczerpanie energii).
        /// </summary>
        public bool DiedByFailure { get; set; } = false;

        /// <summary>
        /// Flaga oznaczająca, że pakiet wysyłany przez ten sensor został zgubiony w poprzednim kroku.
        /// </summary>
        public bool LostPacketLastStep { get; set; } = false;
    }
}
