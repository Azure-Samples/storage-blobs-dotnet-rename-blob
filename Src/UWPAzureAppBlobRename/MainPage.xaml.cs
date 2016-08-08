using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPAzureAppBlobRename
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ProcessStatus process;
        private ProcessStatus processStatus { get {return process; } set {process=value; ProcessSetting(); } }
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            processStatus = ProcessStatus.Begin;
            this.RefreshFileListview();
            processStatus = ProcessStatus.End;
        }

        /// <summary>
        /// Pick a file to save to the blob storage
        /// 1. open a file picker
        /// 2. delete the old file
        /// 3. upload the new file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSaveFiles_Click(object sender, RoutedEventArgs e)
        {
            processStatus = ProcessStatus.Begin;
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add("*");
            var file =  await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                using (var fileStream = await file.OpenStreamForReadAsync())
                {
                    try
                    {
                        await App.Container.CreateIfNotExistsAsync();
                        var blob = App.Container.GetBlockBlobReference(file.Name);
                        await blob.DeleteIfExistsAsync();
                        await blob.UploadFromStreamAsync(fileStream);
                    }
                    catch (Exception ex)
                    {
                        await ShowMessage(ex.Message);
                    }
                }
            }
            RefreshFileListview();
            processStatus = ProcessStatus.End;
        }

        /// <summary>
        /// Rename the selected file
        /// 1. Show content dialog, get the new name
        /// 2. Copy the file and name it as new name
        /// 3. Delete the old file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnRenameFile_Click(object sender, RoutedEventArgs e)
        {
            processStatus = ProcessStatus.Begin;
            var selected = this.listViewBlobFile.SelectedValue;
            if (selected != null)
            {
                try
                {
                    var item = selected as CloudBlockBlob;
                    string fileName = item.Name;
                    var contentDialog =new RenameContentDialog();
                    contentDialog.FileName = fileName;
                    if (await contentDialog.ShowAsync() == ContentDialogResult.Secondary)
                    {
                        fileName = contentDialog.FileName;
                        await App.Container.CreateIfNotExistsAsync();
                        var blobCopy = App.Container.GetBlockBlobReference(fileName);
                        if (await blobCopy.ExistsAsync())
                        {
                            await ShowMessage($"There is already a file with the same name in this container.");
                        }
                        else
                        {
                            var blob = App.Container.GetBlockBlobReference(item.Name);
                            await blobCopy.StartCopyAsync(blob);
                            await blob.DeleteIfExistsAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    await ShowMessage(ex.Message);
                }
            }
            else
            {
                await ShowMessage("Please select a file to rename.");
            }
            RefreshFileListview();
            processStatus = ProcessStatus.End;
        }

        /// <summary>
        /// Refresh the files in Listview.
        /// </summary>
        private async void RefreshFileListview()
        {
            try
            {
                await App.Container.CreateIfNotExistsAsync();
                BlobContinuationToken token = null;
                var task = App.Container.ListBlobsSegmentedAsync(token);
                var blobsSegmented = await task;
                listViewBlobFile.ItemsSource = blobsSegmented.Results;
            }
            catch(Exception ex)
            {
                await ShowMessage(ex.Message);
            }
        }

        /// <summary>
        /// Set control status according to the process status.
        /// Disable the controls after the process is begin.
        /// Enable the controls after the process is ended.
        /// </summary>
        private void ProcessSetting()
        {
            this.btnRenameFile.IsEnabled = processStatus == ProcessStatus.End;
            this.btnSaveFiles.IsEnabled = processStatus == ProcessStatus.End;
            this.listViewBlobFile.IsEnabled = processStatus == ProcessStatus.End;
        }

        /// <summary>
        /// Show message dialog with a message and an "OK" button.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>         
        private async Task ShowMessage(string message)
        {            
            var dialog = new Windows.UI.Popups.MessageDialog(message);
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("OK") { Id = 0 });
            dialog.DefaultCommandIndex = 0;
            await dialog.ShowAsync();
        }

        private enum ProcessStatus { Begin,End};       
    }
}
