using Autodesk.Revit.DB;

namespace mmOrderMarking.Models
{
    internal class ElementWrapper : BaseElementWrapper
    {
        public ElementWrapper(Element element)
          : base(element.Id, element.UniqueId, element.Document)
        {
            this.Element = element;
            this.GroupId = element.GroupId;
        }

        public Element Element { get; }

        public ElementId GroupId { get; }

        public string GetParameterStringValue(ExtParameter extParameter)
        {
            Parameter sameParameter = extParameter?.GetSameParameter(this.Element);

            if (sameParameter == null) return string.Empty;

            if (sameParameter.StorageType == StorageType.String)
                return sameParameter.AsString() ?? string.Empty;

            return sameParameter.StorageType == StorageType.None ? string.Empty : sameParameter.AsValueString();
        }
    }
}
