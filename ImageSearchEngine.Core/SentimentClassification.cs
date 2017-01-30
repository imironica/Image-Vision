using ImageSearchEngine.DTO.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageSearchEngine.Core
{
    public class SentimentClassification
    {
        private static readonly char[] delimiters = { ' ', '.', ',', ';', '\'', '-', ':', '!', '?', '(', ')', '<', '>', '=', '*', '/', '[', ']', '{', '}', '\\', '"', '\r', '\n' };
    
        public static SentimentEnum Clasify(string text, List<string> lstPositiveTerms, List<string> lstNegativeTerms)
        {
            int positiveTerms = 0;
            int negativeTerms = 0;
            foreach (var wordCount in text
              .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
              .AsParallel())
            {
                if (lstPositiveTerms.Contains(wordCount.ToLower()))
                    positiveTerms++;
                if (lstNegativeTerms.Contains(wordCount.ToLower()))
                    negativeTerms++;
            }
            if (positiveTerms > negativeTerms)
                return SentimentEnum.Positive;
            else
                if (positiveTerms < negativeTerms)
                return SentimentEnum.Negative;
            else
                return SentimentEnum.Neutral;

        }

        public static SentimentEnum Clasify(string text, List<string> lstPositiveTerms, List<string> lstNegativeTerms, out int positiveTerms, out int negativeTerms)
        {
            positiveTerms = 0;
            negativeTerms = 0;
            foreach (var wordCount in text
              .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
              .AsParallel())
            {
                if (lstPositiveTerms.Contains(wordCount.ToLower()))
                    positiveTerms++;
                if (lstNegativeTerms.Contains(wordCount.ToLower()))
                    negativeTerms++;
            }
            if (positiveTerms > negativeTerms)
                return SentimentEnum.Positive;
            else
                if (positiveTerms < negativeTerms)
                return SentimentEnum.Negative;
            else
                return SentimentEnum.Neutral;

        }
    }
}
