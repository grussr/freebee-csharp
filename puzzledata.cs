using System.Reflection.Emit;

namespace FreeBee.Function
{
    public class PuzzleData
    {
        public string letters {get; set;}
        public string center {get;set;}
        public int words { get; set; }
        public int total { get; set; }
        public List<string> wordlist {get; set;}

        public PuzzleData() {
            letters = "";
            center = "";
            words = 0;
            total = 0;
            wordlist = new List<string>();
        }
    }
}