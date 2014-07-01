#region

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#endregion

namespace DogeMuch.Model
{
    public class NotifyPropImpl : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}