using Microsoft.ML.Data;

namespace ImageClassifier.Infrastructure.DTOs
{
    public class ModelOutput
    {
        [ColumnName("PredictedLabel")]
        public string? PredictedLabel { get; set; }

        public float[]? Score { get; set; }
    }
}
