using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO
{
    public class DocumentDatabase
    {
        public DocumentDatabase()
        {
            DescriptorsCodes = new List<DescriptorMetadata>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Folder { get; set; }
        public string Icon { get; set; }
        public List<DescriptorMetadata> DescriptorsCodes { get; set; }
    }
}
