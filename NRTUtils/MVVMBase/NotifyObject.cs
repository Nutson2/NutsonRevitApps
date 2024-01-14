using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NRPUtils.MVVMBase
{
    public class NotifyObject: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}