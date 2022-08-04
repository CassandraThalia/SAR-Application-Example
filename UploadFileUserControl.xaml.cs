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
using Microsoft.Win32;
using SAR.ViewModels;
using SAR.ContentDialogs;
using SAR.Repository;
using System.Collections.ObjectModel;
using SAR.Models;
using SAR.Services;
using CsvHelper;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Text;
using System.Diagnostics;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SAR
{
    public sealed partial class UploadFileUserControl : UserControl
    {
        public UploadFileUserControl()
        {
            this.InitializeComponent();
        }

        private async void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".csv");
            //picker.FileTypeFilter.Add(".xls");

            StorageFile spreadsheetFile = await picker.PickSingleFileAsync();
            if (spreadsheetFile != null)
            {
                // Once a file is selected, it will be copied to a local directory and the copy will be handed to our CSV Parsing Service
                // Need to use a stream to copy the CSV information to handle platform constraints
                // Create new file to receive information
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile newFile = await storageFolder.CreateFileAsync(spreadsheetFile.Name, CreationCollisionOption.ReplaceExisting);

                // This is the collection that will received the parse CSV information 
                ObservableCollection<CSVParsedObject> CSVData = new ObservableCollection<CSVParsedObject>();

                try
                {
                    using (IRandomAccessStream filestream = await spreadsheetFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        // We use a reader to read in the file information as a byte stream
                        using (var reader = new DataReader(filestream.GetInputStreamAt(0)))
                        {
                            await reader.LoadAsync((uint)filestream.Size);
                            var buffer = new byte[(int)filestream.Size];
                            reader.ReadBytes(buffer);
                            // Convert the read bytes back to UTF-8 text and append it to the temporarily created file
                            string byteString = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                            File.AppendAllText(newFile.Path, byteString);
                        }

                        // Get the data from the new file once it has been written
                        CSVData = await CSVParserService.GetRecordsFromCSV(newFile);
                    }
                }
                catch (Exception ex)
                {
                    // Display a modal window with the Exception details
                    ContentDialog fileErrorDialog = new ContentDialog()
                    {
                        Title = "File Open Error",
                        Content = ex.Message,
                        PrimaryButtonText = "OK",
                    };

                    // Error handling for multiple modal windows - VERY specific
                    var openedDialogs = VisualTreeHelper.GetOpenPopups(Window.Current);
                    // We check to see if any open modal windows already exist - Platform constraints define only one window can open at a time
                    foreach (var dialog in openedDialogs)
                    {
                        if (dialog.Child is ContentDialog)
                        {
                            return;
                        }

                    }
                    await fileErrorDialog.ShowAsync();
                    File.Delete(newFile.Path);
                    return;
                }


                bool fileIsDeleted = false;
                while (!fileIsDeleted)
                {
                    try
                    {
                        // Delete the temporary file now that we are done with it
                        File.Delete(newFile.Path);
                        if (!File.Exists(newFile.Path))
                        {
                            fileIsDeleted = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(ex.StackTrace);
                        return;
                    }
                }
                
                //Display intermediate Spreadsheet Upload Selector Dialog
                if (CSVData.Count() > 0) {
                    try
                    {
                        SpreadsheetUploadSelectorDialog susd = new SpreadsheetUploadSelectorDialog(CSVData);
                        var result = await susd.ShowAsync();
                        if (result == ContentDialogResult.Secondary)
                        {
                            SpreadsheetTableViewModel.CompareStudentEnrollmentStatus();
                        }
                        

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(ex.StackTrace);
                        ContentDialog studentDisplayErrorDialog = new ContentDialog()
                        {
                            Title = "Student Display Error",
                            Content = ex.Message,
                            PrimaryButtonText = "OK",
                        };

                        await studentDisplayErrorDialog.ShowAsync();
                    }
                }
            }
            else
            {
                // This is displayed if the user cancels their selection or some other error with file selection happens
                ContentDialog fileErrorDialog = new ContentDialog()
                {
                    Title = "File Open Error",
                    Content = "Error opening file or no file selected",
                    PrimaryButtonText = "OK",
                };
                await fileErrorDialog.ShowAsync();
            }
        }
    }
}

//https://docs.microsoft.com/en-us/windows/uwp/files/quickstart-using-file-and-folder-pickers
