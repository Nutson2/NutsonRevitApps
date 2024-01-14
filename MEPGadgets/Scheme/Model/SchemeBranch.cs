using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NRPUtils.MVVMBase;
using NRPUtils.MEPUtils;



namespace MEPGadgets.Scheme.Model
{
    public class SchemeBranch : NotifyObject
    {
        #region for Form
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                isExpanded = value;
                OnPropertyChanged();

            }
        }

        #endregion

        private bool analyzingFinished;
        public bool AnalyzingFinished
        {
            get => analyzingFinished;
            set
            {
                analyzingFinished = value;
                OnPropertyChanged();
            }
        }

        public string Name { get; private set; }

        private readonly SystemScheme systemScheme;
        private readonly Element      firstElement;
        private SchemeElement         currentElement;

        public List<ICalculator> Calculators { get; private set; }
        public ObservableCollection<SchemeElement> Elements { get; set; }
        public ObservableCollection<SchemeBranch> NextBranches { get; set; }
        public SchemeBranch PreviousBranch { get; private set; }
        public IEnumerable Items
        {
            get
            {
                return Elements?.Cast<object>().Concat(NextBranches);
            }
        }

        public SchemeBranch(SystemScheme SystemScheme,
                            Element firstElementInBranch,
                            int BranchNumber,
                            SchemeBranch previousBranch = null)
        {
            PreviousBranch = previousBranch;
            systemScheme   = SystemScheme;
            Name = systemScheme.BranchNameSuffix + systemScheme.SystemName + " №" + BranchNumber;

            Calculators    = new List<ICalculator>();
            Elements       = new ObservableCollection<SchemeElement>();
            NextBranches   = new ObservableCollection<SchemeBranch>();
            currentElement = null;
            firstElement   = firstElementInBranch;
        }

        internal IList<SchemeBranch> GetNextBranch()
        {
            var el = firstElement;
            List<SchemeBranch> result = new List<SchemeBranch>();

            while (true)
            {
                RegisterElementInBranch(el);
                var nextElements = GetNextElements(el);

                if (!nextElements.Any())
                {
                    //Branch end
                    systemScheme.BrancheEnds.Add(currentElement);
                    Calculate();

                    AnalyzingFinished = true;
                    break;
                }
                else if (nextElements.Count == 1)
                {
                    //Next element in branch
                    el = nextElements[0];
                    continue;
                }
                else if (nextElements.Count > 1)
                {
                    //Branch divided
                    foreach (var element in nextElements)
                    {
                        var newBranch=systemScheme.CreateBranch(element,this);
                        newBranch.PropertyChanged += IsExpanded_PropertyChanged;
                        newBranch.PropertyChanged += AnalyzingFinished_PropertyChanged;

                        NextBranches.Add(newBranch);
                        result.Add(newBranch);
                    }
                    break;
                }
            }
            return result;
        }
        private void RegisterElementInBranch(Element element)
        {
            if (!element.LookupParameter("Имя системы").AsString().Contains(systemScheme.SystemName)) return;

            currentElement = new SchemeElement(this, element, currentElement);
            Elements.Add(currentElement);
            systemScheme.ElementsInScheme.Add(element, currentElement);
        }
        private IList<Element> GetNextElements(Element element)
        {
            IList<Element> result = new List<Element>();

            var CM = MEPUtils.GetConnectorManager(element);
            if (CM == null) return result;


            foreach (Connector c in CM.Connectors)
            {
                if (!c.IsConnected) continue;
                if (c.Domain != Domain.DomainPiping || c.MEPSystem == null) continue;
                if (!c.MEPSystem.Name.Contains(systemScheme.SystemName)) continue;
                if (systemScheme.ConnectorsInScheme
                    .ContainsKey(c.Owner.Id.ToString() + c.Id.ToString())) continue;

                result.Add(GetConnectedElement(c));
            }

            return result;
        }
        private void Calculate()
        {
            if (Calculators.Count == 0) return;
            Calculators.ForEach(x => x.Calculate());
        }
        private void AnalyzingFinished_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AnalyzingFinished)) return;
            if (NextBranches.Any(br => br.AnalyzingFinished == false)) return;
            //TODO: Собрать расходы с подчиненных веток
        }
        private void IsExpanded_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsExpanded))
                IsExpanded = ((SchemeBranch)sender).IsExpanded;
        }
        private Element GetConnectedElement(Connector curConnector)
        {
            var key = curConnector.Owner.Id.ToString() + curConnector.Id.ToString();
            if (systemScheme.ConnectorsInScheme.ContainsKey(key)) return null;
            systemScheme.ConnectorsInScheme.Add(key, curConnector);

            var respondedConnector = GetRespondingConnector(curConnector);

            if (respondedConnector == null) return null;
            var respondConnectorKey = respondedConnector.Owner.Id.ToString() + respondedConnector.Id.ToString();
            systemScheme.ConnectorsInScheme.Add(respondConnectorKey, respondedConnector);

            return respondedConnector.Owner;
        }
        private Connector GetRespondingConnector(Connector con)
        {
            var respondCon = con.AllRefs.Cast<Connector>()
                .Where(x => x.Owner.Id != con.Owner.Id)
                .Where(x => x.Owner.Category.Id.IntegerValue != (int)BuiltInCategory.OST_PipeInsulations)
                .Where(x => x.Owner.Category.Id.IntegerValue != (int)BuiltInCategory.OST_PipingSystem)
                .ToList();

            return respondCon.FirstOrDefault();
        }
    }
}
