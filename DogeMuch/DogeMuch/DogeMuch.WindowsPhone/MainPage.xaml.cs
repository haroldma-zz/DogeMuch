#region

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using System;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using DogeMuch.Utility;
using DogeMuch.ViewModel;

#endregion

namespace DogeMuch
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
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
                int pin;
                double doge;
                try
                {
                    pin = int.Parse(MyDogePin.Text);
                }
                catch
                {
                    MessageBox.Show("wow, pin must be number", "much error");
                    return;
                }
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
                    await App.Api.WithdrawAsync(doge, pin, MySentToAddress.Text);
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
            var addrs = e.ClickedItem as string;
            new QrDialog(addrs).ShowAsync();
        }

        /*private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (QrScanner));
        }*/

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            MySentToAddress.Text = "DTvFrriBRbCvjs2FuimZs4uvz36kzCpnzu";
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
                Vm.LoadDataAsync();
            }
            catch (DogeException dogeException)
            {
                MessageBox.Show(dogeException.InnerException.Message, dogeException.Message);
            }
        }

        private void MySentAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var amm = double.Parse((sender as TextBox).Text);
                var fee = amm*.005;
                feeBlock.Text = string.Format("{0:0.00####}", amm - fee) + " After 0.5% DogeAPI fee.";
            }
            catch
            {
                feeBlock.Text = "0.000000 After 0.5% DogeAPI fee.";
            }
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

        private
            void MySentAmount_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            ForceNumOnly(e);
        }

        private void MyDogePin_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            ForceNumOnly(e);
        }
    }
}