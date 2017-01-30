using ImageSearchEngine.DTO.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO
{
    [Serializable()]
    public class TextInfo
    {
        public string Text { get; set; }
        public SentimentEnum Sentiment { get; set; }
        public int NegativeScore { get; set; }
        public int PositiveScore { get; set; }
    }
}
