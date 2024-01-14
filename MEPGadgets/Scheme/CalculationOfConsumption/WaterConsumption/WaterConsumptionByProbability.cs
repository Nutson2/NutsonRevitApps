namespace MEPGadgets.Scheme
{
    public class WaterConsumptionByProbability: WaterConsumption
    {
        readonly ProbabilityRate SecondsRate;
        readonly ProbabilityRate HoursRate;

        public WaterConsumptionByProbability(Consumer Consumer)
            :base( Consumer)
        {
            consumer.OnNumberOfConsumersPerShiftChanged += Consumer_OnNumberOfConsumersPerShiftChanged;
            consumer.OnNumberOfAppliancesChanged += Consumer_OnNumberOfAppliancesChanged;

            SecondsRate = new ProbabilityRate(rate.HoursConsumptionRate.LitersPerHour, rate.SecondsAppliancesRate.LitersPerHour);
            HoursRate   = new ProbabilityRate(rate.HoursConsumptionRate.LitersPerHour, rate.HoursAppliancesRate.LitersPerHour);
            
            RewriteConsumption();
            CalculateDailyConsumption();
        }
        #region Handlers
        internal void Consumer_OnNumberOfConsumersPerShiftChanged()
        {
            SecondsRate.NumberOfConsumersPerShift = consumer.NumberOfConsumersPerShift;
            HoursRate.NumberOfConsumersPerShift   = consumer.NumberOfConsumersPerShift;
            RewriteConsumption();
        }
        internal  void Consumer_OnNumberOfAppliancesChanged()
        {
            SecondsRate.NumberOfAppliances = consumer.NumberOfAppliances;
            HoursRate.NumberOfAppliances   = consumer.NumberOfAppliances;
            RewriteConsumption();
        }

        #endregion
        private void RewriteConsumption()
        {
            MaximumSecondsConsumption = SecondsRate.Consumption;
            MaximumHoursConsumption   = HoursRate.Consumption;
        }
    }
}