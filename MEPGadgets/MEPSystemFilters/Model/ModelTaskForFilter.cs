using Autodesk.Revit.DB.Plumbing;
using System.Collections.Generic;
using System.Linq;

namespace MEPGadgets.MEPSystemFilters.Model
{
    public class ModelTaskForFilter
    {
        public string Name
        {
            get
            {
                if (Systems.Count <= 0) return "";
                return string.Join(", ", Systems.Select(x => x.Name));
            }
        }
        public List<PipingSystem> Systems { get; set; }

        public ModelTaskForFilter(List<PipingSystem> systems)
        {
            Systems = systems;
        }
    }

}
