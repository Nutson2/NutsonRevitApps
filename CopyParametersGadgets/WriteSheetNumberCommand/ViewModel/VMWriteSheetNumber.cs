using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using CopyParametersGadgets.Command;
using CopyParametersGadgets.Model;
using NRPUtils.MVVMBase;
using NRPUtils.Model;
using NRPUtils.Extentions;


namespace CopyParametersGadgets.VM
{
    public class VMWriteSheetNumber : NotifyObject
    {
        private readonly Document Doc;
        private ParameterElement  paramForWrite;
        private ViewSheet         selectedSheet;

        public List<CategoryModel>                        Categories          { get; set; } = new List<CategoryModel>();

        public ObservableCollection<Node<ViewSheet>>       Sheets                                  { get; set; } = new ObservableCollection<Node<ViewSheet>>();
        public ObservableCollection<Node<ParametersModel>> ProjectAndSheetParameters               { get; set; } = new ObservableCollection<Node<ParametersModel>>();
        public ObservableCollection<ParameterElement>      AllowedParametersFromSelectedCategories { get; set; } = new ObservableCollection<ParameterElement>();
        public ObservableCollection<ParametersModel>       NewStringParts                          { get; set; } = new ObservableCollection<ParametersModel>();

        public ParameterElement ParamForWrite { get => paramForWrite; set { paramForWrite = value; OnPropertyChanged(); } }
        public ViewSheet        SelectedSheet { get => selectedSheet; set { selectedSheet = value; OnPropertyChanged(); } }
       
        public VMWriteSheetNumber(Document Doc)
        {
            this.Doc=Doc;
            FillSheets();
            if (Sheets.Count == 0) return;
            FillSheetParameters();
            FillCategories();

            PropertyChanged+=SelectedSheet_PropertyChanged;
        }

        private void FillCategories()
        {
            foreach (Category curCategory in Doc.Settings.Categories)
            {
                if (curCategory.CategoryType!= CategoryType.Model || curCategory.Name.Contains(".dwg")) continue;

                var categoryModel = new CategoryModel(curCategory);
                Categories.Add(categoryModel);
                categoryModel.PropertyChanged+=CategoryModel_PropertyChanged;
            }
            Categories=Categories.OrderBy(x => x.Category.Name).ToList();
        }
        private void FillSheets()
        {
            var sheetOrg = BrowserOrganization.GetCurrentBrowserOrganizationForSheets(Doc);
            var viewSheets = new FilteredElementCollector(Doc)
                                    .OfClass(typeof(ViewSheet))
                                    .WhereElementIsNotElementType()
                                    .Cast<ViewSheet>()
                                    .ToList();
            var rootNode = new Node<ViewSheet>() { Name="Листы" };
            foreach (var sheet in viewSheets)
            {
                var info = sheetOrg.GetFolderItems(sheet.Id);
                var curNode = rootNode;

                for (var i = 0; i <= info.Count-1; i++)
                {
                    var fNode = curNode.FindSubNodeByName(info[i].Name);

                    if (fNode==null)
                    {
                        var newNode = new Node<ViewSheet>() { Name=info[i].Name };
                        curNode.Nodes.Add(newNode);
                        curNode= newNode;
                    }
                    else
                        curNode= fNode;

                    if (i==info.Count-1)
                    {
                        var lastNode = new Node<ViewSheet>();
                        curNode.Nodes.Add(lastNode);
                        lastNode.Name=sheet.Name;
                        lastNode.Item=sheet;
                        curNode.PropertyChanged+=lastNode.Selected_PropertyChanged;
                    }
                }
            }
            Sheets.Add(rootNode);
        }
        private void FillSheetParameters()
        {
            var rootNode   = new Node<ParametersModel>() { Name = "Доступные параметры" };
            var projParams = new Node<ParametersModel>() { Name = "Сведения о проекте" };
            Doc.ProjectInformation.Parameters.ToList()
                                            .Select(x  => new Node<ParametersModel>() { Item = new ParametersModel(x) { Owner=Doc.ProjectInformation.GetType().Name }, 
                                                                                        Name = x.Definition.Name} )
                                            .OrderBy(x => x.Name).ToList()
                                            .ForEach(x => projParams.Nodes.Add(x));
            rootNode.Nodes.Add(projParams);
            var sheetParam = new Node<ParametersModel>() { Name="Параметры листа" };
            var sh = new FilteredElementCollector(Doc).OfClass(typeof(ViewSheet))
                                                     .WhereElementIsNotElementType()
                                                     .Cast<ViewSheet>()
                                                     .FirstOrDefault();
            if (sh==null) return;
            sh.Parameters.ToList()
                        .Select(x => new Node<ParametersModel>() { Item=new ParametersModel(x) { Owner=sh.GetType().Name},
                                                                   Name=x.Definition.Name })
                        .OrderBy(x => x.Name).ToList()
                        .ForEach(x => sheetParam.Nodes.Add(x));

            rootNode.Nodes.Add(sheetParam);
            ProjectAndSheetParameters.Add(rootNode);
        }

