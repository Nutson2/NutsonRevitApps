using System.Windows;

namespace MEPGadgets.MEPSystemFilters.View
{
    /// <summary>
    /// Логика взаимодействия для ViewSelectSystem.xaml
    /// </summary>
    public partial class ViewSelectSystem : Window
    {
        private readonly VMMEPSystemFilters vm;
        public ViewSelectSystem(VMMEPSystemFilters VM)
        {
            InitializeComponent();
            vm = VM;
            DataContext = vm;
        }
    }
}
