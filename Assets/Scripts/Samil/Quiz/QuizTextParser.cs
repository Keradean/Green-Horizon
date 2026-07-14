using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Samil.Quiz
{
    // Bemerkung: Regex mit Claude gemacht
    public static class QuizTextParser
    {
        private static readonly Regex QuestionLine = new Regex(@"^\s*\d+\.\s*(.+)$");
        private static readonly Regex AnswerLine    = new Regex(@"^\s*[A-D]\)\s*(.+)$");
        private static readonly Regex CorrectMarker = new Regex(@"\s*\(richtig\)\s*$", RegexOptions.IgnoreCase);

        public static List<QuizQuestion> Parse(string rawText)
        {
            var questions = new List<QuizQuestion>();
            if (string.IsNullOrEmpty(rawText))
                return questions;

            QuizQuestion current = null;

            foreach (var rawLine in rawText.Split('\n'))
            {
                var line = rawLine.Trim('\r', ' ', '\t');
                if (line.Length == 0)
                    continue;

                var answerMatch = AnswerLine.Match(line);
                if (answerMatch.Success && current != null)
                {
                    var answerText = answerMatch.Groups[1].Value;
                    if (CorrectMarker.IsMatch(answerText))
                    {
                        current.CorrectAnswerIndex = current.Answers.Count;
                        answerText = CorrectMarker.Replace(answerText, "");
                    }
                    current.Answers.Add(answerText.Trim());
                    continue;
                }

                var questionMatch = QuestionLine.Match(line);
                if (questionMatch.Success)
                {
                    current = new QuizQuestion { Text = questionMatch.Groups[1].Value.Trim() };
                    questions.Add(current);
                }
            }

            return questions;
        }
    }
}
