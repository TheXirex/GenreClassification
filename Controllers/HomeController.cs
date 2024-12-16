using GenreClassification.Classification;
using GenreClassification.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GenreClassification.Controllers
{
    public class HomeController : Controller
    {
        private static string dataPath = Path.Combine(Environment.CurrentDirectory, "Data", "book_data_goodread.csv");

        public IActionResult Index()
        {
            List<string> allGenres = new List<string>();
            if (System.IO.File.Exists(dataPath))
            {
                var lines = System.IO.File.ReadAllLines(dataPath).Skip(1);
                allGenres = lines
                    .Select(l => l.Split(';')[1])
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();
            }
            ViewData["AllGenres"] = allGenres;
            return View();
        }

        [HttpPost]
        public IActionResult Predict([FromBody] PredictionViewModel input)
        {
            try
            {
                var bookSample = new Book
                {
                    Title = input.Title
                };

                GenreClassificator classificator = new GenreClassificator();
                var prediction = classificator.GetPredict(bookSample);

                return Json(new { Genre = prediction });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AddToDatabase([FromBody] PredictionViewModel input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.Title) || string.IsNullOrWhiteSpace(input.Genre))
                    return BadRequest(new { error = "Invalid data" });

                var line = $"{input.Title};{input.Genre}";
                System.IO.File.AppendAllLines(dataPath, new[] { line });

                return Ok(new { message = "Book added to the database." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteFromDatabase([FromBody] int lineIndex)
        {
            try
            {
                if (!System.IO.File.Exists(dataPath))
                    return BadRequest(new { error = "File not found." });

                var lines = System.IO.File.ReadAllLines(dataPath).ToList();
                int actualLine = lineIndex + 1;
                if (actualLine < 1 || actualLine >= lines.Count)
                    return BadRequest(new { error = "Invalid line index." });

                lines.RemoveAt(actualLine);
                System.IO.File.WriteAllLines(dataPath, lines);

                return Ok(new { message = "Book deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult List(string search, string genreFilter, string sort, int page = 1)
        {
            const int PageSize = 20;
            var books = new List<(int Index, Book BookData)>();

            if (System.IO.File.Exists(dataPath))
            {
                var lines = System.IO.File.ReadAllLines(dataPath);
                var dataLines = lines.Skip(1).ToList();
                for (int i = 0; i < dataLines.Count; i++)
                {
                    var line = dataLines[i];
                    var parts = line.Split(';');
                    if (parts.Length >= 2)
                    {
                        books.Add((i, new Book { Title = parts[0], Genre = parts[1] }));
                    }
                }
            }

            if (!string.IsNullOrEmpty(genreFilter) && genreFilter != "All")
            {
                books = books.Where(b => b.BookData.Genre == genreFilter).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                books = books.Where(b => b.BookData.Title.ToLower().Contains(search)
                                      || b.BookData.Genre.ToLower().Contains(search)).ToList();
            }

            if (sort == "desc")
            {
                books.Reverse();
            }

            int totalBooks = books.Count;
            int totalPages = (int)Math.Ceiling(totalBooks / (double)PageSize);

            books = books.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            ViewData["CurrentSearch"] = search;
            ViewData["CurrentSort"] = sort;
            ViewData["CurrentGenre"] = genreFilter;
            ViewData["AllGenres"] = books.Select(b => b.BookData.Genre).Distinct().OrderBy(g => g).ToList();
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(books);
        }
        [HttpGet]
        public IActionResult GetAllGenres()
        {
            var genres = System.IO.File
                .ReadAllLines(dataPath)
                .Skip(1)
                .Select(line => line.Split(';')[1])
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            return Json(genres);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
