using System;

namespace ImageSearchEngine.DTO
{
    [Serializable()]
    public class DocumentPCAInfo
    {
        public DocumentPCAInfo()
        {
        }

        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public string image { get; set; }
    }
}
