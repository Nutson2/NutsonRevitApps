using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace CopyParametersGadgets.WriteCalculation.Model
{
    public partial class CalculationModel : ObservableObject
    {
        [ObservableProperty]
        private string _category;
        [ObservableProperty]
        private string _parameterForWrite;
        [ObservableProperty]
        private string _parameterForSumming;

        public List<string> Categories { get; set; } = new List<string>();
        public List<string> AvailableParametersForWrite { get; set; } = new List<string>();
        public List<string> AvailableParametersForSumming { get; set; } = new List<string>();

    }
}