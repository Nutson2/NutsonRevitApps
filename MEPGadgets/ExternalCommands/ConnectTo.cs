using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using NRPUtils.MEPUtils;
using System;

namespace MEPGadgets
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class ConnectTo : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {

            Document      doc = revit.Application.ActiveUIDocument.Document;
            UIApplication app = revit.Application;

            Reference RefElementA;
            try
            {
                RefElementA = app.ActiveUIDocument.Selection.PickObject(ObjectType.Element, "Выбор элемента для присоединения (будет сдвигаться)");

            }
            catch (Exception) { return Result.Cancelled; }

            Element ElementA             = doc.GetElement(RefElementA.ElementId);
            ConnectorManager ElementA_CM = MEPUtils.GetConnectorManager(ElementA);
            Connector ElementAConnector  = MEPUtils.GetConnectorClosestTo(ElementA_CM.Connectors, RefElementA.GlobalPoint);
            if (ElementAConnector == null)
            {
                TaskDialog.Show("Внимание!", "У выбранного элемента нет свободных коннекторов.");
                return Result.Cancelled;
            }

            Reference RefElementB;
            try
            {
                RefElementB = app.ActiveUIDocument.Selection.PickObject(ObjectType.Element, "Выбор элемента к которому присоединяем");

            }
            catch (Exception) { return Result.Cancelled; }

            Element ElementB             = doc.GetElement(RefElementB.ElementId);
            ConnectorManager ElementB_CM = MEPUtils.GetConnectorManager(ElementB);
            Connector ElementBConnector  = MEPUtils.GetConnectorClosestTo(ElementB_CM.Connectors, RefElementB.GlobalPoint);

            if (ElementBConnector == null)
            {
                TaskDialog.Show("Внимание!", "У выбранного элемента нет свободных коннекторов.");
                return Result.Cancelled;
            }

            using (Transaction tr = new Transaction(doc, "Connect element"))
            {
                tr.Start();

                var ElementACon_Z = ElementAConnector.CoordinateSystem.BasisZ;
                var ElementBCon_Z = ElementBConnector.CoordinateSystem.BasisZ;
                var angle         = ElementACon_Z.AngleTo(ElementBCon_Z);
                XYZ vector        = null;
                if (Math.Round(angle, 2) != Math.Round(Math.PI, 2))
                {
                    vector = Math.Round(angle, 2) == 0 ? ElementAConnector.CoordinateSystem.BasisY :
                                                        ElementACon_Z.CrossProduct(ElementBCon_Z);

                    if (RefElementA.GlobalPoint != RefElementA.GlobalPoint + vector)
                    {
                        var line2 = Line.CreateBound(RefElementA.GlobalPoint, RefElementA.GlobalPoint + vector);
                        ElementA.Location.Rotate(line2, angle - Math.PI);
                    }
                }
                ElementA.Location.Move(ElementBConnector.Origin - ElementAConnector.Origin);

                ElementBConnector.ConnectTo(ElementAConnector);
                tr.Commit();
            }
            return Result.Succeeded;
        }
    }
}
