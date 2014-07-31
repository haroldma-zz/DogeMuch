#region

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using System;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using DogeMuch.Model;
using DogeMuch.Utility;
using DogeMuch.ViewModel;

#endregion

namespace DogeMuch
{
    public sealed partial class MainPage
    {
        public readonly MainPageViewModel Vm = new MainPageViewModel();
        private bool _refresh;

        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            DataContext = Vm;
            Vm.FailedToLoad += async (sender, args) =>
            {
                var e = sender as DogeException;
                if (!_refresh)
                {
                    var msg = new MessageDialog("much bye, shutting down, problem: " + e.InnerException.Message)
                    {
                        Title = e.Message
                    };
                    await msg.ShowAsync();
                    Application.Current.Exit();
                }
                else
                    MessageBox.Show(e.InnerException.Message, e.Message);
            };
            _refresh = false;
            Vm.LoadDataAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back || App.QrScanAddress == null) return;
            MySentToAddress.Text = App.QrScanAddress;
            App.QrScanAddress = null;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _refresh = true;
            Vm.LoadDataAsync();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(MySentAmount.Text))
                MessageBox.Show("plz input doge amount!", "much error");
            else if (string.IsNullOrEmpty(MySentToAddress.Text))
                MessageBox.Show("plz input sent to address!", "much error");
            else if (string.IsNullOrEmpty(MyDogePin.Text))
                MessageBox.Show("plz input pin!", "much error");
            else
            {
                if (MyDogePin.Text.Length < 8)
                {
                    MessageBox.Show("Pin must be 8 characters or longer.");
                    return;
                }

                double doge;
                try
                {
                    doge = double.Parse(MySentAmount.Text);
                }
                catch
                {
                    MessageBox.Show("wow, doge amount must be number", "much error");
                    return;
                }

                if (doge < 5)
                {
                    MessageBox.Show("minimum doge amount for sending is 5", "much error");
                    return;
                }

                try
                {
                    Vm.IsLoading = true;
                    await App.Api.WithdrawAsync(doge, MyDogePin.Text, MySentToAddress.Text);
                    MySentAmount.Text = "";
                    MySentToAddress.Text = "";
                    MyDogePin.Text = "";
                }
                catch (DogeException ex)
                {
                    MessageBox.Show(ex.InnerException.Message, ex.Message);
                }
                finally
                {
                    Vm.IsLoading = false;
                }
            }
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var addrs = e.ClickedItem as BlockAddress;
            new QrDialog(addrs.Address).ShowAsync();
        }

        /*private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (QrScanner));
        }*/

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            MySentToAddress.Text = "DKCYsWR8eqXz2sZBfcpJDgp4GpfcSVeQU3";
        }

        private async void NewAddressButton_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog("new address", "Label (optional)", "Plz make");
            var result = await input.ShowAsync();

            if (result != ContentDialogResult.Primary) return;

            var label = input.InputText;
            try
            {
                await App.Api.GetNewAddressAsync(label);
                await Vm.LoadDataAsync();
            }
            catch (DogeException dogeException)
            {
                MessageBox.Show(dogeException.InnerException.Message, dogeException.Message);
            }
        }

        private
            void MySentAmount_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            ForceNumOnly(e);
        }

        private void ForceNumOnly(KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Back && ((e.Key < VirtualKey.Number0) || (e.Key > VirtualKey.Number9)) &&
                ((e.Key < VirtualKey.NumberPad0) || (e.Key > VirtualKey.NumberPad9)))
            {
                // If it's not a numeric character, prevent the TextBox from handling the keystroke
                e.Handled = true;
            }
        }
    }
}