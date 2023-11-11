using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FreeBee.Function
{
    public class generate_puzzle
    {
        private readonly ILogger _logger;
        private List<string> wordList;

        public generate_puzzle(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<generate_puzzle>();
            wordList = File.ReadAllLines("wordlist.txt").ToList();
        }

        [Function("generate_puzzle")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var rng = new Random(int.Parse(DateTime.Now.ToString("yyyyMMdd")));
            if (req.Query["date"] != null) {
                var dateAsInt = 0;
                var success = int.TryParse(req.Query["date"], out dateAsInt);
                if (success) {
                    rng = new Random(dateAsInt);
                }
                else {
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            if (req.Query["random"] != null) {
                rng = new Random();
            }   
            var puzzle = new PuzzleData();
            
            do {
                HashSet<string> letters;
                
                letters = GenerateRandomLetters(rng);
                
                var center = letters.First();
                letters.Remove(letters.First());
                puzzle.letters = string.Join("", letters);
                puzzle.center = center;
                //puzzle.wordlist = GenerateWords("c", new HashSet<string> { "p", "d", "n", "a", "e", "k"});
                puzzle.wordlist = GenerateWords(center, letters);
                puzzle.words = puzzle.wordlist.Count();
                puzzle.total = ScoreWordList(puzzle.wordlist);
            } while (puzzle.total == -1 || puzzle.words > 2000 || puzzle.words < 20);
            var response = req.CreateResponse(HttpStatusCode.OK);
            
            response.WriteAsJsonAsync<PuzzleData>(puzzle);
            return response;
        }

        private HashSet<string> GenerateRandomLetters(Random rng) {
            var result = new HashSet<string>();
            
            for (int i = 0; i < 7; i++) {
                int num = rng.Next(0, 26); // Zero to 25
                char let = (char)('a' + num);
                if (result.Contains(let.ToString())) {
                    i--;
                } 
                else {
                    result.Add(let.ToString());
                }
            }
            return result;
        }

        private List<string> GenerateWords(string centerLetter, HashSet<string> letters) {
            var result = new List<string>();
            letters.Add(centerLetter);
            foreach (var word in wordList) {
                var found = true;
                foreach (var letter in word) {
                    if ((!word.Contains(centerLetter)) || (!letters.Contains(letter.ToString()))) {
                        found = false;
                        break;
                    }
                }
                if (!found)
                {
                    continue;
                }
                result.Add(word);
            }
            letters.Remove(centerLetter);
            return result;
        }
    
        private int ScoreWordList(List<string> words) {
            var total = 0;
            var hasPangram = false;

            foreach (var word in words) {
                var uniqueLetters = word.Distinct();
                var letterCount = word.Length;

                if (letterCount == 4) {
                    total += 1;
                }
                else {
                    total += letterCount;
                }

                if (uniqueLetters.Count() == 7) {
                    total += 7;
                    hasPangram = true;
                }
            }
            if (!hasPangram) {
                return -1;
            }
            return total;
        }
    }
}
