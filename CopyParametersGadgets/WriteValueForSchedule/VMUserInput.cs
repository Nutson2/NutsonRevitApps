using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyParametersGadgets.Properties;
using NRPUtils.MVVMBase;

namespace CopyParametersGadgets.Command
{
    public partial class VMUserInput : ObservableObject
    {
        [ObservableProperty]
        private double _pipeSafetyFactor;

        [ObservableProperty]
        private double _pipeInsulationSafetyFactor;

        [ObservableProperty]
        private int _pipeRound;

        [ObservableProperty]
        private int _insulationRound;
        private readonly UIDocument uiDoc;

        partial void OnPipeSafetyFactorChanged(double value)
        {
            Properties.Settings.Default.SchedulePipeK = value;
        }

        partial void OnPipeInsulationSafetyFactorChanged(double value)
        {
            Properties.Settings.Default.SchedulePipeInsulationK = value;
        }

        partial void OnPipeRoundChanged(int value)
        {
            Properties.Settings.Default.PipeRound = value;
        }

        partial void OnInsulationRoundChanged(int value)
        {
            Properties.Settings.Default.InsulationRound = value;
        }

        public VMUserInput(UIDocument UIDoc)
        {
            uiDoc = UIDoc;
            PipeSafetyFactor = Settings.Default.SchedulePipeK;
            PipeInsulationSafetyFactor = Settings.Default.SchedulePipeInsulationK;
            PipeRound = Settings.Default.PipeRound;
            InsulationRound = Settings.Default.InsulationRound;
        }

        [RelayCommand]
        public void Apply()
        {
            Properties.Settings.Default.Save();
            var service = new ServiceCopyParametersValue(uiDoc, this);
            service.CopyParamValue();
        }
    }
}
