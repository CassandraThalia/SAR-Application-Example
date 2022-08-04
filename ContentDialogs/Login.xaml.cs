using SAR.Models;
using SAR.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
// https://docs.microsoft.com/en-us/uwp/api/Windows.UI.Xaml.Controls.ContentDialog?redirectedfrom=MSDN&view=winrt-22000

namespace SAR.ContentDialogs
{
    public enum SignInResult
    {
        SignInOK,
        SignInFail,
        SignInError,
        SignInCancel,
        Nothing
    }
    public sealed partial class Login : ContentDialog
    {
        public SignInResult Result { get; private set; }

        public User User { get; set; }
        
        public Login()
        {
            this.InitializeComponent();

            this.Opened += SignInContentDialog_Opened;
            this.Closing += SignInContentDialog_Closing;
        }

        private bool UserLoginPassed()
        {
            // Ensure the user name and password fields aren't empty. If a required field
            // is empty, set args.Cancel = true to keep the dialog open.
            if (string.IsNullOrEmpty(wNumLoginBox.Text))
            {                
                errMsg.Text = "User name is required.";
                return false;
            }
            else if (string.IsNullOrEmpty(userPasswordBox.Password))
            {                
                errMsg.Text = "Password is required.";
                return false;
            }
            else
            {
                // use user's entry to instantiate the object
                // remove the "w" first
                string wNum = wNumLoginBox.Text.Remove(0, 1);

                string pWordInput = userPasswordBox.Password;

                // to verify the credential: 0 = Good; 1 = Incorrect Login; 2 = Error
                int isVerified = Repository.UserRepository.CheckCredential(wNum, pWordInput);
                if (isVerified == 0)
                {
                    Result = SignInResult.SignInOK;

                    // when it's verified, instantiate the User object
                    User = new User(wNum);

                    return true;
                }
                else if (isVerified == 1)
                {                    
                    Result = SignInResult.SignInFail;
                    errMsg.Text = "Wrong username or password.";
                    return false;
                }
                else
                {
                    Result = SignInResult.SignInError;
                    return false;
                }
            }
        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // call the authentication process and get back the result
            if (!UserLoginPassed())
            {
                // set args.Cancel = true to keep the dialog open.
                args.Cancel = true;
            }

            //// testing, get hashed string
            //List<string> strings = new List<string>();
            //strings.Add("Scarfone123");
            //strings.Add("Crocker123");
            //strings.Add("Mogensen123");
            //strings.Add("Cunningham123");

            //List<string> hashed = Repository.UserRepository.CreateHashedStrings(strings);
            //foreach (string s in hashed)
            //{
            //    Debug.WriteLine(s);
            //}            
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // User clicked Cancel, ESC, or the system back button.
            args.Cancel = true;
            Result = SignInResult.SignInCancel;

            wNumLoginBox.Text = String.Empty;
            userPasswordBox.Password = String.Empty;
            errMsg.Text = String.Empty;

            //CoreApplication.Exit();
        }

        private void SignInContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            Result = SignInResult.Nothing;

            // hardcode a default wNum for quick process for the other scenarios
            //wNumLoginBox.Text = "W9990001";
        }

        private void SignInContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            //If sign in was successful

            if (Result == SignInResult.SignInOK)
            {
                MainPage.HashedPass = UserRepository.GetHashedStr(userPasswordBox.Password);
            }
        }

        private void userPasswordBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // when the user press Enter key, run the authentication process
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (UserLoginPassed())
                {
                    this.Hide();
                }
            }
        }
    }
}
