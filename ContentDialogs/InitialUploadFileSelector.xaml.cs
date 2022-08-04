using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SAR.ContentDialogs
{
    public sealed partial class InitialUploadFileSelector : ContentDialog
    {
        private StorageFile ProgramFile { get; set; }
        private StorageFile CourseFile { get; set; }
        private StorageFile InstructorFile { get; set; }
        private StorageFile StudentFile { get; set; }
        private StorageFile AuthenticationFile { get; set; }
        private StorageFile ProgramCourseFile { get; set; }
        private StorageFile InstructorCourseFile { get; set; }
        private StorageFile InstructorStudentFile { get; set; }
        private StorageFile CourseStudentFile { get; set; }
        public InitialUploadFileSelector()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Get a confirmation that the proper files are picked
            MessageDialog confirmDialog = new MessageDialog("Warning: This operation will result in substantial changes to the database. Are you certain you have selected the proper files for the proper fields?", "Warning");
            var confirmCommand = new UICommand("Confirm");
            var cancelCommand = new UICommand("Cancel");
            confirmDialog.Options = MessageDialogOptions.None;
            confirmDialog.Commands.Add(confirmCommand);
            confirmDialog.Commands.Add(cancelCommand);

            // Prevent immediate closure of content dialog
            var deferral = args.GetDeferral();
            args.Cancel = true;
            var result = await confirmDialog.ShowAsync();
            deferral.Complete();

            // If they are sure, execute. If not, do nothing
            if (result == confirmCommand)
            {
                // Make this content dialog go away
                this.Hide();
                ExecuteUpload();

            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        // There is a lot of duplication here that can likely be simplified in future iterations
        private async void ProgramButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".csv");
            ProgramFile = await picker.PickSingleFileAsync();
            if (ProgramFile != null)
            {
                ProgramFileName.Text = ProgramFile.Name;
                ProgramFile = await CopyFileToLocalDirectory(ProgramFile);
            }
        }

        private async void CourseButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".csv");
            CourseFile = await picker.PickSingleFileAsync();
            if (CourseFile != null)
            {
                CourseFileName.Text = CourseFile.Name;
                CourseFile = await CopyFileToLocalDirectory(CourseFile);
            }
        }

        private async void InstructorButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".csv");
            InstructorFile = await picker.PickSingleFileAsync();
            if (InstructorFile != null)
            {
                InstructorFileName.Text = InstructorFile.Name;
                InstructorFile = await CopyFileToLocalDirectory(InstructorFile);
            }
        }

        private async void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".csv");
            StudentFile = await picker.PickSingleFileAsync();
            if (StudentFile != null)
            {
                StudentFileName.Text = StudentFile.Name;
                StudentFile = await CopyFileToLocalDirectory(StudentFile);
            }
        }

        private async void AuthenticationButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".csv");
            AuthenticationFile = await picker.PickSingleFileAsync();
            if (AuthenticationFile != null)
            {
                AuthenticationFileName.Text = AuthenticationFile.Name;
                AuthenticationFile = await CopyFileToLocalDirectory(AuthenticationFile);
            }
        }

        private async void ProgramCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".csv");
            ProgramCourseFile = await picker.PickSingleFileAsync();
            if (ProgramCourseFile != null)
            {
                ProgramCourseFileName.Text = ProgramCourseFile.Name;
                ProgramCourseFile = await CopyFileToLocalDirectory(ProgramCourseFile);
            }
        }

        private async void InstructorCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".csv");
            InstructorCourseFile = await picker.PickSingleFileAsync();
            if (InstructorCourseFile != null)
            {
                InstructorCourseFileName.Text = InstructorCourseFile.Name;
                InstructorCourseFile = await CopyFileToLocalDirectory(InstructorCourseFile);

            }
        }

        private async void InstructorStudentButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".csv");
            InstructorStudentFile = await picker.PickSingleFileAsync();
            if (InstructorStudentFile != null)
            {
                InstructorStudentFileName.Text = InstructorStudentFile.Name;
                InstructorStudentFile = await CopyFileToLocalDirectory(InstructorStudentFile);
            }
        }

        private async void CourseStudentButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".csv");
            CourseStudentFile = await picker.PickSingleFileAsync();
            if (CourseStudentFile != null)
            {
                CourseStudentFileName.Text = CourseStudentFile.Name;
                CourseStudentFile = await CopyFileToLocalDirectory(CourseStudentFile);
            }
        }

    

        private async void ExecuteUpload()
        {
            // boolean to determine whether upload was successful
            bool isSuccessful = true;
            // Perform null check for every property
            if (ProgramFile != null)
            {
                isSuccessful = await ExecuteUploadByTable("sar_db.dbo.program", ProgramFile.Path);
                DeleteFile(ProgramFile);
            }
            if (CourseFile != null)
            {
                isSuccessful = await ExecuteUploadByTable("sar_db.dbo.course", CourseFile.Path);
                DeleteFile(CourseFile);
            }
            if (InstructorFile != null)
            {
                isSuccessful = await ExecuteUploadByTable("sar_db.dbo.instructor", InstructorFile.Path);
                DeleteFile(InstructorFile);
            }
            if (StudentFile != null)
            {
                isSuccessful = await ExecuteUploadByTable("sar_db.dbo.student", StudentFile.Path);
                DeleteFile(StudentFile);
            }
            if (AuthenticationFile != null)
            {
                isSuccessful = await ExecuteUploadByTable("sar_db.dbo.authentication", AuthenticationFile.Path);
                DeleteFile(AuthenticationFile);
            }
            if (ProgramCourseFile != null)
            {
                isSuccessful = await ExecuteUploadByTable("sar_db.dbo.program_course", ProgramCourseFile.Path);
                DeleteFile(ProgramCourseFile);
            }
            if (InstructorCourseFile != null)
            {
                isSuccessful = await ExecuteUploadByTable("sar_db.dbo.instructor_course", InstructorCourseFile.Path);
                DeleteFile(InstructorCourseFile);
            }
            if (InstructorStudentFile != null)
            {
                isSuccessful = await ExecuteUploadByTable("sar_db.dbo.instructor_student", InstructorStudentFile.Path);
                DeleteFile(InstructorStudentFile);
            }
            if (CourseStudentFile != null)
            {
                isSuccessful = await ExecuteUploadByTable("sar_db.dbo.course_student", CourseStudentFile.Path);
                DeleteFile(CourseStudentFile);
            }
            if (isSuccessful)
            {
                ContentDialog uploadDialog = new ContentDialog()
                {
                    Title = "Successful Upload",
                    Content = "Academic Information for the upcoming year has been successfully uploaded!",
                    PrimaryButtonText = "OK",
                };
                await uploadDialog.ShowAsync();
            }
        }

        private async Task<bool> ExecuteUploadByTable(string tableName, string filePath)
        {
           
            string execQuery = "EXEC sp_bulkInsertIntoTableCSVwindows '" + tableName + "', '" + filePath + "'";
            string connectionString = @"Server=localhost;Database=sar_db;Trusted_Connection=True;";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = execQuery;
                            cmd.ExecuteNonQuery();

                        }
                    }
                    conn.Close();
                }
                return true;
            }
            catch (SqlException eSql)
            {
                string trimmedTableName = tableName.Substring(11, (tableName.Length - 11));
                Debug.WriteLine("Exception: " + eSql.Message);
                Debug.WriteLine(eSql.StackTrace);
                ContentDialog fileErrorDialog = new ContentDialog()
                {
                    Title = "Error Uploading Information",
                    Content = "There was an error trying to upload " + filePath + " into the " + trimmedTableName + " table.\n" + eSql.Message,
                    PrimaryButtonText = "OK",
                };

                await fileErrorDialog.ShowAsync();
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return false;
        }

        private async Task<StorageFile> CopyFileToLocalDirectory(StorageFile file)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile newFile = await storageFolder.CreateFileAsync(file.Name, CreationCollisionOption.ReplaceExisting);
            using (IRandomAccessStream filestream = await file.OpenAsync(FileAccessMode.ReadWrite))
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

                
            }
            return newFile;
        }

        private void DeleteFile(StorageFile file)
        {
            if (File.Exists(file.Path))
            {
                File.Delete(file.Path);
            }
        }
    }
}
