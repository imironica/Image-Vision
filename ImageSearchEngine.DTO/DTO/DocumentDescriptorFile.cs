using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO
{
    public class DocumentDescriptorFile
    {
        public DocumentDescriptorFile()
        {
            UsePCA = false;
        }

        public int Id { get; set; }
        public string FileName { get; set; }
        public DocumentDescriptor DocumentDescriptor { get; set; }
        public bool UsePCA { get; set; }
    }
}
