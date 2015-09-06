using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartZipper.Models
{
    public class UnzipModel
    {
        public UnzipModel()
        {
            
        }

        public string ZipFilePath { get; set; }
        public string ZipFileName { get; set; }
        public string ExtractFolderPath { get; set; }
        public string ExtractFolderName { get; set; }
        public int Status { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
    }
}
