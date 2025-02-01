using System.Collections.Generic;

namespace Api.Models{

    public class Metrics{
        public Transcription transcription;
        public List<WordTiming> word_timings;
        public List<Repetition> repetitions;
        public List<Prolongation> prolongations;
        public List<Block> blocks;
        public float reading_speed;
        public int fluency_score;
        public TextComparison text_comparison;

    }
}