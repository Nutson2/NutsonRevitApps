using System;

namespace MEPGadgets.Scheme
{
    public class WaterConsumptionFabric
    {
        public static WaterConsumption GetCalculator(ConsumptionCalculateType calculateType, Consumer consumer)
        {
            switch (calculateType) 
            {
                case ConsumptionCalculateType.ByRate:
                    return new WaterConsumptionByRate(consumer);
                    
                case ConsumptionCalculateType.ByProbability:
                    return new WaterConsumptionByProbability(consumer);
                default:
                    throw new Exception($"Неизвестный тип перечисления {nameof(ConsumptionCalculateType)} - {calculateType}" ) ;
            }
        }
    }
}