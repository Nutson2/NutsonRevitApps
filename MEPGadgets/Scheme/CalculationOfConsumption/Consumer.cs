using System;

namespace MEPGadgets.Scheme
{
    public class Consumer
    {
        private int numberOfAppliances;
        private int numberOfConsumersPerDay;
        private int numberOfConsumersPerShift;

        public ConsumersTypeRate ConsumersTypeRate { get; private set; }
        public WaterConsumption WaterConsumption { get; private set; }
        public int NumberOfAppliances
        {
            get => numberOfAppliances;
            set
            {
                numberOfAppliances = value;
                OnNumberOfAppliancesChanged();
            }
        }
        public int NumberOfConsumersPerDay
        {
            get => numberOfConsumersPerDay;
            set
            {
                numberOfConsumersPerDay = value;
                OnNumberOfConsumersPerDayChanged();

            }
        }
        public int NumberOfConsumersPerShift
        {
            get => numberOfConsumersPerShift;
            set
            {
                numberOfConsumersPerShift = value;
                OnNumberOfConsumersPerShiftChanged();
            }
        }

        public Consumer(ConsumersTypeRate _ConsumersTypeRate)
        {
            ConsumersTypeRate = _ConsumersTypeRate;
            WaterConsumption = WaterConsumptionFabric.GetCalculator(ConsumersTypeRate.CalculateType, this);
        }

        #region Events
        public event Action OnNumberOfAppliancesChanged;
        public event Action OnNumberOfConsumersPerDayChanged;
        public event Action OnNumberOfConsumersPerShiftChanged;

        #endregion
    }
}