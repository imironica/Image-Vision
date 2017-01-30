using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO.Enum
{
    public enum DescriptorTypeEnum
    {
        ScalableColorMPEG7 = 1,
        ColorLayoutMPEG7 = 2,
        EdgeHistogramMPEG7 = 3,
        DominantColorMPEG7 = 4,
        BOW_SIFT = 5,
        HOG = 6
    }
}
