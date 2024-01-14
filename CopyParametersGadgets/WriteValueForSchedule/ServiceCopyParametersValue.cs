using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NRPUtils.Extentions;

namespace CopyParametersGadgets.Command
{
    public class ServiceCopyParametersValue
    {
        private readonly Document      doc;
        private readonly VMUserInput   vm;
        private readonly StringBuilder errors = new StringBuilder();
        public ServiceCopyParametersValue(UIDocument UIDoc, VMUserInput VM)
        {
            doc = UIDoc.Document;
            vm = VM;
        }

        public bool CopyParamValue()
        {
            List<Element> elements = GetMEPElements(doc);
            var cashedData=new CacheData();

            using (Transaction tr = new Transaction(doc, "перенос"))
            {
                tr.Start();
                Element element=null;
                foreach (Element el in elements)
                {
                    try
                    {
                            element = el;
                            CopyingParametersInElement(cashedData, el);
                            element = null;
                    }
                    catch (Exception ex)
                    {
                        errors.AppendLine($"Element ID: {element.Id} || Message: {ex.Message} || StackTrace: {ex.StackTrace}");
                    }
                }
                tr.Commit();
            }

            if (errors.Length > 0) SaveErrorsLog();
            return true;
        }

        private void SaveErrorsLog()
        {
            var path=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (var stream = new StreamWriter(Path.Combine(path, "Schedule error.txt"), false, Encoding.Default))
            {
                stream.WriteLine(errors.ToString());
            }
            Process.Start(Path.Combine(path, "Schedule error.txt"));
        }
        private void CopyingParametersInElement( CacheData cashedData, Element el)
        {
            switch ((BuiltInCategory)el.Category.Id.IntegerValue)
            {
                case BuiltInCategory.OST_MechanicalEquipment:
                    // "1. Оборудование";
                    CopyQuantity(el);
                    break;

                case BuiltInCategory.OST_PlumbingFixtures:
                    //Санитарные приборы";
                    CopyQuantity(el);
                    SetSystemName(el);
                    SetFamilyInstanceSizeMark((FamilyInstance)el);
                    break;

                case BuiltInCategory.OST_PipeAccessory:
                    //Трубопроводная арматура";
                    CopyQuantity(el);
                    SetSystemName(el);
                    SetFamilyInstanceSizeMark((FamilyInstance)el);
                    break;

                case BuiltInCategory.OST_FlexPipeCurves:
                    CopyPipeQuantity(el);
                    SetSystemName(el);
                    break;

                case BuiltInCategory.OST_PipeInsulations:
                    CopyinsulationQuantity(el);
                    SetSystemName(el);
                    SetInsulationSizeMark(cashedData, (PipeInsulation)el);
                    break;

                case BuiltInCategory.OST_PipeFitting:
                    CopyQuantity(el);
                    SetSystemName(el);
                    SetFamilyInstanceSizeMark((FamilyInstance)el);
                    break;

                case BuiltInCategory.OST_PipeCurves:
                    CopyPipeQuantity(el);
                    SetSystemName(el);
                    SetPipeSizeMark(el);
                    break;
                default:
                    break;

            }
            SetGroupParamValue(cashedData,el);
        }
        private static List<Element> GetMEPElements(Document doc)
        {
            var coll    = new FilteredElementCollector(doc);
            var CatList = new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_PipeCurves,
                    BuiltInCategory.OST_PipeFitting,
                    BuiltInCategory.OST_PipeInsulations,
                    BuiltInCategory.OST_PipeAccessory,
                    BuiltInCategory.OST_FlexPipeCurves,
                    BuiltInCategory.OST_PlumbingFixtures,
                    BuiltInCategory.OST_MechanicalEquipment
                };

            var filter   = new ElementMulticategoryFilter(CatList);
            var elements = coll.WherePasses(filter).WhereElementIsNotElementType().ToList();
            return elements;
        }

