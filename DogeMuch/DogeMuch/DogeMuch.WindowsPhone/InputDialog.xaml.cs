#region

using Windows.UI.Xaml.Controls;

#endregion

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace DogeMuch
{
    public sealed partial class InputDialog : ContentDialog
    {
        public InputDialog(string title, string inputHeader, string primaryButtonText, bool cancelButton = true)
        {
            InitializeComponent();
            FullSizeDesired = true;
            Title = title.ToUpper();
            text.Header = inputHeader;
            primaryButtonText = primaryButtonText;
            if (!cancelButton)
                SecondaryButtonText = "";
        }

        public string InputText;
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            InputText = text.Text;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}