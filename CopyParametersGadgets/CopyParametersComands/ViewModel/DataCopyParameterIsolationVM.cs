using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using NRPUtils.Extentions;
namespace CopyParametersGadgets
{
    public class DataCopyParameterIsolationVM : DataCopyParameterVMBase
    {
        private List<Element> donor;

        public List<Element> PipeInsulations
        {
            get { return donor; }
            set
            {
                donor = value;
                OnPropertyChanged();
            }
        }
        public DataCopyParameterIsolationVM(Document Doc)
        {
            _doc            = Doc;
            PipeInsulations = new List<Element>();
        }

        public bool PrepareParameters()
        {
            PipeInsulations = new FilteredElementCollector(_doc).OfClass(typeof(PipeInsulation))?.WhereElementIsNotElementType()?.ToList();
            if (PipeInsulations == null || PipeInsulations.Count ==0) return false;
            ParamSet = CollectParametersForTransit(PipeInsulations.First());
            return true;

        }

        public override void CopyParameters(IList selectItem)
        {

            using Transaction tr = new Transaction(_doc, "Копирование параметров в элементы");
            tr.Start();

            foreach (PipeInsulation pipeInsulation in PipeInsulations.Cast<PipeInsulation>())
            {
                var curEl = _doc.GetElement(pipeInsulation.HostElementId);

                if (curEl == null) continue;
                if (curEl.Category == null) continue;

                foreach (DataParametersM data in selectItem)
                {
                    Parameter curParam;
                    Parameter donorParam;
                    if (data.Parameter.IsShared)
                    {
                        donorParam = curEl.get_Parameter(data.Guid);
                        curParam = pipeInsulation.get_Parameter(data.Guid);

                    }
                    else
                    {
                        donorParam = curEl.LookupParameter(data.Name);
                        curParam = pipeInsulation.LookupParameter(data.Name);
                    }

                    if (curParam == null || curParam.IsReadOnly) continue;
                    ParameterExtention.CopyParameterValue(curParam, donorParam, data.AppendValue);
                }

            }
            tr.Commit();
        }
    }
}
