using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MEPGadgets.MEPSystemFilters.Model
{
    public partial class CategoryModel : ObservableObject 
    {
        public Category Category { get; set; }
        [ObservableProperty]
        private bool _selected;

        public CategoryModel(Category category)
        {
            Category = category;
            Selected = false;
        }
    }

}
