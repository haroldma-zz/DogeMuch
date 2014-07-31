using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using DogeMuch.Utility;

namespace DogeMuch
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(MyApiKeyBox.Text))
                MessageBox.Show("API key can't be empty", "much error");
             else
            {
                (sender as HyperlinkButton).IsEnabled = false;
                App.Api.ChangeApiKey(MyApiKeyBox.Text);
                string error = null;

                var success = true;
                try
                {
                    var network = await App.Api.GetNetworkForKeyAsync();
                    if (network != "DOGE")
                    {
                        error = string.Format("api key is not for the DOGE network but {0}", network);
                        throw new Exception();
                    }
                }
                catch
                {
                    success = false;
                }

                if (success)
                {
                    App.ApiKey = MyApiKeyBox.Text;
                    Frame.Navigate(typeof (MainPage));
                }
                else
                    MessageBox.Show((error ?? "make sure that the API Key is valid."), "much error");
                (sender as HyperlinkButton).IsEnabled = true;
            }
        }
    }
}
