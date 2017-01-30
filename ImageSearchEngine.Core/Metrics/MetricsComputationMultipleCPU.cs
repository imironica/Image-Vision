using ImageSearchEngine.DTO;
using ImageSearchEngine.DTO.Interfaces.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageSearchEngine.Core
{
    public class MetricsComputationMultipleCPU : IMetricsComputation
    {
        public void ComputeEuclidianDistance(ref List<DocumentInfo> dbImages, string descLabel, double[] queryFeature)
        {
            var db = dbImages;
            Parallel.For(0, dbImages.Count, new ParallelOptions { MaxDegreeOfParallelism = 24 }, index =>
            {
                double distance = 0;
                var featuresImage = db[index].LstDescriptors[descLabel];
                for (int j = 0; j < queryFeature.Length; j++)
                {
                    distance += (featuresImage[j] - queryFeature[j]) * (featuresImage[j] - queryFeature[j]);
                }
                db[index].Distance = distance;
            });
            dbImages = db;
        }
        public void ComputeManhathanDistance(ref List<DocumentInfo> dbImages, string descLabel, double[] queryFeature)
        {
            var db = dbImages;
            Parallel.For(0, dbImages.Count, new ParallelOptions { MaxDegreeOfParallelism = 24 }, index =>
            {
                double distance = 0;
                var featuresImage = db[index].LstDescriptors[descLabel];
                for (int j = 0; j < queryFeature.Length; j++)
                {
                    distance += Math.Abs(featuresImage[j] - queryFeature[j]);
                }
                db[index].Distance = distance;
            });
            dbImages = db;
        }
        public void ComputeCosinusDistance(ref List<DocumentInfo> dbImages, string descLabel, double[] queryFeature)
        {
            
            var db = dbImages;
            Parallel.For(0, dbImages.Count, new ParallelOptions { MaxDegreeOfParallelism = 24 }, index =>
            {
                double distance = 0;
                var featuresImage = db[index].LstDescriptors[descLabel];
                double distCos1 = 0;
                double distCos2 = 0;
                for (int j = 0; j < queryFeature.Length; j++)
                {
                    distance += featuresImage[j] * queryFeature[j];
                    distCos1 += featuresImage[j] * queryFeature[j];
                    distCos2 += queryFeature[j] * queryFeature[j];
                }
                db[index].Distance = distance / Math.Sqrt(distCos1 * distCos2);
            });
            dbImages = db;
        }
    }
}
