using System;
using UnitsNet;

namespace MEPGadgets.Scheme
{
    internal class ProbabilityRate
    {
        private readonly double consumptionRate;
        private readonly double appliancesRate;

        private int numberOfConsumersPerShift;
        private int numberOfAppliances;

        private double p;
        private double np;
        private double npq;
        private double coefficientAlfa;

        public int NumberOfConsumersPerShift
        {
            get { return numberOfConsumersPerShift; }
            set { numberOfConsumersPerShift = value; Calculation(); }
        }
        public int NumberOfAppliances
        {
            get { return numberOfAppliances; }
            set { numberOfAppliances = value; Calculation(); }
        }

        /// <summary>
        /// Probability of appliances use
        /// </summary>
        public double P
        {
            get { return p; }
        }
        /// <summary>
        /// Product number of appliances and probability
        /// </summary>
        public double NP
        {
            get { return np;           }
        }
        public double NPQ
        {
            get { return npq; }
        }
        public double CoefficientAlfa
        {
            get { return coefficientAlfa; }
        }
        public VolumeFlow Consumption
        {
            get { return VolumeFlow.FromLitersPerHour(5* appliancesRate* coefficientAlfa); }
        }

        public ProbabilityRate(double ConsumptionRateLitersPerHour, double AppliancesRateLitersPerHour)
        {
            consumptionRate = ConsumptionRateLitersPerHour;
            appliancesRate = AppliancesRateLitersPerHour;
            Calculation();
        }
        private void Calculation()
        {
            p= (consumptionRate * NumberOfConsumersPerShift) /
               appliancesRate       * NumberOfAppliances;
            np = (consumptionRate * NumberOfConsumersPerShift) /
                appliancesRate ;
            npq = (consumptionRate * NumberOfConsumersPerShift);
            coefficientAlfa = GetCoefficientAlfa(np);
        }
        private double GetCoefficientAlfa(double np)
        {
            throw new NotImplementedException();
        }
    }

}