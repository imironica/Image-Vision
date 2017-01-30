using ImageSearchEngine.DTO;
using ImageSearchEngine.DTO.Enum;
using ImageSearchEngine.DTO.Interfaces;
using ImageSearchEngine.DTO.Interfaces.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.Core
{
    public class SearchImageProcessor : ISearchImageProcessor
    {
        public List<DocumentInfo> SearchSimilarImage(List<DocumentInfo> db, DocumentInfo searchedImage, string metricType, string root, int providedResults, string processingType)
        {
            var descLabel = "DESC";
            if (searchedImage == null)
                throw new Exception("Image not found in the current database!");

            var features = searchedImage.LstDescriptors[descLabel];
            IMetricsComputation compute = MetricsComputationFactory.GetProcessor(processingType);

            if (metricType == Convert.ToInt32(MetricTypeEnum.Euclidian).ToString())
            {
                compute.ComputeEuclidianDistance(ref db, descLabel, features);
            }
            if (metricType == Convert.ToInt32(MetricTypeEnum.Manhattan).ToString())
            {
                compute.ComputeManhathanDistance(ref db, descLabel, features);
            }
            if (metricType == Convert.ToInt32(MetricTypeEnum.Cosinus).ToString())
            {
                compute.ComputeCosinusDistance(ref db, descLabel, features);
            }

            IEnumerable<DocumentInfo> lstReturnedImages;

            if (metricType != Convert.ToInt32(MetricTypeEnum.Cosinus).ToString())
                lstReturnedImages = db.Select(x => new DocumentInfo()
                {
                    Description = x.Description,
                    Root = root,
                    ImageUrl = x.ImageUrl,
                    Distance = x.Distance
                }).OrderBy(x => x.Distance).Take(providedResults);
            else
                lstReturnedImages = db.Select(x => new DocumentInfo()
                {
                    Description = x.Description,
                    Root = root,
                    ImageUrl = x.ImageUrl,
                    Distance = x.Distance
                }).OrderByDescending(x => x.Distance).Take(providedResults);

            return lstReturnedImages.ToList();
        }


    }
}
