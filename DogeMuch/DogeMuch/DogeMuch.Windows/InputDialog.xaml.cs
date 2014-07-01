using Windows.System;

#region

using System.Threading.Tasks;
using Windows.UI.Xaml;

#region

#endregion

#endregion

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace DogeMuch
{
    public sealed partial class InputDialog
    {
        public string InputText;

        public InputDialog(string title, string inputHeader, string primaryButtonText, bool cancelButton = true)
        {
            InitializeComponent();
            Title = title;
            text.Header = inputHeader;
            PrimaryButton.Content = primaryButtonText;
            if (!cancelButton)
                SecondaryButton.Visibility = Visibility.Collapsed;
        }

        public Task<ContentDialogResult> ShowAsync()
        {
            var tsk = new TaskCompletionSource<ContentDialogResult>();
            SecondaryButton.Click += (sender, args) =>
            {
                tsk.TrySetResult(ContentDialogResult.Secondary);
                IsOpen = false;
            };
            PrimaryButton.Click += (sender, args) =>
            {
                InputText = text.Text;
                tsk.TrySetResult(ContentDialogResult.Primary);
                IsOpen = false;
            };
            IsOpen = true;
            return tsk.Task;
        }
    }




    public enum ContentDialogResult
    {
        Primary,
        Secondary
    }
}