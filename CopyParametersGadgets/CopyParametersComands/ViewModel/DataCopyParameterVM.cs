using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;
using NRPUtils.Extentions;

namespace CopyParametersGadgets
{
    public class DataCopyParameterVM : DataCopyParameterVMBase 
    {
        private readonly BuiltInCategory _category;
        private List<Element>   donor;
        public List<Element> Donors { get { return donor; } set { donor = value; OnPropertyChanged(); } } 

        public DataCopyParameterVM(Document Doc, BuiltInCategory builtInCategory)
        {
            _doc      = Doc;
            _category = builtInCategory;
            Donors    = new List<Element>();
        }
        public bool SharedParametersFromGroup(ICollection<ElementId> elementIds )
        {
            if (elementIds.Count>0)
            {
                foreach (ElementId elId  in elementIds)
                {
                    Element el = _doc.GetElement(elId);
                    if ((BuiltInCategory)el.Category.Id.IntegerValue==_category)
                    {
                        Donors.Add(el); 
                    }
                }
            }

            Element elementDonor = _doc.GetElement(elementIds.FirstOrDefault());
            if (elementDonor == null)return false;
            
            ParamSet = CollectParametersForTransit(elementDonor);
            return true;
            
        }
        private IList<ElementId> GetElementIds(Element donor)
        {
            IList<ElementId> elementIntersectsDonor;
            switch (_category)
            {
                case BuiltInCategory.OST_IOSModelGroups:
                    Group group = (Group)donor;
                    elementIntersectsDonor = group.GetMemberIds();
                    break;
                default:

                    BoundingBoxXYZ bbox = donor.get_BoundingBox(_doc.ActiveView);
                    Outline o=new Outline(bbox.Min,bbox.Max);
                    FilteredElementCollector collector = new FilteredElementCollector(_doc);

                    BoundingBoxIntersectsFilter boundingBoxIntersectsFilter = new BoundingBoxIntersectsFilter(o);

                    ICollection<ElementId> idsExclude   = new List<ElementId>();

                    idsExclude.Add(donor.Id);

                    elementIntersectsDonor = (IList<ElementId>)collector
                        .WhereElementIsNotElementType()
                        .Excluding(idsExclude)
                        .WherePasses(boundingBoxIntersectsFilter)
                        .ToElementIds();


                    break;

            }
            return elementIntersectsDonor;
        }

        public override void CopyParameters(IList selectItem)
        {
            if (Donors.Count < 1)
            {
                FilteredElementCollector col = new FilteredElementCollector(_doc);
                Donors = col
                        .OfCategoryId(new ElementId(_category))
                        .WhereElementIsNotElementType()
                        .ToElements().ToList<Element>();
            }

            using (Transaction tr = new Transaction(_doc, "Копирование параметров в элементы"))
            {
                tr.Start();

                foreach (Element donor in Donors)
                {
                    IList<ElementId> elementInGroup = GetElementIds(donor);
                    if (elementInGroup == null) { continue; }

                    foreach (ElementId ElId in elementInGroup)
                    {
                        Element curEl = _doc.GetElement(ElId);
                        if (curEl == null) continue;
                        if (curEl.Category == null) continue;
                        if (curEl.Category.Name == "Формы") continue;

                        foreach (DataParametersM data in selectItem)
                        {
                            Parameter curParam;
                            Parameter donorParam;
                            if (data.Parameter.IsShared)
                            {
                                curParam = curEl.get_Parameter(data.Guid);
                                donorParam = donor.get_Parameter(data.Guid);

                            }
                            else
                            {
                                curParam = curEl.LookupParameter(data.Name);
                                donorParam = donor.LookupParameter(data.Name);

                            }
                            if (curParam == null || curParam.IsReadOnly) continue;
                            ParameterExtention.CopyParameterValue(curParam, donorParam, data.AppendValue);
                        }
                    }

                }
                tr.Commit();
            }
        }
    }
}
