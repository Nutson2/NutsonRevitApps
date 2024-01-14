using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
namespace MEPGadgets
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    class DisconnectElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;
            UIApplication app = revit.Application;

            var selRefs = app.ActiveUIDocument.Selection.PickObjects(ObjectType.Element, "Выбери элемент с коннекторами");
            if (selRefs == null) return Result.Cancelled;

            using (Transaction tr = new Transaction(doc, "Disconnect element"))
            {
                tr.Start();
                foreach (Reference selRef in selRefs)
                {
                    if (!(doc.GetElement(selRef.ElementId) is FamilyInstance elem)) continue;
                    ConnectorManager elemConnectManager = elem.MEPModel.ConnectorManager;
                    if (elemConnectManager == null) continue;

                    foreach (Connector curConnector in elemConnectManager.Connectors)
                    {
                        var connectorRefs = curConnector.AllRefs;
                        foreach (Connector connectorRef in connectorRefs)
                        {
                            curConnector.DisconnectFrom(connectorRef);
                        }
                    }
                }
                tr.Commit();
            }
            return Result.Succeeded;
        }
    }


}
