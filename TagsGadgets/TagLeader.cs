using System;
using Autodesk.Revit.DB;

namespace TagGadgets
{
    internal class TagLeader
    {
        private readonly Document _doc;
        private readonly View _currentView;
        private readonly Element _taggedElement;
        private readonly IndependentTag _tag;
        private readonly XYZ _tagHeadPosition;
        private XYZ _headOffset;
        private XYZ _tagCenter;
        private Line _endLine;
        private Line _baseLine;
        private readonly LegSide _side;
        private XYZ _elbowPosition;
        private readonly XYZ _leaderEnd;
        private double _tagHeight;
        private double _tagWidth;

        public TagLeader(IndependentTag tag, Document doc)
        {
            _doc = doc;
            _currentView = _doc.GetElement(tag.OwnerViewId) as View;

            _tag = tag;
            _taggedElement = GetTaggedElement(_doc, _tag);

            _tagHeadPosition = _currentView.CropBox.Transform.Inverse.OfPoint(tag.TagHeadPosition);
            _tagHeadPosition = new XYZ(_tagHeadPosition.X, _tagHeadPosition.Y, 0.0);
            _leaderEnd = GetLeaderEnd(_taggedElement, _currentView);
            _side =
                ((_currentView.CropBox.Max + _currentView.CropBox.Min) / 2.0).X <= _leaderEnd.X
                    ? LegSide.Right
                    : LegSide.Left;
            GetTagDimension();
        }

        public XYZ TagCenter
        {
            get => _tagCenter;
            set
            {
                _tagCenter = value;
                UpdateLeaderPosition();
            }
        }

        public Line EndLine => _endLine;

        public Line BaseLine => _baseLine;

        public LegSide Side => _side;

        public XYZ ElbowPosition => _elbowPosition;

        public XYZ LeaderEnd => _leaderEnd;

        public double TagHeight => _tagHeight;

        public double TagWidth => _tagWidth;

        private void UpdateLeaderPosition()
        {
            XYZ xyz = _leaderEnd - _tagCenter;
            double num1 = xyz.X * xyz.Y;
            double num2 = num1 / Math.Abs(num1);
            _elbowPosition =
                _tagCenter + new XYZ(xyz.X - xyz.Y * Math.Tan(num2 * Math.PI / 4.0), 0.0, 0.0);

            _endLine =
                _leaderEnd.DistanceTo(_elbowPosition) <= _doc.Application.ShortCurveTolerance
                    ? Line.CreateBound(new XYZ(0.0, 0.0, 0.0), new XYZ(0.0, 0.0, 1.0))
                    : Line.CreateBound(_leaderEnd, _elbowPosition);

            if (_elbowPosition.DistanceTo(_tagCenter) > _doc.Application.ShortCurveTolerance)
                _baseLine = Line.CreateBound(_elbowPosition, _tagCenter);
            else
                _baseLine = Line.CreateBound(new XYZ(0.0, 0.0, 0.0), new XYZ(0.0, 0.0, 1.0));
        }

        private void GetTagDimension()
        {
            BoundingBoxXYZ boundingBoxXyz = _tag.get_BoundingBox(_currentView);
            BoundingBoxXYZ cropBox = _currentView.CropBox;

            _tagHeight =
                cropBox.Transform.Inverse.OfPoint(boundingBoxXyz.Max).Y
                - cropBox.Transform.Inverse.OfPoint(boundingBoxXyz.Min).Y;

            _tagWidth =
                cropBox.Transform.Inverse.OfPoint(boundingBoxXyz.Max).X
                - cropBox.Transform.Inverse.OfPoint(boundingBoxXyz.Min).X;

            _tagCenter =
                (
                    cropBox.Transform.Inverse.OfPoint(boundingBoxXyz.Max)
                    + cropBox.Transform.Inverse.OfPoint(boundingBoxXyz.Min)
                ) / 2.0;

            _tagCenter = new XYZ(_tagCenter.X, _tagCenter.Y, 0.0);
            _headOffset = _tagHeadPosition - _tagCenter;
        }

        public static Element GetTaggedElement(Document doc, IndependentTag tag) =>
            tag.TaggedElementId.HostElementId != ElementId.InvalidElementId
                ? doc.GetElement(tag.TaggedElementId.HostElementId)
                : (doc.GetElement(tag.TaggedElementId.LinkInstanceId) as RevitLinkInstance)
                    .GetLinkDocument()
                    .GetElement(tag.TaggedElementId.LinkedElementId);

        public static XYZ GetLeaderEnd(Element taggedElement, View currentView)
        {
            var boundingBoxXyz = taggedElement.get_BoundingBox(currentView);
            var cropBox = currentView.CropBox;
            var xyz2 =
                boundingBoxXyz == null
                    ? (cropBox.Max + cropBox.Min) / 2.0 + new XYZ(0.001, 0.0, 0.0)
                    : (boundingBoxXyz.Max + boundingBoxXyz.Min) / 2.0;

            var xyz3 = cropBox.Transform.Inverse.OfPoint(xyz2);
            return new XYZ(Math.Round(xyz3.X, 4), Math.Round(xyz3.Y, 4), 0.0);
        }

        public void UpdateTagPosition()
        {
            _tag.HasLeader = true;
            _tag.LeaderEndCondition = LeaderEndCondition.Free;
            UpdateLeaderPosition();
            var xyz2 =
                _side != LegSide.Left
                    ? new XYZ(Math.Abs(_tagWidth) * 0.5 + 0.1, 0.0, 0.0)
                    : new XYZ(-Math.Abs(_tagWidth) * 0.5 - 0.1, 0.0, 0.0);
            //_tag.TagHeadPosition  = _currentView.CropBox.Transform.OfPoint(_headOffset+ _tagCenter+ xyz2);
            _tag.LeaderElbow = _currentView.CropBox.Transform.OfPoint(_elbowPosition);
        }
    }
}
