using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageClassifier.ViewModel.ViewModels
{
    public class ImageItemViewModel
    {
        public string FileName { get; set; } = "FileName";
        public string FilePath { get; set; } = "FilePath";
        public ImageSource? FilePreview { get; set; }
        public string? Extension { get; set; }
    }
}
