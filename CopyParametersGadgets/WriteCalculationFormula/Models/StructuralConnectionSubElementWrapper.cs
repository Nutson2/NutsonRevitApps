
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace mmOrderMarking.Models
{
  internal class StructuralConnectionSubElementWrapper : BaseElementWrapper
  {
    private readonly Subelement _subelement;

    public StructuralConnectionSubElementWrapper(
      Subelement subelement,
      StructuralConnectionHandler structuralConnectionHandler)
      : base(structuralConnectionHandler.Id, subelement.UniqueId, structuralConnectionHandler.Document)
    {
      this._subelement = subelement;
      this.ParameterElements = new List<ParameterElement>();
      foreach (ElementId allParameter in (IEnumerable<ElementId>) subelement.GetAllParameters())
      {
        if (this.Document.GetElement(allParameter) is ParameterElement element)
          this.ParameterElements.Add(element);
      }
    }

    public List<ParameterElement> ParameterElements { get; }

    public string GetStringParameterValue(ElementId parameterId) => (this._subelement.GetParameterValue(parameterId) is StringParameterValue parameterValue ? parameterValue.Value : (string) null) ?? string.Empty;

    public string GetParameterStringValue(ExtParameter extParameter)
    {
      if (extParameter == null)
        return string.Empty;
      switch (this._subelement.GetParameterValue(extParameter.Parameter.Id))
      {
        case StringParameterValue stringParameterValue:
          return stringParameterValue.Value;
        case DoubleParameterValue doubleParameterValue:
          return doubleParameterValue.Value.ToString((IFormatProvider) CultureInfo.CurrentCulture);
        case IntegerParameterValue integerParameterValue:
          return integerParameterValue.Value.ToString();
        case ElementIdParameterValue idParameterValue:
          return this.Document.GetElement(idParameterValue.Value)?.Name ?? string.Empty;
        default:
          return string.Empty;
      }
    }

    public void SetParameterValue(ElementId parameterId, string value) => this._subelement.SetParameterValue(parameterId, (ParameterValue) new StringParameterValue(value));

    public void SetParameterValue(ElementId parameterId, double value) => this._subelement.SetParameterValue(parameterId, (ParameterValue) new DoubleParameterValue(value));

    public void SetParameterValue(ElementId parameterId, int value) => this._subelement.SetParameterValue(parameterId, (ParameterValue) new IntegerParameterValue(value));
  }
}
