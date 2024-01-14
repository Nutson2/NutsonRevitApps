using System;
using UnitsNet;

namespace MEPGadgets.Scheme
{
    public class ConsumersTypeRate
    {
        public string TypeName { get; private set; }
        public ConsumptionCalculateType CalculateType { get; private set; }
        public VolumeFlow SecondsAppliancesRate { get; private set; }
        public VolumeFlow HoursAppliancesRate { get; private set; }

        public VolumeFlow DailyConsumptionRate { get; private set; }
        public VolumeFlow HoursConsumptionRate { get; private set; }

        public TimeSpan DurationOfConsumptionInHour { get; private set; }
        public TimeSpan DurationOfConsumptionInDay { get; private set; }
    }
}