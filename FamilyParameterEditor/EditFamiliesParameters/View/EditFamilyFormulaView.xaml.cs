using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FamilyParameterEditor.EditFamiliesParameters.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FamilyParameterEditor.EditFamiliesParameters.View
{
    public partial class EditFamilyFormulaView : Window
    {
        private readonly VMEditFamiliesParameters VMEditFamilies;

        public EditFamilyFormulaView(ExternalCommandData commandData, RevitTask revitTask)
        {
            InitializeComponent();
            VMEditFamilies=new VMEditFamiliesParameters(commandData, revitTask);
            DataContext = VMEditFamilies;
            Closed += EditFamilyFormulaView_Closed;
        }

        private void EditFamilyFormulaView_Closed(object sender, EventArgs e)
        {
            VMEditFamilies.Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VMEditFamilies.ApplyNewFormula();
        }

        
    }
}
