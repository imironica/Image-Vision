using ImageSearchEngine.DTO.Enum;
using ImageSearchEngine.DTO.Interfaces.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.Core
{
    public class MetricsComputationFactory
    {
        public static IMetricsComputation GetProcessor(string processingType)
        {
            if (processingType == Convert.ToInt32(ProcessingTypeEnum.OneCPU).ToString())
            {
                return new MetricsComputationCPU();
            }
            if (processingType == Convert.ToInt32(ProcessingTypeEnum.Multithreading).ToString())
            {
                return new MetricsComputationMultipleCPU();
            }
            if (processingType == Convert.ToInt32(ProcessingTypeEnum.GPU).ToString())
            {
                return new MetricsComputationCPU();
            }
            return new MetricsComputationCPU();
        }
    }
}
