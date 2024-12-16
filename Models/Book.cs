using Microsoft.ML.Data;

namespace GenreClassification.Models
{
    public class Book
    {
        [LoadColumn(0)] public string Title { get; set; }
        [LoadColumn(1)] public string Genre { get; set; }
    }
}
