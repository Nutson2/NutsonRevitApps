using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Nice3point.Revit.Extensions;
using static System.Math;

namespace TagGadgets
{
    internal static class TagHelpers
    {
        public static void ModifyTag(UIDocument docUI, IndependentTag tag, XYZ TagHead)
        {
            var weightTag = GetTagWeight(docUI.ActiveView, tag);
            tag.HasLeader = true;
            tag.LeaderEndCondition = LeaderEndCondition.Free;

            //Точка на объекте
            tag.LeaderEnd = GetPointOnHostElement(docUI.Document, tag);

            //Определение размещения ножки относительно текста
            var transform = docUI.ActiveView.CropBox.Transform;
            var onViewTagHeadPos = transform.Inverse.OfPoint(TagHead);
            var onViewTagLeaderEndPos = transform.Inverse.OfPoint(tag.LeaderEnd);

            var sign = onViewTagHeadPos.X > onViewTagLeaderEndPos.X ? LegSide.Left : LegSide.Right;

            //Задание смещения что бы выноска выходила сбоку текста
            var offset = UnitExtensions.FromMillimeters(0.001);

            tag.TagHeadPosition = TagHead;
            var offs = (int)sign * (offset + (weightTag * 0.5));
            //var offs          = sign == -1 ? offset : offset + weightTag ;
            var newLeaderElbow = new XYZ(
                onViewTagHeadPos.X + offs,
                onViewTagHeadPos.Y,
                onViewTagHeadPos.Z
            );
            tag.LeaderElbow = transform.OfPoint(newLeaderElbow);
        }

        private static double GetTagWeight(View view, IndependentTag tag)
        {
            var boundingBoxXyz = tag.get_BoundingBox(view);
            var pointA1 = boundingBoxXyz.Max;
            var pointB1 = boundingBoxXyz.Min;
            var byNormalAndOrigin = Plane.CreateByNormalAndOrigin(view.ViewDirection, view.Origin);
            var num = ProjectedDistance(byNormalAndOrigin, pointA1, pointB1);
            var pointA2 = new XYZ(pointA1.X, pointB1.Y, pointA1.Z);
            var pointB2 = new XYZ(pointB1.X, pointA1.Y, pointB1.Z);
            var num2 = ProjectedDistance(byNormalAndOrigin, pointA2, pointB2);
            if (num2 > num)
                (pointA1, pointB1) = (pointA2, pointB2);
            var bbRectangle = new Rectangle(pointA1, pointB1);

            return bbRectangle.Weight;
        }

        private class Rectangle
        {
            public XYZ UpLeft;
            public XYZ UpRight;
            public XYZ DownLeft;
            public XYZ DownRight;
            public XYZ Center;
            public double Weight
            {
                get { return Abs(UpLeft.X - UpRight.X); }
            }

            public Rectangle(XYZ xyz1, XYZ xyz2)
            {
                UpLeft = new XYZ(Min(xyz2.X, xyz1.X), Max(xyz1.Y, xyz2.Y), 0.0);
                UpRight = new XYZ(Max(xyz2.X, xyz1.X), Max(xyz1.Y, xyz2.Y), 0.0);
                DownLeft = new XYZ(Min(xyz2.X, xyz1.X), Min(xyz1.Y, xyz2.Y), 0.0);
                DownRight = new XYZ(Max(xyz2.X, xyz1.X), Min(xyz1.Y, xyz2.Y), 0.0);
                Center = (UpRight + DownLeft) / 2.0;
            }
        }

        public static XYZ GetPointOnHostElement(Document doc, IndependentTag tag)
        {
            Element TaggedElement = doc.GetElement(tag.TaggedElementId.HostElementId);
            return GetPointOnHostElement(doc, TaggedElement);
        }

        public static XYZ GetPointOnHostElement(Document doc, Element TaggedElement)
        {
            XYZ pointOnHost;

            var taggedElementBB = TaggedElement.get_BoundingBox(doc.ActiveView);
            pointOnHost = taggedElementBB.Min + (taggedElementBB.Max - taggedElementBB.Min) / 2;

            return pointOnHost;
        }

        private static double ProjectedDistance(Plane plane, XYZ pointA, XYZ pointB) =>
            ProjectionOnPlane(pointA, plane).DistanceTo(ProjectionOnPlane(pointB, plane));

        private static XYZ ProjectionOnPlane(XYZ q, Plane plane)
        {
            XYZ origin = plane.Origin;
            XYZ xyz = plane.Normal.Normalize();
            return q - xyz.Multiply(xyz.DotProduct(q - origin));
        }
    }
}
