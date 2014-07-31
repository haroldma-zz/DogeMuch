#region

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using System;
using Windows.Media.Capture;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using DogeMuch.Model;
using DogeMuch.Utility;
using DogeMuch.ViewModel;
using ZXing;
using ZXing.Common;

#endregion

namespace DogeMuch
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public readonly MainPageViewModel Vm = new MainPageViewModel();
        private bool _refresh;

        public MainPage()
        {
            InitializeComponent();
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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _refresh = true;
            Vm.LoadDataAsync();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var stack = (sender as Button).Parent as StackPanel;
            var pinBox = stack.FindName("MyDogePin") as TextBox;
            var sentToBox = stack.FindName("MySentToAddress") as TextBox;
            var amountBox =
                ((stack.FindName("Panel") as Grid).FindName("DogePanel") as StackPanel).FindName("MySentAmount")
                    as TextBox;

            if (string.IsNullOrEmpty(amountBox.Text))
                MessageBox.Show("plz input doge amount!", "much error");
            else if (string.IsNullOrEmpty(sentToBox.Text))
                MessageBox.Show("plz input sent to address!", "much error");
            else if (string.IsNullOrEmpty(pinBox.Text))
                MessageBox.Show("plz input pin!", "much error");
            else
            {
                if (pinBox.Text.Length < 8)
                {
                    MessageBox.Show("Pin must be 8 characters or longer.");
                    return;
                }

                double doge;
                try
                {
                    doge = double.Parse(amountBox.Text);
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
                    await App.Api.WithdrawAsync(doge, pinBox.Text, sentToBox.Text);
                    amountBox.Text = "";
                    sentToBox.Text = "";
                    pinBox.Text = "";
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
            new QrFlyout(addrs.Address).ShowIndependent();
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new CameraCaptureUI();
                dialog.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.HighestAvailable;

                var file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);
                if (file == null) return;
                using (var stream = await file.OpenReadAsync())
                {
                    // initialize with 1,1 to get the current size of the image
                    var writeableBmp = new WriteableBitmap(1, 1);
                    writeableBmp.SetSource(stream);
                    // and create it again because otherwise the WB isn't fully initialized and decoding
                    // results in a IndexOutOfRange
                    writeableBmp = new WriteableBitmap(writeableBmp.PixelWidth, writeableBmp.PixelHeight);
                    stream.Seek(0);
                    writeableBmp.SetSource(stream);

                    var result = ScanBitmap(writeableBmp);
                    if (result != null && result.BarcodeFormat == BarcodeFormat.QR_CODE)
                    {
                        var sentToBox =
                            ((sender as HyperlinkButton).Parent as StackPanel).FindName("MySentToAddress") as
                                TextBox;
                        sentToBox.Text = result.Text.Replace("dogecoin:", "");
                    }
                    else
                    {
                        MessageBox.Show("couldn't decode bar code from image", "much sorry");
                    }
                }
            }
            catch
            {
                MessageBox.Show("couldn't open camera", "much error");
            }
        }

        private Result ScanBitmap(WriteableBitmap writeableBmp)
        {
            var barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    // restrict to one or more supported types, if necessary
                    PossibleFormats = new[]
                    {
                        BarcodeFormat.QR_CODE
                    }
                }
            };
            var result = barcodeReader.Decode(writeableBmp);

            return result;
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            var sentToBox =
                ((sender as HyperlinkButton).Parent as StackPanel).FindName("MySentToAddress") as
                    TextBox;
            sentToBox.Text = "DKCYsWR8eqXz2sZBfcpJDgp4GpfcSVeQU3";
        }

        private async void NewAddressButton_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog("new address", "label (optional)", "Plz make");
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

        private void MySentAmount_KeyDown(object sender, KeyRoutedEventArgs e)
        {
           ForceNumOnly(e);
        }

        private void ForceNumOnly(KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Back && ((e.Key < VirtualKey.Number0) || (e.Key > VirtualKey.Number9)) &&
                ((e.Key < VirtualKey.NumberPad0) || (e.Key > VirtualKey.NumberPad9)) && (e.Key != VirtualKey.Decimal))
            {
                // If it's not a numeric character, prevent the TextBox from handling the keystroke
                e.Handled = true;
            }
        }
    }
}