using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TagGadgets
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class EditTag : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;
            UIDocument docUI = revit.Application.ActiveUIDocument;

            IList<Reference> selRefs = null;
            try
            {
                selRefs = docUI.Selection.PickObjects(
                    ObjectType.Element,
                    new IndependentTagFilter(),
                    "Выбери элемент с коннекторами"
                );
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                    return Result.Cancelled;
            }
            if (selRefs == null)
                return Result.Cancelled;

            var tagsHeadPosition = new Dictionary<IndependentTag, XYZ>();

            using (Transaction tr = new Transaction(docUI.Document, "pre-modify tags"))
            {
                tr.Start();
                foreach (Reference selRef in selRefs)
                {
                    if (!(docUI.Document.GetElement(selRef.ElementId) is IndependentTag elem))
                        continue;
                    elem.HasLeader = false;
                    tagsHeadPosition.Add(elem, elem.TagHeadPosition);
                    var displacementGroupId = DisplacementElement.GetDisplacementElementId(
                        doc.ActiveView,
                        elem.TaggedElementId.HostElementId
                    );

                    if (displacementGroupId != ElementId.InvalidElementId)
                    {
                        var displacementGroup =
                            doc.GetElement(displacementGroupId) as DisplacementElement;
                        var groupOffset = GetSummaryOffset(displacementGroup);
                        elem.TagHeadPosition =
                            TagHelpers.GetPointOnHostElement(doc, elem) - groupOffset;
                    }
                    else
                        elem.TagHeadPosition = TagHelpers.GetPointOnHostElement(doc, elem);
                }
                tr.Commit();
            }
            using (Transaction tr = new Transaction(docUI.Document, "modify tags"))
            {
                tr.Start();
                foreach (var tag in tagsHeadPosition)
                {
                    TagHelpers.ModifyTag(docUI, tag.Key, tag.Value);
                }
                tr.Commit();
            }
            return Result.Succeeded;
        }

        private XYZ GetSummaryOffset(DisplacementElement displacementElement)
        {
            var xYZ = displacementElement.GetRelativeDisplacement();
            if (displacementElement.ParentId != ElementId.InvalidElementId)
                xYZ += GetSummaryOffset(
                    displacementElement.Document.GetElement(displacementElement.ParentId)
                        as DisplacementElement
                );
            return xYZ;
        }

        public class IndependentTagFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is IndependentTag)
                    return true;
                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
