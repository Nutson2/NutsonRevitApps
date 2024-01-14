using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using CopyParametersGadgets;
using CopyParametersGadgets.Model;
using CopyParametersGadgets.WriteCalculation.Model;
using mmOrderMarking.Enums;
using mmOrderMarking.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NRPUtils.Extentions;

namespace mmOrderMarking.Services
{
    public partial class NumerateService
    {
        private readonly UIApplication _uiApplication;
        private readonly Document _doc;

        public NumerateService(UIApplication uiApplication)
        {
            _uiApplication = uiApplication;
            _doc           = _uiApplication.ActiveUIDocument.Document;
        }
        public void CalculateValue(Dictionary<string, CalculationModel[]> dict)
        {
            if (!(_doc.ActiveView is ViewSchedule viewSchedule)) return;
            var el= new FilteredElementCollector(_doc, _uiApplication.ActiveUIDocument.ActiveGraphicalView.Id)
                                                                .WhereElementIsNotElementType()
                                                                .Where(e => e.IsValidObject && !(e is RevitLinkInstance)).FirstOrDefault();
            if (el == null) return;
            var param = el.GetParameters("О_Формула расчета").FirstOrDefault();
            if (param == null) return;
            var extParam = new ExtParameter(param) { IsInstanceParameter=true };

            var wrapElements         = GetWrappedElements(viewSchedule);
            var sortedElementsByRows = GetGroupedElementsFromSchedule(extParam, viewSchedule, wrapElements);

            using (var tr=new Transaction(_doc, "Заполнение параметра формула расчета"))
            {
                tr.Start();
                foreach (var rowData in sortedElementsByRows)
                {
                    var curElId = wrapElements.FirstOrDefault(e => e.UniqueId== rowData.Items.First());
                    if (curElId==null) continue;
                    if (!(curElId is ElementWrapper wrapedElement))  continue;
                    var categoryID=wrapedElement.Element.Category.Name;

                    if(dict.ContainsKey(categoryID))
                    {
                        var pair=dict[categoryID];
                        CalculateParamValueForPipe(rowData, wrapElements, pair);
                    }

                }
                tr.Commit();
            }
        }

        private void CalculateParamValueForPipe(RowData rowData, List<BaseElementWrapper> wrapElements, CalculationModel[] calculationModel)
        {
            var elements   = new List<Element>();
            var prefix     = string.Empty;
            var resString  = new StringBuilder[calculationModel.Length];
            
            foreach (var uniqId in rowData.Items)
            {
                if (!(wrapElements.FirstOrDefault(x => x.UniqueId==uniqId) is ElementWrapper wrapEl)) continue;
                for (int i = 0; i < calculationModel.Length; i++)
                {
                    var param=wrapEl.Element.LookupParameter(calculationModel[i].ParameterForSumming);
                    if (param == null) continue;
                    var paramValue = Math.Round(param.AsDouble(), 2);   
                    prefix=elements.Count==0 ? "" : "+";
                    
                    var valueString=string.Empty;
                    if (param.StorageType== StorageType.String)
                        valueString= param.AsString();
                    else
                        valueString= param.AsValueString()?.Split(' ')[0];

                    prefix=elements.Count==0 ? "" : " + ";
                    
                    if(elements.Count == 0 ) resString[i]=new StringBuilder(0);
                    resString[i].Append(prefix + valueString);
                }
                elements.Add(wrapEl.Element);
            }
            var res=resString.Select(x=>x.ToString()).ToArray();
            
            elements.ForEach(x => 
            {
                for (int i = 0; i < calculationModel.Length; i++)
                {
                    x.LookupParameter(calculationModel[i].ParameterForWrite).TrySetValue(res[i]);
                }
            });
        }

        public void NumerateInSchedule(InScheduleNumerateData numerateData)
        {
            if (!(_doc.ActiveView is ViewSchedule activeView)) return;

            List<BaseElementWrapper> list = CollectElementsInSchedule(numerateData, activeView).ToList();
            if (!list.Any()) return;

            ProceedNumeration(numerateData, list);
        }

