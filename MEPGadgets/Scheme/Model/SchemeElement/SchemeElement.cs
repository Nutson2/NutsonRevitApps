using Autodesk.Revit.DB;

namespace MEPGadgets.Scheme.Model
{
    public class SchemeElement : NotifyObject
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

        public string Name { get; private set; }
        public string Category { get; private set; }
        public string Size { get; private set; }
        public SchemeElement PreviousElement { get; private set; }
        public Element RevitElement { get; private set; }
        public SchemeBranch Branch { get; private set; }
        public BasePressureCalculator PressureCalculator { get; private set; }

        public SchemeElement(
            SchemeBranch Branch,
            Element RevitElement,
            SchemeElement previousElement = null
        )
        {
            this.Branch = Branch;
            this.RevitElement = RevitElement;
            PreviousElement = previousElement;

            Name = this.Branch.Name + " элемент: " + (this.Branch.Elements.Count + 1).ToString();
            CollectInfo();
        }

        private void CollectInfo()
        {
            Category = RevitElement.Category.Name;
            Size = RevitElement.LookupParameter("Размер")?.AsString();
        }
    }
}
