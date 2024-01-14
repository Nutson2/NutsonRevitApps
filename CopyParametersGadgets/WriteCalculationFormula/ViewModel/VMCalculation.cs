using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CopyParametersGadgets.WriteCalculation.Model;
using mmOrderMarking.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CopyParametersGadgets.WriteCalculation.ViewModel
{
    public class VMCalculation
    {
        private readonly UIApplication uiApp;
        private readonly Document doc;
        private readonly List<string> categories=new List<string>();
        public ObservableCollection<CalculationModel> CalculationModels { get; set; } =new ObservableCollection<CalculationModel>();

        public List<string> Categories                    { get; set; } = new List<string>();
        public List<string> AvailableParametersForWrite   { get; set; } = new List<string>();
        public List<string> AvailableParametersForSumming { get; set; } = new List<string>();

        public VMCalculation(UIApplication uiApp)
        {
            this.uiApp = uiApp;
            doc = uiApp.ActiveUIDocument.Document;
            
            var scheduleDef = ((ViewSchedule)doc.ActiveView).Definition;

            var fieldsID    = scheduleDef .GetFieldOrder();
            var fields      = fieldsID.Select(x => scheduleDef.GetField(x))
                                    .Where(x=> x.FieldType==ScheduleFieldType.Instance ||
                                                x.FieldType==ScheduleFieldType.ElementType||
                                                x.FieldType==ScheduleFieldType.ViewBased||
                                                x.FieldType==ScheduleFieldType.Room||
                                                x.FieldType==ScheduleFieldType.FromRoom||
                                                x.FieldType==ScheduleFieldType.ToRoom||
                                                x.FieldType==ScheduleFieldType.ProjectInfo||
                                                x.FieldType==ScheduleFieldType.Material||
                                                x.FieldType==ScheduleFieldType.MaterialQuantity);

            if(scheduleDef.CategoryId==ElementId.InvalidElementId)
            {
            foreach (Category curCategory in doc.Settings.Categories)
            {
                    if (curCategory.CategoryType != CategoryType.Model || curCategory.Name.Contains(".dwg")) continue;
                categories.Add(curCategory.Name);
            }
            categories = categories.OrderBy(x => x).ToList();
        }
            else
            {
                var catBuilt = (BuiltInCategory)scheduleDef.CategoryId.IntegerValue ;
                var cat      = doc.Settings.Categories.get_Item(catBuilt);
                categories.Add(cat.Name);
            }

            //fields.Where(x => x. == ParameterType.Text)
            //                .OrderBy(x => x.GetName())
            //                .ToList()
            //                .ForEach(x =>
            //                {
            //                    AvailableParametersForWrite.Add(x.GetName());
            //                });

            fields.OrderBy(x => x.GetName())
                            .ToList()
                            .ForEach(x =>
                            {
                                AvailableParametersForWrite.Add(x.GetName());
                                AvailableParametersForSumming.Add(x.GetName());
                            });

        }

        public CalculationModel AddCalculationModel()
        {
            var model = new CalculationModel()
            {
                Categories=categories,
                AvailableParametersForSumming=AvailableParametersForSumming,
                AvailableParametersForWrite=AvailableParametersForWrite
            };
            CalculationModels.Add(model) ;
            return model;
        }

        public void Apply()
        {
            var numerateService = new NumerateService(uiApp);
            var dict= CalculationModels.GroupBy(x=>x.Category).ToDictionary(x => x.Key, y => y.ToArray());
            numerateService.CalculateValue(dict);

            
        }
    }
}