        //public void NumerateInView(ElementsSelectionType selectionType, InViewNumerateData numerateData, List<Element> elements)
        //{
        //    OrderDirection orderDirection;
        //    List<ElementWrapper> elements1;

        //    if (selectionType == ElementsSelectionType.ByRectangle)
        //    {
        //        orderDirection = OrderDirection.Ascending;
        //        elements1 = numerateData.LocationOrder == LocationOrder.Creation ?
        //                            elements.Select(e => new ElementWrapper(e)).ToList() :
        //                            GetElementsSortedByLocation(elements, numerateData.LocationOrder);
        //    }
        //    else
        //    {
        //        orderDirection = numerateData.OrderDirection;
        //        elements1 = elements.Select(e => new ElementWrapper(e)).ToList();
        //    }
        //    for (int index = 0; index < elements1.Count; ++index)
        //        elements1[index].NumberingValue = orderDirection == OrderDirection.Ascending ?
        //                numerateData.StartValueDouble + index * numerateData.Step :
        //                elements1.Count + numerateData.StartValueDouble - index * numerateData.Step - 1.0;
        //    ProceedNumeration(numerateData, elements1);
        //}

        public void ClearInSchedule(ExtParameter extParameter)
        {
            if (extParameter.IsNumeric) return;
            Document document = _uiApplication.ActiveUIDocument.Document;

            if (!(document.ActiveView is ViewSchedule activeView)) return;
            ClearStringParameter(new FilteredElementCollector(document, activeView.Id)
                                    .Where(e => e.IsValidObject && e.LookupParameter(extParameter.Name) != null), extParameter.Name);
        }

        public void ClearInView(ExtParameter extParameter, List<Element> elements) => ClearStringParameter(elements, extParameter.Name);

        private void ClearStringParameter(IEnumerable<Element> listElements, string parameterName)
        {
            Document document = _uiApplication.ActiveUIDocument.Document;
            string name = "mmOrderMarking";

            using (Transaction transaction = new Transaction(document))
            {
                if (transaction.Start(name) == TransactionStatus.Started)
                {
                    foreach (Element listElement in listElements)
                        listElement.LookupParameter(parameterName)?.Set(string.Empty);
                }
                transaction.Commit();
            }
        }

        private void ProceedNumeration(NumerateData numerateData, IEnumerable<BaseElementWrapper> elements)
        {
            Document document = _uiApplication.ActiveUIDocument.Document;
            try
            {

                //_uiApplication.Application.FailuresProcessing += NumerateService.\u003C\u003EO.\u003C0\u003E__ApplicationOnFailuresProcessing ??
                //            (NumerateService.\u003C\u003EO.\u003C0\u003E__ApplicationOnFailuresProcessing = 
                //    new EventHandler<FailuresProcessingEventArgs>(ApplicationOnFailuresProcessing));
                string name = "mmOrderMarking";

                using (Transaction transaction = new Transaction(document))
                {
                    transaction.Start(name);
                    List<ElementId> elementIdList = new List<ElementId>();
                    foreach (BaseElementWrapper baseWrapElement in elements)
                    {
                        try
                        {
                            if (baseWrapElement is ElementWrapper wrapElement)
                            {
                                ElementId elementId1;
                                if (!wrapElement.Element.CanHaveTypeAssigned())
                                    elementId1 = ElementId.InvalidElementId;
                                else
                                    elementId1 = wrapElement.Element.GetTypeId()?? ElementId.InvalidElementId;

                                if (elementId1 == ElementId.InvalidElementId && !elementIdList.Contains(elementId1))  continue; 

                                Parameter sameParameter = numerateData.Parameter.GetSameParameter(wrapElement.Element);
                                if (sameParameter == null)  continue; 

                                if (!numerateData.Parameter.IsInstanceParameter && elementId1 != ElementId.InvalidElementId)
                                    elementIdList.Add(elementId1);

                                if (!sameParameter.IsReadOnly)
                                    SetParameterValue(wrapElement, sameParameter, numerateData);
                            }

                            else if (baseWrapElement is StructuralConnectionSubElementWrapper element2)
                                    SetParameterValue(element2, numerateData);
                        }
                        catch (Exception) { }
                    }
                    transaction.Commit();
                }
                //this._uiApplication.Application.FailuresProcessing -= NumerateService.\u003C\u003EO.\u003C0\u003E__ApplicationOnFailuresProcessing ??
                //    (NumerateService.\u003C\u003EO.\u003C0\u003E__ApplicationOnFailuresProcessing = new EventHandler<FailuresProcessingEventArgs>(ApplicationOnFailuresProcessing));
            }
            catch (Exception) { } 
        }

