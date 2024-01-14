using Autodesk.Revit.DB;
using NRPUtils.Extentions;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CopyParametersGadgets
{
    public class DataCopyParameterSubFamiliesVM : DataCopyParameterVMBase
    {
        private List<FamilyInstance> donors;
        public List<FamilyInstance> Donors
        {
            get { return donors; }
            set
            {
                donors = value;
                OnPropertyChanged();
            }
        }

        public DataCopyParameterSubFamiliesVM(Document Doc)
        {
            _doc = Doc;
            Donors = new List<FamilyInstance>();
        }
        public bool PrepareParentFamilyParameters(ICollection<ElementId> elementIds)
        {
            if (elementIds.Count == 0) return false;

            foreach (ElementId elId in elementIds)
            {
                if (_doc.GetElement(elId) is FamilyInstance familyInstance) 
                    Donors.Add(familyInstance);
            }

            ParamSet = CollectParametersForTransit(Donors.First());
            return true;

        }

        public override void CopyParameters(IList selectItem)
        {

            using (Transaction tr = new Transaction(_doc, "Копирование параметров в вложенные семейства"))
            {
                tr.Start();

                foreach (FamilyInstance donor in Donors)
                {
                    var subelementsID = donor.GetSubComponentIds();
                    if (subelementsID == null) { continue; }

                    foreach (ElementId ElId in subelementsID)
                    {
                        if (!(_doc.GetElement(ElId) is FamilyInstance curEl)) continue;

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
