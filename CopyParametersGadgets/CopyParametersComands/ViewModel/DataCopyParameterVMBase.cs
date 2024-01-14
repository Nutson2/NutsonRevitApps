using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using NRPUtils.MVVMBase;

namespace CopyParametersGadgets
{
    public abstract class DataCopyParameterVMBase: NotifyObject
    {
        public List<DataParametersM> ParamSet;
        protected Document _doc;

        public abstract void CopyParameters(IList selectItem);

        protected List<DataParametersM> CollectParametersForTransit(Element elementDonor)
        {
            List<DataParametersM> Params = new List<DataParametersM>();
            foreach (Parameter param in elementDonor.Parameters)
            {
                if (param.IsReadOnly) continue;

                Params.Add(new DataParametersM
                {
                    Name = param.Definition.Name,
                    StorageType = param.StorageType.ToString(),
                    Parameter = param,
                    Guid = param.IsShared ? param.GUID : Guid.Empty
                });
            }
            Params = Params.OrderBy(x => x.Name).ToList();
            return Params;
        }

    }
}