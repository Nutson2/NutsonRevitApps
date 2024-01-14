using MEPGadgets.Scheme.Model;
using UnitsNet;

namespace MEPGadgets.Scheme
{
    public class WaterConsumption:NotifyObject
    {
        internal Consumer consumer;
        internal ConsumersTypeRate rate;

        private VolumeFlow maximumSecondsConsumption;

        private VolumeFlow maximumHoursConsumption;
        private VolumeFlow minimumHoursConsumption;
        private VolumeFlow averageHoursConsumption;

        private VolumeFlow averageDailyConsumption;

        public VolumeFlow MaximumSecondsConsumption
        {
            get { return maximumSecondsConsumption; }
            set
            {
                maximumSecondsConsumption = value;
                OnPropertyChanged();
            }
        }

        public VolumeFlow MaximumHoursConsumption
        {
            get { return maximumHoursConsumption; }
            set
            {
                maximumHoursConsumption = value;
                OnPropertyChanged();
            }
        }
        public VolumeFlow MinimumHoursConsumption
        {
            get { return minimumHoursConsumption; }
            set
            {
                minimumHoursConsumption = value;
                OnPropertyChanged();
            }
        }
        public VolumeFlow AverageHoursConsumption
        {
            get { return averageHoursConsumption; }
            set
            {
                averageHoursConsumption = value;
                OnPropertyChanged();
            }
        }

        public VolumeFlow AverageDailyConsumption
        {
            get { return averageDailyConsumption; }
            set
            {
                averageDailyConsumption = value;
                OnPropertyChanged();
            }
        }

        public WaterConsumption( Consumer Consumer)
        {
            consumer = Consumer;
            rate = consumer.ConsumersTypeRate;

            consumer.OnNumberOfConsumersPerDayChanged += Consumer_OnNumberOfConsumersPerDayChanged;
        }
        private void Consumer_OnNumberOfConsumersPerDayChanged()
        {
            CalculateDailyConsumption();
        }
        internal void CalculateDailyConsumption()
        {
            AverageDailyConsumption = consumer.NumberOfConsumersPerDay * rate.DailyConsumptionRate;
            AverageHoursConsumption =VolumeFlow.FromCubicMetersPerHour(AverageDailyConsumption.CubicMetersPerDay / 
                                                                        rate.DurationOfConsumptionInDay.Hours);
            MinimumHoursConsumption = CalculateMinimumHoursConsumption();
        }

        private VolumeFlow CalculateMinimumHoursConsumption()
        {
            var Kmax = MaximumHoursConsumption / AverageHoursConsumption;
            var Kmin = GetMinCoefficient(Kmax);
            return AverageHoursConsumption*Kmin;
        }

        private double GetMinCoefficient(double kmax)
        {
            //todo подбор Кмин
            return 0;
        }
    }
}