        private IEnumerable<BaseElementWrapper> CollectElementsInSchedule(InScheduleNumerateData numerateData, ViewSchedule viewSchedule)
        {
            List<BaseElementWrapper> wrapElements = GetWrappedElements( viewSchedule);

            CheckWrapedElements(numerateData, wrapElements);

            if (viewSchedule.Definition.IsItemized)
            {
                #region Работа с элементами в случае когда спецификация для каждого элемента
                List<BaseElementWrapper> sortElements;
                using (Transaction transaction = new Transaction(this._doc))
                {
                    transaction.Start("Find in itemized table");
                    sortElements = GetSortedElementsFromItemizedSchedule(viewSchedule, wrapElements, numerateData.Parameter);
                    transaction.RollBack();
                }
                for (int i = 0; i < sortElements.Count; ++i)
                {
                    var wrapElement = sortElements[i];
                    wrapElement.NumberingValue = numerateData.OrderDirection == OrderDirection.Ascending ?
                                                    numerateData.StartValueDouble + i * numerateData.Step :
                                                    sortElements.Count + numerateData.StartValueDouble - i * numerateData.Step - 2.0;
                    yield return wrapElement;
                    wrapElement = null;
                }
                sortElements = null;
                #endregion
            }
            else
            {
                #region Работа с элементами в случае когда спецификация сгруппирована
                List<RowData> sortedElementsByRows = GetGroupedElementsFromSchedule(numerateData.Parameter, viewSchedule, wrapElements);

                if (sortedElementsByRows.Any())
                {
                    for (int i = 0; i < sortedElementsByRows.Count; ++i)
                    {
                        double numberingValue = numerateData.OrderDirection == OrderDirection.Ascending ?
                                                    numerateData.StartValueDouble + i * numerateData.Step :
                                                    sortedElementsByRows.Count + numerateData.StartValueDouble - i * numerateData.Step - 1.0;

                        foreach (string str in sortedElementsByRows[i].Items)
                        {
                            var uniqueId = str;
                            var wrapedElement = wrapElements.First(e => e.UniqueId == uniqueId);
                            wrapedElement.NumberingValue = numberingValue;

                            yield return wrapedElement;
                            wrapedElement = null;
                        }
                    }
                }
                sortedElementsByRows = null;
                #endregion
            }
        }

        private static void CheckWrapedElements(InScheduleNumerateData numerateData, List<BaseElementWrapper> wrapElements)
        {
            #region Очистка списка от невалидных объектов

            for (int i = wrapElements.Count - 1; i >= 0; --i)
            {
                ElementWrapper wrapElement = wrapElements[i] as ElementWrapper;

                if (wrapElement != null && numerateData.Parameter.GetSameParameter(wrapElement.Element) == null)
                {
                    wrapElements.RemoveAt(i);
                }
                else
                {
                    StructuralConnectionSubElementWrapper wrapSubElement = wrapElements[i] as StructuralConnectionSubElementWrapper;
                    if (wrapSubElement != null && wrapSubElement.ParameterElements.All(p => p.Id != numerateData.Parameter.Parameter.Id))
                        wrapElements.RemoveAt(i);
                    wrapSubElement = null;
                }
                wrapElement = null;
            }

            #endregion
        }

