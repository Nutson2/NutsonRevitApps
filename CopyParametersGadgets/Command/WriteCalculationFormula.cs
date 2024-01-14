using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CopyParametersGadgets.WriteCalculation.View;
using CopyParametersGadgets.WriteCalculation.ViewModel;
using System.Collections.Specialized;
using System.Linq;
using Settings = CopyParametersGadgets.Properties.Settings;

namespace CopyParametersGadgets.Command
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class WriteCalculationFormula : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (!(commandData.Application.ActiveUIDocument.Document.ActiveView is ViewSchedule))
            {
                TaskDialog.Show("Ворнинг", "Должна быть открыта спецификация");
                return Result.Cancelled;
            } 

            var uiApp = commandData.Application;
            var vm    = new VMCalculation(uiApp);
            var view  = new ViewCalculation(vm);

            LoadSetting(vm);
            view.ShowDialog();
            SaveSetting(vm);

            return Result.Succeeded;
        }

        #region Save/Load settings
        private void SaveSetting(VMCalculation VM)
        {
            StringCollection calculationModels=new StringCollection();
            VM.CalculationModels
                .Select(x =>
                    string.Join("/", new string[] { x.Category, x.ParameterForWrite, x.ParameterForSumming }))
                .ToList()
                .ForEach(x => calculationModels.Add(x));
            
            if (calculationModels == null || calculationModels.Count < 1) return;

            Settings.Default.CalculationModelList = calculationModels;
            Settings.Default.Save();
        }
        private void LoadSetting(VMCalculation VM)
        {
            var settings = Settings.Default.CalculationModelList;
            if (settings == null || settings?.Count < 1) return;
            foreach (var row in settings)
            {
                if (string.IsNullOrEmpty(row)) continue;
                var parts=row.Split("/".ToArray(), System.StringSplitOptions.RemoveEmptyEntries );
                if (parts.Length <= 0) continue;
                
                var model =VM.AddCalculationModel();
                model.Category = parts[0];
                model.ParameterForWrite = parts[1];
                model.ParameterForSumming = parts[2];
            } 
        }

        #endregion    
    }
}
