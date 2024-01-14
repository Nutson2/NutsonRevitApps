using MEPGadgets.Scheme.Model;

namespace MEPGadgets.Scheme
{
    internal class WaterConsumptionCalculator : ICalculator
    {
        private readonly SchemeBranch branch;
        public WaterConsumptionCalculator(SchemeBranch schemeBranch)
        {
            branch = schemeBranch;
        }
        public void Calculate()
        {
            throw new System.NotImplementedException();
        }
    }
}