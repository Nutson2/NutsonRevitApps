using Autodesk.Revit.UI;
using NRPUtils.MVVMBase;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CopyParametersGadgets.Command
{
    public partial class VMUserInput : ObservableObject  //NotifyObject
    {
        [ObservableProperty]
        private double _pipeSafetyFactor;
        [ObservableProperty]
        private double _pipeInsulationSafetyFactor;
        [ObservableProperty]
        private int    _pipeRound;
        [ObservableProperty]
        private int    _insulationRound;
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
            PipeSafetyFactor           = Properties.Settings.Default.SchedulePipeK;
            PipeInsulationSafetyFactor = Properties.Settings.Default.SchedulePipeInsulationK;
            PipeRound                  = Properties.Settings.Default.PipeRound;
            InsulationRound            = Properties.Settings.Default.InsulationRound;
        }
        [RelayCommand]
        public void Apply()
        {
            Properties.Settings.Default.Save();
            var service=new ServiceCopyParametersValue(uiDoc,this);
            service.CopyParamValue();

        }
         
    }
}
