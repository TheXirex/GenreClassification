using Microsoft.ML.Data;

namespace GenreClassification.Models
{
    public class GenrePrediction
    {
        [ColumnName("PredictedLabel")]
        public string Genre { get; set; }
    }
}