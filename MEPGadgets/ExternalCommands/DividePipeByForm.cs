using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Nice3point.Revit.Extensions;

namespace MEPGadgets
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class DividePipeByForm : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> pipes = collector
                .OfClass(typeof(Pipe))
                .WhereElementIsNotElementType()
                .ToElements();

            Element fitting = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_PipeFitting)
                .WhereElementIsElementType()
                .Where(x => x.Name == "СП_Сочленение_Типовое")
                .FirstOrDefault();

            ReferenceIntersector refInetrsector = new ReferenceIntersector(
                new ElementCategoryFilter(BuiltInCategory.OST_Mass),
                FindReferenceTarget.All,
                doc.ActiveView as View3D
            );

            using (Transaction tr = new Transaction(doc, "режем"))
            {
                tr.Start();

                foreach (Element el in pipes)
                {
                    Pipe curpipe = el as Pipe;
                    if (curpipe.GroupId.IntegerValue != -1)
                        continue;

                    var LCurve = curpipe.Location as LocationCurve;
                    var pipeLine = LCurve.Curve as Line;
                    var origin = pipeLine.GetEndPoint(0);

                    var intersections = refInetrsector
                        .Find(origin, pipeLine.Direction)
                        .Where(x =>
                            x.Proximity <= pipeLine.Length - UnitExtensions.FromMillimeters(10)
                        )
                        .ToList();

                    if (intersections.Count <= 0)
                        continue;

                    Dictionary<double, double> dictBreakPoint = new Dictionary<double, double>();
                    XYZ vector = pipeLine.GetEndPoint(1).Subtract(origin).Normalize();

                    List<Pipe> CreatedPipe = new List<Pipe>();
                    CreatedPipe.Add(curpipe);

                    foreach (var intersect in intersections)
                    {
                        if (
                            dictBreakPoint.ContainsKey(intersect.Proximity)
                            || Math.Round(intersect.Proximity, 6) == 0
                        )
                            continue;

                        XYZ breakpoint = origin.Add(vector.Multiply(intersect.Proximity));

                        foreach (Pipe nPipe in CreatedPipe)
                        {
                            IntersectionResult res = (
                                (Line)((LocationCurve)nPipe.Location).Curve
                            ).Project(breakpoint);
                            if (Math.Round(res.Distance, 6) != 0)
                                continue;
                            curpipe = nPipe;
                            break;
                        }

                        ElementId newPipeId = PlumbingUtils.BreakCurve(doc, curpipe.Id, breakpoint);
                        Pipe newPipe = doc.GetElement(newPipeId) as Pipe;

                        CreatedPipe.Add(newPipe);
                        dictBreakPoint.Add(intersect.Proximity, intersect.Proximity);

                        FamilyInstance newFittingIns = doc.Create.NewFamilyInstance(
                            breakpoint,
                            fitting as FamilySymbol,
                            vector,
                            null,
                            StructuralType.NonStructural
                        );

                        Parameter param = newFittingIns.LookupParameter("Номинальный радиус");
                        param.Set(curpipe.Diameter * 0.5);

                        Connector ConCurPipe = curpipe.ConnectorManager.Lookup(0);
                        Connector ConNewPipe = newPipe.ConnectorManager.Lookup(1);

                        MEPModel newFitting = newFittingIns.MEPModel;
                        Connector newFittingCon0 = newFitting.ConnectorManager.Lookup(1);
                        Connector newFittingCon1 = newFitting.ConnectorManager.Lookup(2);

                        ConCurPipe.ConnectTo(newFittingCon1);
                        ConNewPipe.ConnectTo(newFittingCon0);
                    }
                }

                tr.Commit();
            }
            return Result.Succeeded;
        }
    }
}
