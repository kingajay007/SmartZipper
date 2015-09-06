using SmartZipper.Commands;
using SmartZipper.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;


namespace SmartZipper.ViewModels
{
    public class ZipViewModel : INotifyPropertyChanged
    {
        #region Fields
        private ZipModel _zipModel;
        private FileOpenPicker _filePicker;
        private FolderPicker _folderPicker;
        

        public event PropertyChangedEventHandler PropertyChanged;

        IReadOnlyList<StorageFile> filesToCompress;
        private string _folderLocation { get; set; }
        private int _compressionType { get; set; }
        private StorageFolder _selectedFolder { get; set; }
        private List<StorageFile> _selectedFiles { get; set; }
        private int _compressionMode { get; set; }
        private string _statusMessage { get; set; }
        private StorageFolder _folderToCreateZipIn { get; set; }
        private bool _progress;
        private CompressionLevel compressionLevel;
        private List<string> source = new List<string> {"Fastest","No Compression","Optimal"};
        
        #endregion
        #region Constructors
        public ZipViewModel(ZipModel zipModel)
        {
            _zipModel = zipModel;
            _filePicker = InitFilePicker();
            _folderPicker = new FolderPicker();
            _folderPicker.ViewMode = PickerViewMode.List;
            _folderPicker.FileTypeFilter.Add("*");

            // commands initialized
            ZipCommand = new ZipCommand(()=>Compress(),()=>true);
            FolderSelectCommand = new ZipCommand(()=> SelectFolderToZip(),()=>true);
            FilesSelectCommand = new ZipCommand(() => SelectMultipleFile(), () => true);
            //CompressionLevelChange = new ZipCommand(()=>SetCompressionLevel(SelectedItem),()=>true);

        }
        #endregion
        #region Properties
        public string FolderLocation
        { get { return _zipModel.FolderLocation; }
            set
            {
                if (value!=_zipModel.FolderLocation)
                {
                    _zipModel.FolderLocation = value;
                    OnPropertyChanged("FolderLocation");
                }
            }
        }
        public int CompressionType
        {
            get { return _zipModel.CompressionType; }
            set
            {
                if (value != _zipModel.CompressionType)
                {
                    _zipModel.CompressionType = value;
                    OnPropertyChanged("CompressionType");
                }
            }
        }
        public StorageFolder SelectedFolder
        {
            get { return _zipModel.SelectedFolder; }
            set
            {
                if (value!=_zipModel.SelectedFolder)
                {
                    _zipModel.SelectedFolder = value;
                    OnPropertyChanged("SelectedFolder");
                } } }
        public IReadOnlyList<StorageFile> SelectedFiles
        {
            get { return _zipModel.SelectedFiles; }
            set
            {
                if (value !=_zipModel.SelectedFiles)
                {
                    _zipModel.SelectedFiles = value;
                }
            } }
        public int CompressionMode
        {
            get { return _zipModel.CompressionMode; }
            set
            {
                if (value!=_zipModel.CompressionMode)
                {
                    _zipModel.CompressionMode = value;
                    OnPropertyChanged("CompressionMode");
                } } }
        public string StatusMessage
        {
            get { return _zipModel.StatusMessage; }
            set
            {
                if (value!=_zipModel.StatusMessage)
                {
                    _zipModel.StatusMessage = value;
                    OnPropertyChanged("StatusMessage");
                }
            } }
        public StorageFolder FolderToCreateZipIn
        {
            get { return _zipModel.FolderToCreateZipIn; }
            set
            {
                if (value!=_zipModel.FolderToCreateZipIn)
                {
                    _zipModel.FolderToCreateZipIn = value;
                    OnPropertyChanged("FolderToCreateZipIn");
                }
            } }
        public bool InProgress
        {
            get { return _progress; }
            set { this._progress = value; OnPropertyChanged("InProgress"); }
        }
        public CompressionLevel CompressionLevelSelected {
            get { return compressionLevel; }
            set { value = compressionLevel; OnPropertyChanged("CompressionLevelSelected"); }
        }
        public List<String> Source { get { return source; } }
        private string _selectedItem = "Fastest";
        public string SelectedItem
        {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
                SetCompressionLevel(_selectedItem);
            } // NotifyPropertyChanged
            
        }

        public ICommand FilesSelectCommand { get; set; }
        public ICommand FolderSelectCommand { get; set; }
        public ICommand ZipCommand { get; set; }
        //public ICommand CompressionLevelChange { get; set; }

        #endregion
        #region Events
        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
        #region Methods
        /// <summary>
        /// method to initialize file picker
        /// </summary>
        /// <returns></returns>
        public FileOpenPicker InitFilePicker()
        {
            FileOpenPicker f = new FileOpenPicker();
            f.FileTypeFilter.Add("*");
            f.ViewMode = PickerViewMode.List;
            return f;
        }