        private void SelectedSheet_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName!=nameof(SelectedSheet)) return;
            UpdateSheetParametersValue();
        }
        private void CategoryModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName!="Selected") return;
            UpdateAllowedParametersList(Doc,Categories, AllowedParametersFromSelectedCategories);
        }

        public void UpdateAllowedParametersList(Document Doc,List<CategoryModel> categories, 
            ObservableCollection<ParameterElement> parameterElements)
        {
            List<ElementId> intersectlist = new List<ElementId>();
            categories.Where(x => x.Selected)
                    .ToList()
                    .ForEach(x =>
                       {
                           if (intersectlist.Count==0)
                               intersectlist.AddRange(TableView.GetAvailableParameters(Doc, x.Category.Id));
                           else
                               intersectlist= intersectlist.Intersect(
                                   TableView.GetAvailableParameters(Doc, x.Category.Id)).ToList();
                       });

            if (intersectlist==null) return;
            intersectlist= intersectlist.Distinct().ToList();

            parameterElements.Clear();
            var paramsList = intersectlist.Select(x => Doc.GetElement(x))
                                        .Cast<ParameterElement>()
                                        .Where(x => x!=null)
                                        .Where(x => x.GetDefinition().ParameterType==ParameterType.Text)
                                        .OrderBy(x => x.Name).ToList();

            paramsList.ForEach(x => parameterElements.Add(x));
        }
        public void UpdateSheetParametersValue()
        {
            if (SelectedSheet==null) return;
            foreach (Node<ParametersModel> node in ProjectAndSheetParameters[0].Nodes[1].Nodes)
            {
                node.Item.Value=SelectedSheet.LookupParameter(node.Name).TryAsString();
            }
        }

        public void WriteSheetNumberToElements()
        {

            var selectedSheets = Sheets.First().GetSelectedSubNodes().Select(x => x.Item);
            Dictionary<ElementId, ElementId> AllElements = new Dictionary<ElementId, ElementId>();

            var mFilter = new ElementMulticategoryFilter(Categories.Where(x => x.Selected)
                                                    .Select(x => (BuiltInCategory)x.Category.Id.IntegerValue).ToList());

            using (Transaction tr = new Transaction(Doc, "Запись в элементы видимость на листах"))
            {
                tr.Start();
                foreach (ViewSheet sheet in selectedSheets)
                {
                    var views = sheet.GetAllPlacedViews();
                    if (views==null) continue;

                    string sheetString = GetStringForWrite(sheet);
                    var elementsOnSheet=new Dictionary<ElementId, ElementId>();

                    foreach (ElementId viewId in views)
                    {
                        var view = Doc.GetElement(viewId);
                        var elementtsOnViewPort = new FilteredElementCollector(Doc, view.Id).WherePasses(mFilter)
                                                                                            .WhereElementIsNotElementType()
                                                                                            .ToElements();
                        if (elementtsOnViewPort.Count<1) continue;

                        foreach (Element element in elementtsOnViewPort)
                        {
                            if (elementsOnSheet.ContainsKey(element.Id)) continue;

                            var sheetNumberParam = element.GetParameters(ParamForWrite.Name);
                            foreach (var curParam in sheetNumberParam)
                            {
                            if (AllElements.ContainsKey(element.Id))
                            {
                                    var existValue=curParam.AsString();
                                    var prefix=string.IsNullOrEmpty(existValue)?"":", ";
                                    curParam.TrySetValue(existValue + prefix + sheetString);
                            }
                            else
                            {
                                    curParam.TrySetValue(sheetString);
                                AllElements.Add(element.Id, element.Id);
                            }
                            }
                            elementsOnSheet.Add(element.Id, element.Id);
                        }
                    }

                }
                tr.Commit();
            }

        }

        private string GetStringForWrite(ViewSheet sheet)
        {
            var strBuilder = new StringBuilder();
            foreach (ParametersModel parametersModel in NewStringParts)
            {
                strBuilder.Append(parametersModel.Prefix);

                var param=sheet.LookupParameter(parametersModel.Name);
                if(param==null)
                    param=Doc.ProjectInformation.LookupParameter(parametersModel.Name);
                    
                strBuilder.Append(param.TryAsString());

                strBuilder.Append(parametersModel.Suffix);
            }
            return strBuilder.ToString();
        }

        public void AddParameterStringParts(ParametersModel parameter)
        {
            NewStringParts.Add(parameter);
        }

        public void MoveUpStringParts(object obj)
        {
            var stringPart = (ParametersModel)obj;
            if (stringPart ==null) return;
            var existPos = NewStringParts.IndexOf(stringPart);
            if (existPos==-1 ||existPos== 0) return;
            NewStringParts.Move(existPos, existPos-1);
        }

        public void MoveDownStringParts(object obj)
        {
            var stringPart = (ParametersModel)obj;
            if (stringPart ==null) return;
            var existPos = NewStringParts.IndexOf(stringPart);
            if (existPos==-1 || existPos==NewStringParts.Count-1) return;
            NewStringParts.Move(existPos, existPos+1);
        }

        public void RemoveStringParts(object obj)
        {
            var stringPart = (ParametersModel)obj;
            if (stringPart ==null) return;
            NewStringParts.Remove(stringPart);
        }

        #region Жалко удалить

        private static void WriteIntoExplicationSheetNumber(ViewSheet sheet)
        {

            var schedules = sheet.GetSchedules().ToList();
            bool scheduleWasFind = false;
            foreach (ViewSchedule viewSchedule in schedules)
            {
                if (!viewSchedule.Name.ToUpper().Contains("ЭКСПЛИКАЦИЯ")) continue;
                var filters = viewSchedule.Definition.GetFilters();

                for (int i = 0; i <= filters.Count-1; i++)
                {
                    var filter = filters[i];
                    var fieldName = viewSchedule.Definition.GetField(filter.FieldId).GetName();
                    if (fieldName=="Номер листа размещения")
                    {
                        filter.SetValue(sheet.SheetNumber.ToString());
                        viewSchedule.Definition.SetFilter(i, filter);

                        var tableData = viewSchedule.GetTableData();
                        var head = tableData.GetSectionData(SectionType.Header);
                        head.SetCellText(0, 0, "Экспликация помещений");

                        scheduleWasFind=true;
                        break;
                    }
                }

                if (scheduleWasFind) break;
            }
        }

        public void CopyNameToSpace(Document doc)
        {
            var coll = new FilteredElementCollector(doc);
            var CatList = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_MEPSpaces
            };

            var filter = new ElementMulticategoryFilter(CatList);
            var elements = coll.WherePasses(filter).WhereElementIsNotElementType().ToList();

            using (Transaction tr = new Transaction(doc, "перенос в пространства"))
            {
                tr.Start();
                foreach (Element el in elements)
                {
                    el.CopyValueBetweenParameters("Номер помещения", "ИОС_Номер пространства");
                    el.CopyValueBetweenParameters("Имя помещения", "О_Помещение");
                    el.CopyValueBetweenParameters("Номер помещения", "ИОС_Номер пространства");

                }
                tr.Commit();
            }
        }

        #endregion
    }
}
