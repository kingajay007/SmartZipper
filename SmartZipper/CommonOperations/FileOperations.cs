using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SmartZipper.CommonOperations
{
    public class FileOperations
    {
        private FileOpenPicker _filePicker;
        private FolderPicker _folderPicker;
        public FileOperations(FileOpenPicker filePicker)
        {
            _filePicker = filePicker;
        }

        public FileOperations(FolderPicker folderPicker)
        {
            _folderPicker = folderPicker;
        }

        //public async void GetFolder(out StorageFolder folder)
        //{
        //    _folderPicker.FileTypeFilter.Add("*");
        //    _folderPicker.ViewMode = PickerViewMode.List;
        //    folder = await _folderPicker.PickSingleFolderAsync();
            
        //} 
    }
}
