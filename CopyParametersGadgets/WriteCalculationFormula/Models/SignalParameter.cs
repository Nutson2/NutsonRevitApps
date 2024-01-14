using Autodesk.Revit.DB;
using mmOrderMarking.Models;
using System.Collections.Generic;

namespace mmOrderMarking.Services
{
    public partial class NumerateService
    {
        private class SignalParameter
        {
            public readonly string Pattern = "MODPLUS_SEPARATOR_START(.*?)MODPLUS_SEPARATOR_END";
            private readonly BuiltInParameter? _builtInParameter;
            private readonly ExtParameter _extParameter;
            private readonly List<BuiltInParameter> _allowableBuiltInParameter = new List<BuiltInParameter>()
                {
                BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS,
                BuiltInParameter.SHEET_NAME,
                BuiltInParameter.VIEW_NAME,
                BuiltInParameter.DATUM_TEXT
                };

            public SignalParameter(List<BaseElementWrapper> elements, ExtParameter extParameter, ScheduleDefinition definition)
            {
                if (!extParameter.IsNumeric && !IsParameterUsedForSorting(definition, extParameter.Parameter.Id))
                    _extParameter = extParameter;
                else
                    _builtInParameter = GetBuiltInParameterForElements(elements, definition);

                if (_extParameter == null && !_builtInParameter.HasValue)
                {
                    if (!extParameter.IsNumeric)
                        _extParameter = extParameter;
                    else
                        _builtInParameter = GetBuiltInParameterForElements(elements, null);
                }
                Set(elements);
            }

            public int GetParameterId() => _builtInParameter.HasValue ? (int)_builtInParameter.Value : _extParameter.Parameter.Id.IntegerValue;

            private void Set(List<BaseElementWrapper> elements)
            {
                foreach (var curElement in elements)
                {
                    if (curElement is ElementWrapper elementWrapper)
                    {
                        if (elementWrapper.GroupId != ElementId.InvalidElementId &&
                            elementWrapper.Document.GetElement(elementWrapper.GroupId) is Group group)
                            group.UngroupMembers();

                        var parameter = _builtInParameter.HasValue ?
                                elementWrapper.Element.get_Parameter(_builtInParameter.Value) :
                                _extParameter.GetSameParameter(elementWrapper.Element);
                        try
                        {
                            parameter.Set(parameter.AsString() + "MODPLUS_SEPARATOR_START" + elementWrapper.UniqueId + "MODPLUS_SEPARATOR_END");
                        }
                        catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
                        {
                            throw ex;
                        }
                    }
                    else if (curElement is StructuralConnectionSubElementWrapper subElementWrapper)
                        subElementWrapper.SetParameterValue(
                            _extParameter.Parameter.Id, subElementWrapper.GetStringParameterValue(_extParameter.Parameter.Id)
                            + "MODPLUS_SEPARATOR_START"
                            + subElementWrapper.UniqueId
                            + "MODPLUS_SEPARATOR_END");
                }
            }

            private BuiltInParameter? GetBuiltInParameterForElements(IReadOnlyCollection<BaseElementWrapper> elements, ScheduleDefinition definition)
            {
                BuiltInParameter? parameterForElements = new BuiltInParameter?();
                foreach (var curBuiltInParameter in _allowableBuiltInParameter)
                {
                    foreach (var element in elements)
                    {
                        ElementId parameterId = null;
                        switch (element)
                        {
                            case ElementWrapper elementWrapper:
                                parameterId = elementWrapper.Element.get_Parameter(curBuiltInParameter)?.Id;
                                break;
                            case StructuralConnectionSubElementWrapper subElementWrapper:
                                foreach (var current in subElementWrapper.ParameterElements)
                                {
                                    var definition1 = current.GetDefinition();
                                    if (definition1 != null && definition1.BuiltInParameter == curBuiltInParameter)
                                        parameterId = current.Id;
                                }
                                break;
                        }

                        if (parameterId == null || parameterId == ElementId.InvalidElementId)
                        {
                            parameterForElements = new BuiltInParameter?();
                            break;
                        }
                        if (definition != null && IsParameterUsedForSorting(definition, parameterId))
                        {
                            parameterForElements = new BuiltInParameter?();
                            break;
                        }
                        parameterForElements = new BuiltInParameter?(curBuiltInParameter);
                    }
                    if (parameterForElements.HasValue)
                        break;
                }
                return parameterForElements;
            }

            private bool IsParameterUsedForSorting(ScheduleDefinition definition, ElementId parameterId)
            {
                foreach (var sortGroupField in definition.GetSortGroupFields())
                {
                    if (definition.GetField(sortGroupField.FieldId).ParameterId == parameterId)
                        return true;
                }
                return false;
            }
        }
    }
}
