using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Nice3point.Revit.Extensions;

namespace TagGadgets
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class CreateTags : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            Document doc = revit.Application.ActiveUIDocument.Document;
            UIDocument uiDoc = revit.Application.ActiveUIDocument;

            var selIDs = uiDoc.Selection.GetElementIds();
            if (selIDs == null || selIDs.Count == 0)
                return Result.Cancelled;

            var point = uiDoc.Selection.PickPoint();
            var tags = new List<IndependentTag>();
            var offset = UnitExtensions.FromMillimeters(0);

            using (Transaction tr = new Transaction(doc))
            {
                tr.Start("Создание марок");
                foreach (var selID in selIDs)
                {
                    var el = doc.GetElement(selID);
                    var refEl = new Reference(el);

                    var endPoint = TagHelpers.GetPointOnHostElement(
                        doc,
                        doc.GetElement(refEl.ElementId)
                    );
                    var newTag = CreateIndependentTag(doc, refEl, point, endPoint);
                    tags.Add(newTag);
                }
                var sortedTags = tags.OrderBy(x => x.LeaderEnd.X);
                foreach (var tag in sortedTags)
                {
                    tag.TagHeadPosition = new XYZ(point.X + offset, point.Y, point.Z);
                    offset += offset;
                }
                tr.Commit();
            }
            return Result.Succeeded;
        }

        private IndependentTag CreateIndependentTag(
            Document document,
            Reference reference,
            XYZ pastePoint,
            XYZ endPoint
        )
        {
            var view = document.ActiveView;
            var tagMode = TagMode.TM_ADDBY_CATEGORY;
            var tagOrientation = TagOrientation.Horizontal;

            IndependentTag newTag = IndependentTag.Create(
                document,
                view.Id,
                reference,
                true,
                tagMode,
                tagOrientation,
                endPoint
            );

            if (null == newTag)
                throw new Exception("Create IndependentTag Failed.");

            newTag.LeaderEndCondition = LeaderEndCondition.Free;
            newTag.TagHeadPosition = pastePoint;
            newTag.LeaderEnd = endPoint;

            return newTag;
        }
    }
}
