using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SmartZipper.Models
{
    public class ZipModel
    {
        public string FolderLocation { get; set; }
        public int CompressionType { get; set; }
        public StorageFolder SelectedFolder { get; set; }
        public IReadOnlyList<StorageFile> SelectedFiles { get; set; }
        public int CompressionMode { get; set; }
        public string StatusMessage { get; set; }
        public StorageFolder FolderToCreateZipIn { get; set; }
    }
}
