using SmartZipper.Commands;
using SmartZipper.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;


namespace SmartZipper.ViewModels
{
    public class UnzipViewModel : INotifyPropertyChanged
    {
        #region Fields
        FileOpenPicker filePicker;
        FolderPicker folderPicker;
        StorageFile zipfile;
        StorageFolder extractToFolder;
        UnzipModel _unzip;
        bool _progress;
        #endregion

        #region Constructors
        public UnzipViewModel(UnzipModel unzip)
        {
            this._unzip = unzip;
            filePicker = InitFilePicker();
            folderPicker = new FolderPicker();
            FileSelectCommand = new UnzipCommand(()=>SelectFile(),()=>true);
            FolderSelectCommand = new UnzipCommand(() => SelectFolder(), () => true);
            ExtractCommand = new UnzipCommand(() => Extract(), () => true);
        } 
        #endregion

        #region Properties
        public string ZipFilePath
        {
            get { return _unzip.ZipFilePath; }
            set
            {
                if (this.zipfile != null && value != this.ZipFilePath)
                {
                    _unzip.ZipFilePath = value;
                    OnPropertyChanged("ZipFilePath");
                }
            }
        }
        public string ZipFileName { get; set; }
        public string ExtractFolderPath
        {
            get { return _unzip.ExtractFolderPath; }
            set
            {
                if (this.zipfile != null && value != this.ExtractFolderPath)
                {
                    _unzip.ExtractFolderPath = value;
                    OnPropertyChanged("ExtractFolderPath");
                }
            }
        }
        public string ExtractFolderName { get; set; }
        public int Status { get; set; }
        public bool InProgress { get { return _progress; }
           set { this._progress = value; OnPropertyChanged("InProgress"); }
        }
        public string ErrorMessage
        {
            get { return _unzip.ErrorMessage; }
            set
            {
                if (this._unzip.ErrorMessage!=value)
                {
                    _unzip.ErrorMessage =value;
                    OnPropertyChanged("ErrorMessage");
                }
            }
        }
        public bool IsSuccess
        {
            get { return _unzip.IsSuccess; }
            set
            {
                if (this.zipfile != null && value != this.IsSuccess)
                {
                    _unzip.IsSuccess = value;
                    OnPropertyChanged("IsSuccess");
                }
            }
        }

        public ICommand FileSelectCommand { get; set; }
        public ICommand FolderSelectCommand { get; set; }
        public ICommand ExtractCommand { get; set; }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged; 
        #endregion

        #region Methods
        public FileOpenPicker InitFilePicker()
        {
            FileOpenPicker f = new FileOpenPicker();
            f.FileTypeFilter.Add(".zip");
            //f.FileTypeFilter.Add(".rar");
            f.ViewMode = PickerViewMode.List;
            return f;
        }

        public async void SelectFile()
        {
            zipfile = await filePicker.PickSingleFileAsync();
            if (zipfile!=null)
            {
                ZipFilePath = zipfile.Path;
            }
            else
            {
                return;
            }
            
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public async void SelectFolder()
        {
            folderPicker.FileTypeFilter.Add("*");
            extractToFolder = await folderPicker.PickSingleFolderAsync();
            if (extractToFolder!=null)
            {
                ExtractFolderPath = extractToFolder.Path;
            }
            else
            {
                return;
            }
            
        }

        public async void Extract()
        {
            if (zipfile != null)
            {
                _unzip.ZipFileName = zipfile.Name;
                string type = zipfile.ContentType;
            }
            else
            {
                ErrorMessage = "No File Selected";
                return;
            }


            string mruToken = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(extractToFolder, extractToFolder.Name);

            Stream stream = await zipfile.OpenStreamForReadAsync();
            try
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    try
                    {
                        StorageFolder folderToSave = await Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(mruToken);
                        IEnumerable<StorageFolder> folders = await folderToSave.GetFoldersAsync();
                        StorageFolder fileNameFolder;
                        string requiredPath = string.Format("{0}\\{1}", folderToSave.Path, zipfile.Name.Replace(".zip", ""));
                        int reqFolderCount = folders.Where(x => x.Path.Contains(requiredPath)).Count();
                        if (reqFolderCount == 0)
                        {
                            fileNameFolder = await folderToSave.CreateFolderAsync(zipfile.Name.Replace(".zip", ""));
                        }
                        else
                        {
                            fileNameFolder = await folderToSave.CreateFolderAsync(zipfile.Name.Replace(".zip", string.Format("({0})", reqFolderCount + 1)));
                        }
                        //StorageFolder fileNameFolder = await folderToSave.CreateFolderAsync(zipfile.Name.Replace(".zip", ""));
                        if (!string.IsNullOrEmpty(fileNameFolder.Path))
                        {
                            _unzip.IsSuccess = false;
                            this.InProgress = true;
                            await Task.Run(() => archive.ExtractToDirectory(fileNameFolder.Path));
                            _unzip.IsSuccess = true;
                            this.InProgress = false;
                            ErrorMessage = "Success Extraction";
                        }

                    }
                    catch (Exception ex)
                    {
                        _unzip.ErrorMessage = ex.Message;
                    }
                }


                using (var archive = new  ZipArchive(stream, ZipArchiveMode.Read))
                {
                    try
                    {
                        StorageFolder folderToSave = await Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(mruToken);
                        IEnumerable<StorageFolder> folders = await folderToSave.GetFoldersAsync();
                        StorageFolder fileNameFolder;
                        string requiredPath = string.Format("{0}\\{1}", folderToSave.Path, zipfile.Name.Replace(".zip", ""));
                        int reqFolderCount = folders.Where(x => x.Path.Contains(requiredPath)).Count();
                        if (reqFolderCount == 0)
                        {
                            fileNameFolder = await folderToSave.CreateFolderAsync(zipfile.Name.Replace(".zip", ""));
                        }
                        else
                        {
                            fileNameFolder = await folderToSave.CreateFolderAsync(zipfile.Name.Replace(".zip", string.Format("({0})", reqFolderCount + 1)));
                        }
                        //StorageFolder fileNameFolder = await folderToSave.CreateFolderAsync(zipfile.Name.Replace(".zip", ""));
                        if (!string.IsNullOrEmpty(fileNameFolder.Path))
                        {
                            _unzip.IsSuccess = false;
                            this.InProgress = true;
                            await Task.Run(() => archive.ExtractToDirectory(fileNameFolder.Path));
                            _unzip.IsSuccess = true;
                            this.InProgress = false;
                            ErrorMessage = "Success Extraction";
                        }

                    }
                    catch (Exception ex)
                    {
                        _unzip.ErrorMessage = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
           
        } 
        #endregion
    }
}
