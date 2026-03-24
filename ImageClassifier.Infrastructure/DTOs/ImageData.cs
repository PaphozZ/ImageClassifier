using System;
namespace ImageClassifier.Infrastructure.DTOs
{
    public class ImageData
    {
        public byte[]? ImageBytes { get; set; }
        public string? Label { get; set; }
    }
}
