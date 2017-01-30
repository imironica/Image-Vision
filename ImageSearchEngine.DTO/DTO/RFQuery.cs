using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO
{
    public class Query
    {
        public Query()
        {

        }
        public Object Images { get; set; }
        public string DbName { get; set; }
        public string Descriptor { get; set; }
        public int ProvidedResults { get; set; }
        public string RFAlgorithm { get; set; }
        public string ImageLink { get; set; }
        public string Metric { get; set; }
    }
}