        /// <summary>
        /// method to select multiple files 
        /// </summary>
        public async void SelectMultipleFile()
        {
            //return;
            SelectedFiles = await _filePicker.PickMultipleFilesAsync();
            if (SelectedFiles != null)
            {
                StorageFile file = SelectedFiles.FirstOrDefault();
                SelectedFolder = await file.GetParentAsync();
                if (SelectedFolder==null)
                {
                    SelectedFolder = await _folderPicker.PickSingleFolderAsync();
                }
                //SelectedFolder = new StorageFolder();
                FolderLocation = SelectedFolder.Path;
                SetFilesToZip(SelectedFiles);
            }
            else
            {
                return;
            }

        }

        /// <summary>
        /// method to select folder to zip
        /// </summary>
        public async void SelectFolderToZip()
        {
            if (_folderPicker != null)
            {
                SelectedFolder = await _folderPicker.PickSingleFolderAsync();
                FolderLocation = SelectedFolder.Path;
                SetFilesToZip(SelectedFolder);
            }
            else
            {
                return;
            }

        }

        private  void SetFilesToZip(IReadOnlyList<StorageFile> selectedFiles)
        {
            filesToCompress = selectedFiles;
        }

        private async void SetFilesToZip(StorageFolder selectedFolder)
        {
            filesToCompress = await GetStorageFiles(selectedFolder as IStorageItem);
        }

        private void SetCompressionLevel(string compressionMode) {
            switch (compressionMode)
            {
                case "Fastest": CompressionLevelSelected = CompressionLevel.Fastest;
                    break;
                case "No Compression":
                    CompressionLevelSelected = CompressionLevel.NoCompression;
                    break;
                case "Optimal":
                    CompressionLevelSelected = CompressionLevel.Optimal;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// method to compress files 
        /// </summary>
        public async void Compress()
        {
            // Retrieve files to compress
            

            // Created new file to store compressed files
            //This will create a file under the selected folder in the name “Compressed.zip”
            StorageFile zipFile = await SelectedFolder.CreateFileAsync("Compressed.zip", CreationCollisionOption.ReplaceExisting);

            // Create stream to compress files in memory (ZipArchive can't stream to an IRandomAccessStream, see
            // http://social.msdn.microsoft.com/Forums/en-US/winappswithcsharp/thread/62541424-ba7d-43d3-9585-1fe53dc7d9e2
            // for details on this issue)
            using (MemoryStream zipMemoryStream = new MemoryStream())
            {
                InProgress = true;
                // Create zip archive
                using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create))
                {
                    // For each file to compress...
                    foreach (StorageFile fileToCompress in filesToCompress)
                    {
                        BasicProperties fileProp = await fileToCompress.GetBasicPropertiesAsync();
                        if (fileProp.Size>0)
                        {
                            //Read the contents of the file
                            byte[] buffer = WindowsRuntimeBufferExtensions.ToArray(await FileIO.ReadBufferAsync(fileToCompress));

                            string fileEntry = fileToCompress.Path.Replace(zipFile.Path.Replace(zipFile.Name, string.Empty), string.Empty);
                            // Create a zip archive entry
                            ZipArchiveEntry entry = zipArchive.CreateEntry(fileEntry,CompressionLevelSelected);

                            // And write the contents to it
                            using (Stream entryStream = entry.Open())
                            {
                                await entryStream.WriteAsync(buffer, 0, buffer.Length);
                            }
                        }
                    }
                }

                using (IRandomAccessStream zipStream = await zipFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // Write compressed data from memory to file
                    using (Stream outstream = zipStream.AsStreamForWrite())
                    {
                        byte[] buffer = zipMemoryStream.ToArray();
                        outstream.Write(buffer, 0, buffer.Length);
                        outstream.Flush();
                    }
                }

                InProgress = false;
                StatusMessage = "Files Zipped...";
            }

        }

        async Task<List<StorageFile>> GetStorageFiles(IStorageItem storageItem)
        {

            List<StorageFile> storageFileList = new List<StorageFile>();
            // Gets the items under the selected folder (Storage Item)
            IReadOnlyList<IStorageItem> items = await (storageItem as StorageFolder).GetItemsAsync();
            foreach (IStorageItem item in items)
            {
                switch (item.Attributes)
                {
                    case Windows.Storage.FileAttributes.Directory:
                        // If the item is a directory under the selected folder, then retrieve the files under the directory by calling the same function recursively
                        List<StorageFile> temp = await GetStorageFiles(item);
                        // Copy the files under the directory to the storage file list
                        Copy(temp, storageFileList);
                        break;
                    default:
                        // If the item is a file, Add the item to the storage file list
                        storageFileList.Add(item as StorageFile);
                        break;
                }
            }
            // Return storage file list for compression
            return storageFileList;
        }

        private void Copy(List<StorageFile> source, List<StorageFile> destination)
        {
            // For each file item present under the directory copy it to the destination storage file list
            foreach (StorageFile file in source)
            {
                destination.Add(file);
            }
        }

        #endregion
    }
}
