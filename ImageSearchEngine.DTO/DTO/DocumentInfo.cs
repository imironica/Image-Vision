using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO
{
    [Serializable()]
    [BsonIgnoreExtraElements]
    public class DocumentInfo 
    {
        public DocumentInfo()
        {
            LstDescriptors = new Dictionary<string, double[]>();
        }

        public string Root { get; set; }
        public string ImageUrl { get; set; }
        public string DocumentName { get; set; }
        public string Description { get; set; }
        public Dictionary<string, double[]> LstDescriptors { get; set; }

        [field: NonSerializedAttribute()]
        private bool selected;
        public bool Selected {
            get { return selected; }
            set { selected = value; }
        }

        [field: NonSerializedAttribute()]
        private double distance;
        public double Distance
        {
            get { return distance; }
            set { distance = value; }
        }
        public Face[] Faces { get; set; }
    }

    [Serializable()]
    public class ImageInfo
    {
        public ImageInfo()
        {
            LstDescriptors = new Dictionary<string, double[]>();
        }

        public string Root { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public Dictionary<string, double[]> LstDescriptors { get; set; }
        public bool Selected { get; set; }
        public double Distance { get; set; }

        public List<Comment> Comments { get; set; }

        public List<Face> Faces { get; set; }
    }


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
