using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfFolderBrowser
{
    class ViewerProp : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _FolderName;
        public string FolderName
        {
            get => _FolderName;
            set
            {
                if (value.CompareTo(_FolderName) == 0)
                {
                    return;
                }
                _FolderName = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FolderName));
            }
        }
        private string _Address;
        public string Address
        {
            get => _Address;
            set
            {
                if (value.CompareTo(_Address) == 0)
                {
                    return;
                }
                _Address = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Address));
            }
        }
    }
}
