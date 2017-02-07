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
        public Concept[] Concepts { get; set; }
    }

   
}
