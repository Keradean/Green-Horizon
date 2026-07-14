using System.Collections.Generic;

namespace Samil.Quiz
{
    public class QuizQuestion
    {
        public string Text;
        public List<string> Answers = new List<string>();
        public int CorrectAnswerIndex;
    }
}
