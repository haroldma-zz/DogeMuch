#region

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DogeMuch.Model;
using DogeMuch.Utility;

#endregion

namespace DogeMuch.ViewModel
{
    public class MainPageViewModel : NotifyPropImpl
    {
        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var balance = await App.Api.GetBalanceAsync();
                DogeBalance = string.Format("{0:0.000000Ɖ}", balance);

                var adds = await App.Api.GetMyAddressesAsync();
                MyAddressesCollection = new ObservableCollection<string>(adds);
            }
            catch (DogeException e)
            {
                if (FailedToLoad != null)
                    FailedToLoad(e, null);
            }
            IsLoading = false;
        }

        public event EventHandler FailedToLoad;

        private string _dogeBalance;
        private bool _isLoading;
        private ObservableCollection<string> _myAddressCollection;
        private ObservableCollection<string> _myWalletsCollection;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; NotifyPropertyChanged(); NotifyPropertyChanged("IsActionEnabled"); }
        }

        public bool IsActionEnabled
        {
            get { return !IsLoading; }
        }

        public ObservableCollection<string> MyAddressesCollection
        {
            get { return _myAddressCollection; }
            set
            {
                _myAddressCollection = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> MyWalletsCollection
        {
            get { return _myWalletsCollection; }
            set
            {
                _myWalletsCollection = value;
                NotifyPropertyChanged();
            }
        }

        public string DogeBalance
        {
            get { return _dogeBalance; }
            set
            {
                _dogeBalance = value;
                NotifyPropertyChanged();
            }
        }
    }
}