        private List<BaseElementWrapper> GetWrappedElements( ViewSchedule viewSchedule)
        {
            IEnumerable<Element> collector =new FilteredElementCollector(_doc, viewSchedule.Id).WhereElementIsNotElementType();
            List<BaseElementWrapper> wrapElements = new List<BaseElementWrapper>();

            foreach (Element element in collector)
            {
                StructuralConnectionHandler structuralConnectionHandler = element as StructuralConnectionHandler;
                if (structuralConnectionHandler != null)
                    wrapElements.AddRange(structuralConnectionHandler
                                                .GetSubelements()
                                                .Select(subelement => new StructuralConnectionSubElementWrapper(subelement, structuralConnectionHandler)));
                else
                    wrapElements.Add(new ElementWrapper(element));
            }
            return wrapElements;
        }

        private List<RowData> GetGroupedElementsFromSchedule(ExtParameter extParameter, ViewSchedule viewSchedule, List<BaseElementWrapper> wrapElements)
        {
            List<RowData> sortedElementsByRows;
            using (Transaction transaction = new Transaction(_doc))
            {
                transaction.Start("Find in rows");
                sortedElementsByRows = GetSortedElementsFromNotItemizedSchedule(viewSchedule, wrapElements, extParameter)
                                                        .Where(e => e.Items.Count > 0)
                                                        .ToList();
                transaction.RollBack();
            }

            return sortedElementsByRows;
        }

