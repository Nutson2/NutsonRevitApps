using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace MEPGadgets
{
    public class MepElementFilter : ISelectionFilter
    {

        public bool AllowElement(Element elem)
        {
            if (elem is MEPCurve || elem is FamilyInstance) return true;
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }


}
