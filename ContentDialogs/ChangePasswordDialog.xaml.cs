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
using SAR.Models;
using SAR.Repository;
using Windows.UI.Popups;
using System.Diagnostics;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SAR.ContentDialogs
{
    public sealed partial class ChangePasswordDialog : ContentDialog
    {
        public User User { get; set; }
        public string OldPass { get; set; }
        public ChangePasswordDialog(User user, string oldPass)
        {
            this.InitializeComponent();
            User = user;
            OldPass = oldPass;
            userName.Text = "Logged in as: " + User.FullName;
        }

        private bool CheckForMatch()
        {
            try
            {
                string firstBox = newPass.Password;
                string secondBox = confirmPass.Password;
                if (firstBox == secondBox)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error comparing passwords: " + e);
            }
            return false;
        }


        //Save and Continue button
        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                //Hash new password
                string pass = UserRepository.GetHashedStr(newPass.Password);

                //Check if either box has been left blank
                if (newPass.Password == "" || confirmPass.Password == "")
                {
                    var deferral = args.GetDeferral();
                    args.Cancel = true;
                    errMsg.Text = "Fields cannot be left empty";
                    deferral.Complete();
                }
                //Check if they actually entered a different password
                else if (pass == OldPass)
                {
                    var deferral = args.GetDeferral();
                    args.Cancel = true;
                    errMsg.Text = "New password must be different from temp password";
                    deferral.Complete();
                }
                //Check if passwords match
                else if (!CheckForMatch())
                {
                    var deferral = args.GetDeferral();
                    args.Cancel = true;
                    errMsg.Text = "Passwords do not match";
                    deferral.Complete();
                }
                //If passwords have been populated and they match, proceed with save
                else if (CheckForMatch())
                {
                    //Send new hashed password to Database
                    UserRepository.UpdatePassword(User, pass);
                    //Show confirmation dialog
                    MessageDialog messageDialog = new MessageDialog("You successfully updated your password!", "Password Saved");
                    await messageDialog.ShowAsync();
                }
            }
            catch (Exception e)
            {
                MessageDialog messageDialog = new MessageDialog("Error saving password: " + e, "Save Error");
                await messageDialog.ShowAsync();
            }
        }

        //private void confirmPass_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (!CheckForMatch())
        //    {
        //        errMsg.Text = "Passwords do not match";
        //    }
        //    else if (CheckForMatch())
        //    {
        //        errMsg.Text = "";
        //    }
        //}
    }
}
