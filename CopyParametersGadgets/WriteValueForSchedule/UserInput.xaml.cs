using CopyParametersGadgets.Command;
using System.Windows;
using NRPUtils.ValueConverters;

namespace CopyParametersGadgets.WriteValueForScheduleCommand
{
    /// <summary>
    /// Логика взаимодействия для UserInput.xaml
    /// </summary>
    public partial class UserInput : Window
    {
        public readonly VMUserInput vm;
        public UserInput(VMUserInput VM)
        {
            InitializeComponent();
            vm=VM;
            DataContext= vm;
        }

    }
}
