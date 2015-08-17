using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.Media.Devices;
using Windows.Graphics.Display;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartZipper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        StorageFile file;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fop = new FileOpenPicker();
            
            
            fop.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            fop.ViewMode = PickerViewMode.List;
            fop.FileTypeFilter.Add(".zip");
            file = await fop.PickSingleFileAsync();
            
           

        }

        private async void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {

            if (file != null)
            {
                lblFileName.Text = file.Name + " Selected";
            }
            else
            {
                lblFileName.Text = "No File Selected";
            }

            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.FileTypeFilter.Add("*");
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            string mruToken = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(folder, folder.Name);

            //StorageFolder storageFolder = await DownloadsFolder.CreateFolderAsync("iii");
            //StorageFolder storageFolder = await DownloadsFolder.CreateFolderAsync("Sample");
            Stream stream = await file.OpenStreamForReadAsync();

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                
                    try
                    {
                    StorageFolder folderToSave = await Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(mruToken);
                    StorageFolder fileNameFolder =await folderToSave.CreateFolderAsync(file.Name.Replace(".zip",""));
                    if (!string.IsNullOrEmpty(fileNameFolder.Path))
                    {
                        progressRing.IsActive = true;
                        await Task.Run(() => archive.ExtractToDirectory(fileNameFolder.Path));
                        lblFileName.Text = "Files extracted successfully...";
                        progressRing.IsActive = false;
                    }
                    
                    }
                    catch (Exception ex)
                    {
                        lblFileName.Text = ex.Message;
                    }
            }


           
        }

        
    }
}
