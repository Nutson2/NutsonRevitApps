using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using MEPGadgets.Scheme.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using NRPUtils.MEPUtils;

namespace MEPGadgets.Scheme
{
    public class SystemScheme
    {
        public string BranchNameSuffix = "Ветка ";
        
        public ObservableCollection<SchemeBranch> Branches;
        public ObservableCollection<SchemeElement> BrancheEnds;

        public SystemConsumptionRate SystemConsumptionRate { get; set; }
        
        public Dictionary<string, Connector> ConnectorsInScheme;
        public Dictionary<Element, SchemeElement> ElementsInScheme;

        public UIDocument UIDoc { get; private set; }
        private int branchCount;
        public string SystemName;

        public SystemScheme(UIDocument uiDoc, Element SourceElement)
        {
            Branches = new ObservableCollection<SchemeBranch>();
            BrancheEnds = new ObservableCollection<SchemeElement>();
            ConnectorsInScheme = new Dictionary<string, Connector>();
            ElementsInScheme = new Dictionary<Element, SchemeElement>();
            UIDoc = uiDoc;
            branchCount = 0;

            SystemName = GetSystemName(SourceElement);

            var firstBranch = CreateBranch(SourceElement);
            CreateScheme(firstBranch);
        }
        private void CreateScheme(SchemeBranch schemeBranch)
        {
            Queue<SchemeBranch> queue = new Queue<SchemeBranch>();
            Branches.Add(schemeBranch);

            queue.Enqueue(schemeBranch);

            while (queue.Count > 0) 
            {
                var current = queue.Dequeue();

                var nextBranch=current.GetNextBranch();
                if (!nextBranch.Any()) continue;

                nextBranch.ToList().ForEach(i => queue.Enqueue(i));
            }
        }
        public SchemeBranch CreateBranch(Element firstElementInBranch, SchemeBranch previousBranch = null)
        {
            branchCount++;
            SchemeBranch newBranch= new SchemeBranch(this, firstElementInBranch, branchCount, previousBranch);
           
            //newBranch.Calculators.Add(
            //    new WaterConsumptionCalculator(newBranch));

            return newBranch;
        }
        private string GetSystemName(Element SourceElement)
        {
            var sourceCon = MEPUtils.GetConnectorManager(SourceElement).Connectors.Cast<Connector>().First();
            return sourceCon.MEPSystem.Name.Split(' ').First();
        }
    }
}
