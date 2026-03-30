namespace ImageClassifier.Core.Models;

public class ModelItemModel
{
    public string LabelName { get; set; }
    public string FileName { get; set; }
    public DateTime LastModified { get; set; }

    public ModelItemModel(string labelName) 
    {
        LabelName = labelName;
        FileName = LabelName + ".zip";
        LastModified = DateTime.Now;
    }
}