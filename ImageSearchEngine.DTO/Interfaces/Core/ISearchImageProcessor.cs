using ImageSearchEngine.DTO.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO.Interfaces
{
    public interface ISearchImageProcessor
    {
        List<DocumentInfo> SearchSimilarImage(List<DocumentInfo> db, DocumentInfo searchedImage, string metricType, string root, int providedResults, string processingType);
    }
}
