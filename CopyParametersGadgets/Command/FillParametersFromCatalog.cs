using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace CopyParametersGadgets.Command
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class FillParametersFromCatalog : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            var strBuilder=new StringBuilder();
            var EnumCategory = new List<BuiltInCategory>()
                    {
                        BuiltInCategory.OST_PipeAccessory,
                        BuiltInCategory.OST_PlumbingFixtures,
                        BuiltInCategory.OST_PipeFitting,
                        BuiltInCategory.OST_MechanicalEquipment
                    };

            foreach (var category in EnumCategory)
            {
                var families=GetFamiliesInDocument(doc, category);
                foreach (var family in families)
                {
                    strBuilder.AppendLine( family.Name);
                }
            }

            var fod = new FolderBrowserDialog();
            if (fod.ShowDialog() == DialogResult.Cancel) return Result.Cancelled;
            var path =fod.SelectedPath;

            using(var stream=new StreamWriter(Path.Combine(path,doc.Title+"_Families.csv"),false,Encoding.Default))
            {
                stream.WriteLine(strBuilder.ToString());
            }
            return Result.Succeeded;

        }
        public  IEnumerable<Family> GetFamiliesInDocument(Document doc, BuiltInCategory builtInCategory)
        {
            var res=new List<Family>();
            var filter=new ElementCategoryFilter(builtInCategory);

            var coll = new FilteredElementCollector(doc);
            res = coll.OfCategory(builtInCategory)
                    .WhereElementIsElementType()
                    .Cast<FamilySymbol>()
                    .GroupBy(x => x.Family.Name)
                    .Select(x => x.First().Family)
                    .Where(x => x.IsEditable)
                    .ToList();

            return res;
        }

    }
}
