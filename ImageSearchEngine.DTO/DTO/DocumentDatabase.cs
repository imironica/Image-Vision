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
            LstDocumentDescriptorFiles = new List<DocumentDescriptorFile>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Folder { get; set; }
        public string Icon { get; set; }
        public List<DocumentDescriptorFile> LstDocumentDescriptorFiles { get; set; }
    }
}
