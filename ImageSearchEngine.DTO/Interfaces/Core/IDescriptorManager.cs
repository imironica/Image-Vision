using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO.Interfaces
{
    public interface IDescriptorManager
    {
        void WriteImageDescriptor(string filePath, List<DocumentInfo> lstImages);
        List<DocumentInfo> GetImageDescriptor(string filePath);
        List<ImageInfo> GetImagesDescriptor(string filePath);
        List<TextInfo> GetTextDescriptor(string filePath); 

    }
}
