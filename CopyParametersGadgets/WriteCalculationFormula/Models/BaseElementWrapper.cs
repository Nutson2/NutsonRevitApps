using Autodesk.Revit.DB;

namespace mmOrderMarking.Models
{
    internal abstract class BaseElementWrapper
  {
    protected BaseElementWrapper(ElementId id, string uniqueId, Document document)
    {
      Id = id;
      UniqueId = uniqueId;
      Document = document;
    }

    public ElementId Id { get; }

    public string UniqueId { get; }

    public Document Document { get; }

    public double NumberingValue { get; set; }

    public override int GetHashCode() => this.UniqueId.GetHashCode();

    public override bool Equals(object obj) => this.Equals(obj as BaseElementWrapper);

    public bool Equals(BaseElementWrapper obj) => obj != null && obj.UniqueId == this.UniqueId;
  }
}
