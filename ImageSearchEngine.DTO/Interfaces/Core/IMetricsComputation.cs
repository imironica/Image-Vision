using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO.Interfaces.Core
{
    public interface IMetricsComputation
    {
        void ComputeEuclidianDistance(ref List<DocumentInfo> dbImages, string descLabel, double[] queryFeature);
        void ComputeCosinusDistance(ref List<DocumentInfo> dbImages, string descLabel, double[] queryFeature);
        void ComputeManhathanDistance(ref List<DocumentInfo> dbImages, string descLabel, double[] queryFeature);
    }
}
 