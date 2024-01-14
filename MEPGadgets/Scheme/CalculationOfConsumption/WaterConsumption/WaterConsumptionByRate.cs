namespace MEPGadgets.Scheme
{
    public class WaterConsumptionByRate:WaterConsumption
    {
        public WaterConsumptionByRate( Consumer Consumer)
            : base( Consumer)
        {
            consumer.OnNumberOfConsumersPerShiftChanged += Consumer_OnNumberOfConsumersPerShiftChanged;
            RewriteConsumption();
            CalculateDailyConsumption();
        }
        internal  void Consumer_OnNumberOfConsumersPerShiftChanged()
        {
            RewriteConsumption();
        }

        private void RewriteConsumption()
        {
            MaximumSecondsConsumption = rate.SecondsAppliancesRate * consumer.NumberOfConsumersPerShift;
            MaximumHoursConsumption   = rate.HoursAppliancesRate   * consumer.NumberOfConsumersPerShift;
        }

    }
}