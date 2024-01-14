using System.Collections.Generic;
using NRPUtils.MVVMBase;

namespace CopyParametersGadgets.WriteCalculation.Model
{
    public class CalculationModel : NotifyObject
    {
        private string category;
        private string parameterForWrite;
        private string parameterForSumming;
        public string Category { get => category; set { category = value; OnPropertyChanged(); } }
        public string ParameterForWrite { get => parameterForWrite; set { parameterForWrite = value; OnPropertyChanged(); } }
        public string ParameterForSumming { get => parameterForSumming; set { parameterForSumming = value; OnPropertyChanged(); } }
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> AvailableParametersForWrite { get; set; } = new List<string>();
        public List<string> AvailableParametersForSumming { get; set; } = new List<string>();

    }
}