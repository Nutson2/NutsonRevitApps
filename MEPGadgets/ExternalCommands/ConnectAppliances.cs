using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using NRPUtils.MEPUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MEPGadgets
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class ConnectAppliances : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc   = commandData.Application.ActiveUIDocument.Document;
            var uiDoc = commandData.Application.ActiveUIDocument;
            IList<Reference> selRefs = default;
            try
            {
                selRefs = uiDoc.Selection.PickObjects(ObjectType.Element,
                                                        new MepElementFilter(),
                                                        "Выберите элементы");
            }
            catch (OperationCanceledException ex)
            {
                return Result.Cancelled;
            }

            var selElems = selRefs.Select(El => doc.GetElement(El.ElementId));

            var freeAppliancesConnectors = selElems
                .Where(el => el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures)
                .Select(el =>MEPUtils.GetConnectorManager(el).UnusedConnectors.Cast<Connector>()).SelectMany(x => x);


            var freeOtherConnectors = selElems
                .Where(el => el.Category.Id.IntegerValue != (int)BuiltInCategory.OST_PlumbingFixtures)
                .Select(el => MEPUtils.GetConnectorManager(el).UnusedConnectors.Cast<Connector>()).SelectMany(x => x);

            if (!freeAppliancesConnectors.Any() || !freeOtherConnectors.Any()) return Result.Cancelled;

            ElementId pipeTypeId = new FilteredElementCollector(doc).OfClass(typeof(FlexPipeType)).FirstElementId();

            using (Transaction tr = new Transaction(doc, "Connect appliances"))
            {
                tr.Start();

                foreach (var con in freeAppliancesConnectors)
                {
                    var curCon = freeOtherConnectors
                        .Where(  x => con.PipeSystemType == x.PipeSystemType)
                        .OrderBy(x => con.Origin.DistanceTo(x.Origin))
                        .FirstOrDefault();
                    if (curCon == null) continue;
                    if (con.Origin.DistanceTo(curCon.Origin) > UnitUtils.ConvertToInternalUnits(1, DisplayUnitType.DUT_METERS)) continue;

                    var fPipe = FlexPipe.Create(doc,
                                                curCon.MEPSystem.GetTypeId(),
                                                pipeTypeId,
                                                con.Owner.LevelId,
                                                new List<XYZ>() {   con.Origin,
                                                                    curCon.Origin });

                    fPipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(con.Radius * 2);
                    fPipe.StartTangent = con.CoordinateSystem.BasisZ;
                    fPipe.EndTangent = curCon.CoordinateSystem.BasisZ.Negate();

                    foreach (Connector pipeCon in fPipe.ConnectorManager.UnusedConnectors)
                    {
                        if (pipeCon.Origin.IsAlmostEqualTo(con.Origin))
                            pipeCon.ConnectTo(con);
                        else
                            pipeCon.ConnectTo(curCon);
                    }
                }
                tr.Commit();
            }
            return Result.Succeeded;
        }
    }


}
