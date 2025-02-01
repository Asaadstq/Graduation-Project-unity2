namespace Api.Models{

    public class TextComparison
    {
        public float similarity_ratio;
        public float word_accuracy;
        public int correct_words;
        public int missing_words;
        public int extra_words;
        public bool success;
    }
    
}