using System.Collections.Generic;

namespace ImageSearchEngine.DTO.Interfaces
{
    public interface IDescriptorManager
    {
        void WriteImageDescriptor(string filePath, List<DocumentInfo> lstImages);
        List<DocumentInfo> GetImageDescriptor(string filePath);

    }
}
