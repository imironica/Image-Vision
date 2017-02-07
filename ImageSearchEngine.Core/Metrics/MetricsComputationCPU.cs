using ImageSearchEngine.DTO;
using ImageSearchEngine.DTO.Interfaces.Core;
using System;
using System.Collections.Generic;

namespace ImageSearchEngine.Core
{
    public class MetricsComputationCPU : IMetricsComputation
    {
        public void ComputeEuclidianDistance(ref List<DocumentInfo> dbImages, string descLabel, double[] queryFeature)
        {
            for (int i = 0; i < dbImages.Count; i++)
            {
                double distance = 0;

                if (dbImages[i].LstDescriptors.ContainsKey(descLabel))
                {
                    var featuresImage = dbImages[i].LstDescriptors[descLabel];
                    for (int j = 0; j < queryFeature.Length; j++)
                    {
                        distance += (featuresImage[j] - queryFeature[j]) * (featuresImage[j] - queryFeature[j]);
                    }
                    dbImages[i].Distance = distance;
                }
                else
                    dbImages[i].Distance = int.MaxValue;

            }
        }
        public void ComputeManhathanDistance(ref List<DocumentInfo> dbImages, string descLabel, double[] queryFeature)
        {
            for (int i = 0; i < dbImages.Count; i++)
            {
                double distance = 0;
                if (dbImages[i].LstDescriptors.ContainsKey(descLabel))
                {
                    var featuresImage = dbImages[i].LstDescriptors[descLabel];
                    for (int j = 0; j < queryFeature.Length; j++)
                    {
                        distance += Math.Abs(featuresImage[j] - queryFeature[j]);
                    }
                    dbImages[i].Distance = distance;
                }
                else
                    dbImages[i].Distance = int.MaxValue;
            }
        }
        public void ComputeCosinusDistance(ref List<DocumentInfo> dbImages, string descLabel, double[] queryFeature)
        {
            double distCos1 = 0;
            double distCos2 = 0;
            for (int i = 0; i < dbImages.Count; i++)
            {
                double distance = 0;
                if (dbImages[i].LstDescriptors.ContainsKey(descLabel))
                {
                    var featuresImage = dbImages[i].LstDescriptors[descLabel];
                    distCos1 = 0;
                    distCos2 = 0;
                    for (int j = 0; j < queryFeature.Length; j++)
                    {
                        distance += featuresImage[j] * queryFeature[j];
                        distCos1 += featuresImage[j] * queryFeature[j];
                        distCos2 += queryFeature[j] * queryFeature[j];
                    }
                    dbImages[i].Distance = distance / Math.Sqrt(distCos1 * distCos2);
                }
                else
                    dbImages[i].Distance = int.MaxValue;
            }
        }
    }
}