        private static void SetParameterValue(ElementWrapper element, Parameter parameter, NumerateData numerateData)
        {
            if (element.GroupId != ElementId.InvalidElementId &&
                parameter.Definition is InternalDefinition definition &&
                numerateData.Parameter.IsInstanceParameter &&
                !definition.VariesAcrossGroups &&
                definition.BuiltInParameter != BuiltInParameter.DOOR_NUMBER)
                throw new Exception();
            try
            {
                if (numerateData.Parameter.IsNumeric)
                {
                    if (parameter.StorageType == StorageType.Integer)
                    {
                        parameter.Set((int)element.NumberingValue);
                    }
                    else
                    {
                        if (parameter.StorageType != StorageType.Double)
                            return;
                        parameter.SetValueString(element.NumberingValue.ToString(CultureInfo.CurrentCulture));
                    }
                }
                else
                {
                    string prefix;
                    string suffix;
                    if (numerateData.PrefixSuffixSource == PrefixSuffixSource.String)
                    {
                        prefix = numerateData.Prefix;
                        suffix = numerateData.Suffix;
                    }
                    else
                    {
                        prefix = numerateData.PrefixParameter.Parameter != null ?
                                    element.GetParameterStringValue(numerateData.PrefixParameter) + numerateData.PrefixParameterDelimiter :
                                    string.Empty;

                        suffix = numerateData.SuffixParameter.Parameter != null ?
                                    numerateData.SuffixParameterDelimiter + element.GetParameterStringValue(numerateData.SuffixParameter) :
                                    string.Empty;
                    }
                    string numberValue = string.Format(numerateData.Format, element.NumberingValue)
                                                        .Replace(".", numerateData.DecimalSeparator)
                                                        .Replace(",", numerateData.DecimalSeparator);
                    parameter.Set(prefix + numberValue + suffix);
                }
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        private void SetParameterValue(StructuralConnectionSubElementWrapper element, NumerateData numerateData)
        {
            if (numerateData.Parameter.IsNumeric)
            {
                if (numerateData.Parameter.Parameter.StorageType == StorageType.Integer)
                {
                    element.SetParameterValue(numerateData.Parameter.Parameter.Id, (int)element.NumberingValue);
                }
                else
                {
                    if (numerateData.Parameter.Parameter.StorageType != StorageType.Double)
                        return;
                    element.SetParameterValue(numerateData.Parameter.Parameter.Id, element.NumberingValue);
                }
            }
            else
            {
                string prefix;
                string suffix;
                if (numerateData.PrefixSuffixSource == PrefixSuffixSource.String)
                {
                    prefix = numerateData.Prefix;
                    suffix = numerateData.Suffix;
                }
                else
                {
                    prefix = numerateData.PrefixParameter.Parameter != null ?
                        element.GetParameterStringValue(numerateData.PrefixParameter) + numerateData.PrefixParameterDelimiter :
                        string.Empty;
                    suffix = numerateData.SuffixParameter.Parameter != null ?
                        numerateData.SuffixParameterDelimiter + element.GetParameterStringValue(numerateData.SuffixParameter) :
                        string.Empty;
                }
                element.SetParameterValue(numerateData.Parameter.Parameter.Id, prefix + string.Format(numerateData.Format, element.NumberingValue) + suffix);
            }
        }

        private static void ApplicationOnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            IList<FailureMessageAccessor> failureMessages = e.GetFailuresAccessor().GetFailureMessages();
            if (!failureMessages.Any()) return;

            foreach (var failure in failureMessages)
            {
                if (failure.GetFailureDefinitionId() == BuiltInFailures.GeneralFailures.DuplicateValue)
                    e.GetFailuresAccessor().DeleteWarning(failure);
            }
        }

        private List<BaseElementWrapper> GetSortedElementsFromItemizedSchedule(ViewSchedule viewSchedule, List<BaseElementWrapper> elements, ExtParameter targetParameter)
        {
            var itemizedSchedule = new List<BaseElementWrapper>();
            var signalParameter = new SignalParameter(elements, targetParameter, viewSchedule.Definition);
            
            var schedule = AddFieldToSchedule(viewSchedule, signalParameter);
            FixFilterField(viewSchedule.Definition, schedule);

            var regex = new Regex(signalParameter.Pattern);
            var stringSet = new HashSet<string>();
            var sectionData = viewSchedule.GetTableData().GetSectionData(SectionType.Body);

            for (int firstRowNumber = sectionData.FirstRowNumber; firstRowNumber <= sectionData.LastRowNumber; ++firstRowNumber)
            {
                for (int firstColumnNumber = sectionData.FirstColumnNumber; firstColumnNumber <= sectionData.LastColumnNumber; ++firstColumnNumber)
                {
                    var cellText = viewSchedule.GetCellText(SectionType.Body, firstRowNumber, firstColumnNumber);
                    Match match = regex.Match(cellText);
                    if (match.Success)
                    {
                        string idStr = match.Groups[1].Value;
                        if (stringSet.Add(idStr))
                            itemizedSchedule.Add(elements.First(e => e.UniqueId == idStr));
                    }
                }
            }
            return itemizedSchedule;
        }

        private IEnumerable<RowData> GetSortedElementsFromNotItemizedSchedule(ViewSchedule viewSchedule, List<BaseElementWrapper> elements, ExtParameter targetParameter)
        {
            var GroupElementsFromRow         = new List<RowData>();

            var definition      = viewSchedule.Definition;
            var signalParameter = new SignalParameter(elements, targetParameter, definition);

            definition.IsItemized  = true;
            definition.ShowHeaders = true;

            for (int index = 0; index < definition.GetFieldCount(); ++index)
                definition.GetField(index).IsHidden = false;

            var schedule = AddFieldToSchedule(viewSchedule, signalParameter);
            FixFilterField(definition, schedule);

            var headingAndName = new List<HeadingAndName>();
            foreach (var sortGroupField in definition.GetSortGroupFields())
            {
                var field = definition.GetField(sortGroupField.FieldId);
                field.ColumnHeading = Guid.NewGuid().ToString();
                headingAndName.Add(new HeadingAndName(field.ColumnHeading, field.GetName()));
            }

            var regex             = new Regex(signalParameter.Pattern);
            var uniqId            = new HashSet<string>();
            var sortingColumnIndx = new List<int>();
            var sectionData       = viewSchedule.GetTableData().GetSectionData(SectionType.Body);

            for (int curRowIndx = sectionData.FirstRowNumber; curRowIndx <= sectionData.LastRowNumber; ++curRowIndx)
            {
                var keyValueOfRow = string.Empty;
                var elementUniqID = string.Empty;

                for (int curColumnIndx = sectionData.FirstColumnNumber; curColumnIndx <= sectionData.LastColumnNumber; ++curColumnIndx)
                {
                    var cellValue = viewSchedule.GetCellText(SectionType.Body, curRowIndx, curColumnIndx);
                    
                    if (headingAndName.Any(s => s.IsMatch(cellValue)))
                        sortingColumnIndx.Add(curColumnIndx);
                    else
                    {
                        if (sortingColumnIndx.Contains(curColumnIndx))
                            keyValueOfRow += cellValue;
                        Match match = regex.Match(cellValue);
                        if (match.Success && uniqId.Add(match.Groups[1].Value))
                            elementUniqID = match.Groups[1].Value;
                    }
                }
                if (!string.IsNullOrEmpty(elementUniqID))
                {
                    var rowData = GroupElementsFromRow.FirstOrDefault(rd => rd.RowMatchValue == keyValueOfRow);
                    if (rowData == null)
                        GroupElementsFromRow.Add(new RowData(GroupElementsFromRow.Count + 1, keyValueOfRow) { Items = { elementUniqID } } );
                    else
                        rowData.Items.Add(elementUniqID);
                }
            }
            return GroupElementsFromRow;
        }

        private void FixFilterField(ScheduleDefinition definition, ScheduleFieldId signalFieldId)
        {
            var filters = definition.GetFilters();
            for (int index = 0; index < filters.Count; ++index)
            {
                var filter = filters[index];
                if (filter.FieldId == signalFieldId && filter.IsStringValue)
                {
                    switch (filter.FilterType)
                    {
                        case ScheduleFilterType.Equal:
                            filter.FilterType = ScheduleFilterType.BeginsWith;
                            break;
                        case ScheduleFilterType.NotEqual:
                            filter.FilterType = ScheduleFilterType.NotContains;
                            break;
                    }
                    definition.SetFilter(index, filter);
                }
            }
        }

        private static ScheduleFieldId AddFieldToSchedule(ViewSchedule viewSchedule, SignalParameter signalParameter)
        {
            var schedulableFields = viewSchedule.Definition.GetSchedulableFields();
            var flag = false;
            var parameterId = signalParameter.GetParameterId();
            ScheduleFieldId schedule = null;

            foreach (var schedulableField in schedulableFields)
            {
                if (schedulableField.FieldType != ScheduleFieldType.Instance ||
                    schedulableField.ParameterId.IntegerValue != parameterId)
                    continue;

                foreach (var fieldId in viewSchedule.Definition.GetFieldOrder())
                {
                    try
                    {
                        ScheduleField field = viewSchedule.Definition.GetField(fieldId);
                        if (field.GetSchedulableField() == schedulableField)
                        {
                            flag = true;
                            field.IsHidden = false;
                            schedule = field.FieldId;
                            break;
                        }
                    }
                    catch
                    {
                    }
                }
                if (!flag)
                    schedule = viewSchedule.Definition.AddField(schedulableField).FieldId;
            }
            viewSchedule.Document.Regenerate();
            return schedule;
        }

        private static List<ElementWrapper> GetElementsSortedByLocation(IEnumerable<Element> elements, LocationOrder locationOrder)
        {
            List<ElementWrapper> sortedByLocation = new List<ElementWrapper>();
            Dictionary<ElementWrapper, XYZ> dictionary1 = new Dictionary<ElementWrapper, XYZ>();
            foreach (Element element in elements)
            {
                Curve curve = default;
                int num;
                if (element is Grid grid)
                {
                    curve = grid.Curve;
                    num = curve != null ? 1 : 0;
                }
                else
                    num = 0;
                if (num != 0)
                {
                    dictionary1.Add(new ElementWrapper(element), curve.Evaluate(0.5, true));
                }
                else
                {
                    Location location = element.Location;
                    if (location is LocationPoint locationPoint)
                        dictionary1.Add(new ElementWrapper(element), locationPoint.Point);
                    else if (location is LocationCurve locationCurve)
                        dictionary1.Add(new ElementWrapper(element), locationCurve.Curve.Evaluate(0.5, true));
                }
            }
            bool flag1;
            switch (locationOrder)
            {
                case LocationOrder.LeftToRightUpToDown:
                case LocationOrder.LeftToRightDownToUp:
                case LocationOrder.RightToLeftUpToDown:
                case LocationOrder.RightToLeftDownToUp:
                    flag1 = true;
                    break;
                default:
                    flag1 = false;
                    break;
            }
            if (flag1)
            {
                List<Dictionary<ElementWrapper, XYZ>> source1 = new List<Dictionary<ElementWrapper, XYZ>>();
                foreach (KeyValuePair<ElementWrapper, XYZ> keyValuePair in dictionary1)
                {
                    if (source1.Count == 0)
                    {
                        Dictionary<ElementWrapper, XYZ> dictionary2 = new Dictionary<ElementWrapper, XYZ>()
            {
              {
                keyValuePair.Key,
                keyValuePair.Value
              }
            };
                        source1.Add(dictionary2);
                    }
                    else
                    {
                        bool flag2 = false;
                        foreach (Dictionary<ElementWrapper, XYZ> dictionary3 in source1)
                        {
                            if (!flag2)
                            {
                                foreach (XYZ xyz in dictionary3.Values)
                                {
                                    if (Math.Abs(xyz.Y - keyValuePair.Value.Y) < 0.0001)
                                    {
                                        dictionary3.Add(keyValuePair.Key, keyValuePair.Value);
                                        flag2 = true;
                                        break;
                                    }
                                }
                            }
                            else
                                break;
                        }
                        if (!flag2)
                        {
                            Dictionary<ElementWrapper, XYZ> dictionary4 = new Dictionary<ElementWrapper, XYZ>()
              {
                {
                  keyValuePair.Key,
                  keyValuePair.Value
                }
              };
                            source1.Add(dictionary4);
                        }
                    }
                }
                if (source1.Any<Dictionary<ElementWrapper, XYZ>>())
                {
                    source1.Sort((Comparison<Dictionary<ElementWrapper, XYZ>>)((r1, r2) => r1.Values.First<XYZ>().Y.CompareTo(r2.Values.First<XYZ>().Y)));
                    if (locationOrder == LocationOrder.LeftToRightUpToDown || locationOrder == LocationOrder.RightToLeftUpToDown)
                        source1.Reverse();
                    foreach (Dictionary<ElementWrapper, XYZ> source2 in source1)
                    {
                        bool flag3;
                        switch (locationOrder)
                        {
                            case LocationOrder.LeftToRightUpToDown:
                            case LocationOrder.LeftToRightDownToUp:
                                flag3 = true;
                                break;
                            default:
                                flag3 = false;
                                break;
                        }
                        if (flag3)
                            sortedByLocation.AddRange(source2.OrderBy<KeyValuePair<ElementWrapper, XYZ>, double>((Func<KeyValuePair<ElementWrapper, XYZ>, double>)(r => r.Value.X)).Select<KeyValuePair<ElementWrapper, XYZ>, ElementWrapper>((Func<KeyValuePair<ElementWrapper, XYZ>, ElementWrapper>)(keyValuePair => keyValuePair.Key)));
                        else
                            sortedByLocation.AddRange(source2.OrderByDescending<KeyValuePair<ElementWrapper, XYZ>, double>((Func<KeyValuePair<ElementWrapper, XYZ>, double>)(r => r.Value.X)).Select<KeyValuePair<ElementWrapper, XYZ>, ElementWrapper>((Func<KeyValuePair<ElementWrapper, XYZ>, ElementWrapper>)(keyValuePair => keyValuePair.Key)));
                    }
                }
            }
            else
            {
                List<Dictionary<ElementWrapper, XYZ>> source3 = new List<Dictionary<ElementWrapper, XYZ>>();
                foreach (KeyValuePair<ElementWrapper, XYZ> keyValuePair in dictionary1)
                {
                    if (source3.Count == 0)
                    {
                        Dictionary<ElementWrapper, XYZ> dictionary5 = new Dictionary<ElementWrapper, XYZ>()
            {
              {
                keyValuePair.Key,
                keyValuePair.Value
              }
            };
                        source3.Add(dictionary5);
                    }
                    else
                    {
                        bool flag4 = false;
                        foreach (Dictionary<ElementWrapper, XYZ> dictionary6 in source3)
                        {
                            if (!flag4)
                            {
                                foreach (XYZ xyz in dictionary6.Values)
                                {
                                    if (Math.Abs(xyz.X - keyValuePair.Value.X) < 0.0001)
                                    {
                                        dictionary6.Add(keyValuePair.Key, keyValuePair.Value);
                                        flag4 = true;
                                        break;
                                    }
                                }
                            }
                            else
                                break;
                        }
                        if (!flag4)
                        {
                            Dictionary<ElementWrapper, XYZ> dictionary7 = new Dictionary<ElementWrapper, XYZ>()
              {
                {
                  keyValuePair.Key,
                  keyValuePair.Value
                }
              };
                            source3.Add(dictionary7);
                        }
                    }
                }
                if (source3.Any<Dictionary<ElementWrapper, XYZ>>())
                {
                    source3.Sort((Comparison<Dictionary<ElementWrapper, XYZ>>)((c1, c2) => c1.Values.First<XYZ>().X.CompareTo(c2.Values.First<XYZ>().X)));
                    if (locationOrder == LocationOrder.UpToDownRightToLeft || locationOrder == LocationOrder.DownToUpRightToLeft)
                        source3.Reverse();
                    foreach (Dictionary<ElementWrapper, XYZ> source4 in source3)
                    {
                        bool flag5;
                        switch (locationOrder)
                        {
                            case LocationOrder.DownToUpLeftToRight:
                            case LocationOrder.DownToUpRightToLeft:
                                flag5 = true;
                                break;
                            default:
                                flag5 = false;
                                break;
                        }
                        if (flag5)
                            sortedByLocation.AddRange(source4.OrderBy<KeyValuePair<ElementWrapper, XYZ>, double>((Func<KeyValuePair<ElementWrapper, XYZ>, double>)(c => c.Value.Y)).Select<KeyValuePair<ElementWrapper, XYZ>, ElementWrapper>((Func<KeyValuePair<ElementWrapper, XYZ>, ElementWrapper>)(pair => pair.Key)));
                        else
                            sortedByLocation.AddRange(source4.OrderByDescending<KeyValuePair<ElementWrapper, XYZ>, double>((Func<KeyValuePair<ElementWrapper, XYZ>, double>)(c => c.Value.Y)).Select<KeyValuePair<ElementWrapper, XYZ>, ElementWrapper>((Func<KeyValuePair<ElementWrapper, XYZ>, ElementWrapper>)(pair => pair.Key)));
                    }
                }
            }
            return sortedByLocation;
        }

        private class HeadingAndName
        {
            private readonly string _heading;
            private readonly string _name;

            public HeadingAndName(string heading, string name)
            {
                _heading = heading;
                _name = name;
            }

            public bool IsMatch(string value)
            {
                if (!string.IsNullOrEmpty(_heading) && _heading == value)
                    return true;
                return !string.IsNullOrEmpty(_name) && _name == value;
            }
        }
    }
}
