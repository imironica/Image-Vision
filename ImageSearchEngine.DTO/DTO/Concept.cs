using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO
{
    [Serializable()]
    public class Concept
    {
        public double Percentage { get; set; }
        public string Name { get; set; }
    }
}