        private void SetSystemName(Element el)
        {
            var ownerFam = el;
            if (el is FamilyInstance fInst && fInst.SuperComponent != null)
                ownerFam = fInst.SuperComponent;

            el.CopyValueBetweenElements(ownerFam, BuiltInParameter.RBS_SYSTEM_NAME_PARAM, "ИОС_Имя системы");

            var elType = doc.GetElement(ownerFam.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM).AsElementId());
            if (elType != null)
                el.CopyValueBetweenElements(elType, BuiltInParameter.ALL_MODEL_DESCRIPTION, "ИОС_Наименование системы");
        }

        #region Size mark
        private void SetPipeSizeMark(Element el)
        {
            var pipeType = doc.GetElement(el.GetTypeId()) as PipeType;
            var pipeMark = pipeType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString();
            if (string.IsNullOrEmpty(pipeMark)) return;

            var OD = el.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
            var ID = el.get_Parameter(BuiltInParameter.RBS_PIPE_INNER_DIAM_PARAM);
            var Dy = el.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
            var t = UnitUtils.ConvertFromInternalUnits((OD.AsDouble()-ID.AsDouble())*0.5, DisplayUnitType.DUT_MILLIMETERS);

            var markParts = pipeMark.Split('x');

            foreach (var mPart in markParts)
            {
                switch (mPart)
                {
                    case "Dy":
                    pipeMark = pipeMark.Replace("Dy", Dy.AsValueString());
                    break;
                    case "OD":
                    pipeMark = pipeMark.Replace("OD", OD.AsValueString());
                    break;
                    case "t":
                    pipeMark = pipeMark.Replace("t", string.Format("{0:F1}", t));
                    break;
                    default:
                    break;
                }
            }
            var param = el.LookupParameter("ИОС_Размер_Специф");
            param.TrySetValue(pipeMark);
        }
        private void SetInsulationSizeMark(CacheData cachedData, PipeInsulation el)
        {
            Dictionary<int, int[]> currentTypeSizes = null;
            var hostEl = doc.GetElement(el.HostElementId);
            if (hostEl is FamilyInstance)
            {
                var exclude = el.LookupParameter("Исключить из спецификации");
                exclude.TrySetValue(1);
                return;
            }

            var insTypeId      = el.GetTypeId();
            var insulationType = doc.GetElement(insTypeId);
            var insTypeComment = insulationType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString();

            var resultString = string.Empty;
            var Thikness     = el.get_Parameter(BuiltInParameter.RBS_INSULATION_THICKNESS_FOR_PIPE);
            var Thikness_mm  = UnitUtils.ConvertFromInternalUnits(Thikness.AsDouble(), DisplayUnitType.DUT_MILLIMETERS);

            if (string.IsNullOrEmpty(insTypeComment)) return;

            if (insTypeComment.StartsWith("Сортамент"))
            {
                if (cachedData.InsulationSizes.ContainsKey(insTypeId))
                {
                    currentTypeSizes = cachedData.InsulationSizes[insTypeId];
                }
                else
                {
                    currentTypeSizes = insTypeComment.Split(':').Skip(1)
                        .Select(x => x.Split(';').Select(y => int.Parse(y)).ToArray())
                        .ToDictionary(x => x[0], x => x.Skip(1).ToArray());
                    cachedData.InsulationSizes.Add(insTypeId, currentTypeSizes);
                }
                var pipeOD = UnitUtils.ConvertFromInternalUnits(
                                hostEl.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER)
                                                .AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
                var insulationID = currentTypeSizes[(int)Thikness_mm].FirstOrDefault(x=>x>=pipeOD);

                resultString = $"{insulationID}, t={Thikness_mm}мм";
            }
            else if (insTypeComment.StartsWith("t"))
            {
                resultString = $"t={Thikness_mm}мм";
            }

            var param = el.LookupParameter("ИОС_Размер_Специф");
            param.TrySetValue(resultString);
        }
        private void SetFamilyInstanceSizeMark( FamilyInstance el)
        {
            var Size = el.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE);
            if (Size == null) return;

            var SizeString = Size.AsString();
            if(string.IsNullOrEmpty( SizeString)) return;

            var pipeSettings = PipeSettings.GetPipeSettings(doc);
            if (pipeSettings.SizePrefix != "") SizeString.Replace(pipeSettings.SizePrefix, "");
            if (pipeSettings.SizeSuffix != "") SizeString.Replace(pipeSettings.SizeSuffix, "");

            var newSize_msv = SizeString.Split(pipeSettings.ConnectorSeparator.ToArray(), StringSplitOptions.RemoveEmptyEntries)
                                    .Distinct();

            var param = el.LookupParameter("ИОС_Размер_Специф");
            param.TrySetValue(string.Join("x", newSize_msv));

            var o_Coll = el.LookupParameter("О_Количество");
            o_Coll.TrySetValue(1.0);

            var subFI = el.GetSubComponentIds();
            if (subFI == null) return;

            foreach (var subEl in subFI)
            {
                SetFamilyInstanceSizeMark((FamilyInstance)doc.GetElement(subEl));
            }

        }

        #endregion

        #region Quantity
        private void CopyPipeQuantity(Element el)
        {
            var O_Col = el.LookupParameter("О_Количество");
            var length = el.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
            if (O_Col.IsReadOnly) return;

            O_Col.Set(Math.Round(UnitUtils.ConvertFromInternalUnits(length.AsDouble() * vm.PipeSafetyFactor,
                                                            DisplayUnitType.DUT_METERS), vm.PipeRound));
        }
        private void CopyinsulationQuantity(Element el)
        {
            var ins = el as PipeInsulation;
            var parentEl = el.Document.GetElement(ins.HostElementId);
            if (parentEl is Pipe pipe)
            {
                var length =Math.Round(
                                UnitUtils.ConvertFromInternalUnits(
                                        el.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)
                                        .AsDouble()*vm.PipeInsulationSafetyFactor, DisplayUnitType.DUT_METERS),vm.InsulationRound);

                var d = UnitUtils.ConvertFromInternalUnits(pipe.Diameter, DisplayUnitType.DUT_MILLIMETERS);
                if (d > 160)
                {
                    var t = el.get_Parameter(BuiltInParameter.RBS_INSULATION_THICKNESS_FOR_PIPE);
                    var t_mm = UnitUtils.ConvertFromInternalUnits(t.AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
                    length = length * Math.PI * (d + t_mm) / 1000;
                }
                var O_Col = el.LookupParameter("О_Количество");
                if (O_Col.IsReadOnly) return;

                O_Col.Set(length);
            }

        }
        private void CopyQuantity(Element el)
        {
            var O_Col = el.LookupParameter("О_Количество");
            O_Col.TrySetValue(1);
        }

        #endregion

        private void SetGroupParamValue(CacheData cachedData, Element el)
        {
            SubSetGroupParamValue(cachedData, el);
            if (!(el is FamilyInstance famInstance)) return;

            var subFI = famInstance.GetSubComponentIds();
            if (subFI == null)  return; 

            foreach (var subEl in subFI)
            {
                SubSetGroupParamValue(cachedData, doc.GetElement(subEl));
            }
        }
        private void SubSetGroupParamValue(CacheData cachedData, Element el)
        {
            Element ownerFam = el;

            if (el is FamilyInstance fInst && fInst.SuperComponent != null)
                ownerFam = fInst.SuperComponent;

            Parameter paramIOSSystemName;
            if (el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures)
                paramIOSSystemName = el.LookupParameter("ИОС_Наименование системы");
            else
                paramIOSSystemName = ownerFam.LookupParameter("ИОС_Наименование системы");

            var paramOName         = el.LookupParameter("О_Наименование");
            var paramIOSSize       = el.LookupParameter("ИОС_Размер_Специф");
            var paramOObosnachenie = el.LookupParameter("О_Обозначение");
            var paramOZavod        = el.LookupParameter("О_Завод-изготовитель");

            if (paramOName == null || paramIOSSize == null || paramOObosnachenie == null || paramOZavod == null)
            {
                var elTypeId = el.GetTypeId();
                var elType   = doc.GetElement(elTypeId);

                if (paramOName == null) paramOName = elType.LookupParameter("О_Наименование");
                if (paramIOSSize == null) paramIOSSize = elType.LookupParameter("ИОС_Размер_Специф");
                if (paramOObosnachenie == null) paramOObosnachenie = elType.LookupParameter("О_Обозначение");
                if (paramOZavod == null) paramOZavod = elType.LookupParameter("О_Завод-изготовитель");
            }

            string CategoryNameGost=string.Empty;
            if (cachedData.GostCategoryNames.ContainsKey((BuiltInCategory)el.Category.Id.IntegerValue))
            {
                CategoryNameGost= cachedData.GostCategoryNames[(BuiltInCategory)el.Category.Id.IntegerValue];
                el.LookupParameter("Категория элемента").TrySetValue(CategoryNameGost);
            }

            var paramIOSNamefull = el.LookupParameter("О_Наименование и обозначение_экз");
            paramIOSNamefull.TrySetValue(paramOName.TryAsString() + " Ø" + paramIOSSize.TryAsString());

            var ElCategory   = el.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM);
            var ElStage      = ownerFam.get_Parameter(BuiltInParameter.PHASE_CREATED);
            var paramSorting = el.LookupParameter("Сортировка в спецификации");

            if (ElCategory == null || ElStage == null || paramSorting == null) return;

            var stringForSorting = string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}",
                                                  ElStage.TryAsString(),
                                                  paramIOSSystemName.TryAsString(),
                                                  CategoryNameGost,
                                                  ElCategory.TryAsString(),
                                                  paramOZavod.TryAsString(),
                                                  paramOName.TryAsString(),
                                                  paramOObosnachenie.TryAsString(),
                                                  paramIOSSize.TryAsString());
            paramSorting.TrySetValue(stringForSorting);

            var ElGroupBOP              = el.LookupParameter("Сортировка в спецификации ВОР");
            var ElApplicationConditions = el.LookupParameter("О_Условия применения");
            var ElSheetReference        = el.LookupParameter("О_Ссылка на лист");

            var stringForSortingBOP = string.Format("{0}_{1}_{2}",
                                                        stringForSorting,
                                                        ElApplicationConditions.TryAsString(),
                                                        ElSheetReference.TryAsString());
            ElGroupBOP.TrySetValue(stringForSortingBOP);

        }

        public class CacheData
        {
            public readonly Dictionary<ElementId, Dictionary<int, int[]>> InsulationSizes = new Dictionary<ElementId, Dictionary<int, int[]>>();

            public readonly Dictionary<BuiltInCategory,string> GostCategoryNames;
            public CacheData()
            {
                GostCategoryNames= new Dictionary<BuiltInCategory, string>
                   {
                        { BuiltInCategory.OST_MechanicalEquipment,  "1. Оборудование"},
                        { BuiltInCategory.OST_PlumbingFixtures,     "2. Санитарные приборы"},
                        { BuiltInCategory.OST_PipeAccessory,        "3. Трубопроводная арматура"},
                        { BuiltInCategory.OST_FlexPipeCurves,       "4. Трубопроводы"},
                        { BuiltInCategory.OST_PipeFitting,          "4. Трубопроводы"},
                        { BuiltInCategory.OST_PipeCurves,           "4. Трубопроводы"},
                        { BuiltInCategory.OST_PipeInsulations,      "5. Конструкции теплоизоляционные"}
                   };
            }
        }
    }
}
