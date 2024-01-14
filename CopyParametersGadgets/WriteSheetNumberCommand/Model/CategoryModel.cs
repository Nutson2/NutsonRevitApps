using Autodesk.Revit.DB;
using NRPUtils.MVVMBase;

namespace CopyParametersGadgets.Model
{
    public class CategoryModel:NotifyObject
    {
        private Category category;
        private bool     selected;

        public Category Category
        {
            get { return category; }
            set { category = value; }
        }

        public bool Selected
        {
            get { return selected; }
            set { selected = value; OnPropertyChanged(); }
        }

        public CategoryModel(Category category)
        {
            Category=category;
            Selected= false;
        }
    }
}
