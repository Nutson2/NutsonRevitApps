using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MEPGadgets.Scheme.View;
using NRPUtils.Filters;

namespace MEPGadgets.Scheme
{
    public class HydraulicScheme : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
			var doc = uiDoc.Document;
            var selRef = uiDoc.Selection.PickObject(ObjectType.Element, new MepElementFilter(), "Выберите элемент");
            var selEl = doc.GetElement(selRef.ElementId);

            SystemScheme Scheme = new SystemScheme(uiDoc,selEl);
            var SchemeView = new SchemeView(Scheme);
            SchemeView.Show();

            return Result.Succeeded;
        }
    
    }

}
