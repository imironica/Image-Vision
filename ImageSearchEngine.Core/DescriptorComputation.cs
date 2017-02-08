using ImageSearchEngine.DTO.Enum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.Core
{
    public static class DescriptorComputation
    {
        public static double[] ComputeDescriptor(DescriptorTypeEnum descriptorType, string imageFileName)
        {
            double[] descriptor = new double[0];
            Bitmap image = (Bitmap)Image.FromFile(imageFileName, true);

            return ComputeDescriptor(descriptorType, image);
        }

        public static double[] ComputeDescriptor(string descriptorName, Bitmap image)
        {
            double[] descriptor = new double[0];
            if (descriptorName.Contains("CSD"))
            {
                descriptor = ComputeScalableColorMPEG7(image);
            }
            if (descriptorName.Contains("CLD"))
            {
                descriptor = ComputeColorLayoutMPEG7(image);
            }
            if (descriptorName.Contains("EHD"))
            {
                descriptor = ComputeEdgeHistogramMPEG7(image);
            }
            var sum = descriptor.Sum();
            if (sum > 0)
                for (int k = 0; k < descriptor.Length; k++)
                    descriptor[k] /= sum;
            return descriptor;
        }

        public static double[] ComputeDescriptor(DescriptorTypeEnum descriptorType, Bitmap image)
        {
            double[] descriptor = new double[0];
            switch (descriptorType)
            {
                case DescriptorTypeEnum.ScalableColorMPEG7:
                    {
                        descriptor = ComputeScalableColorMPEG7(image);
                        break;
                    }
                case DescriptorTypeEnum.EdgeHistogramMPEG7:
                    {
                        descriptor = ComputeEdgeHistogramMPEG7(image);
                        break;
                    }
                case DescriptorTypeEnum.ColorLayoutMPEG7:
                    {
                        descriptor = ComputeColorLayoutMPEG7(image);
                        break;
                    }
                case DescriptorTypeEnum.DominantColorMPEG7:
                    {
                        descriptor = DominantColorMPEG7(image);
                        break;
                    }
            }

            return descriptor;
        }

        private static double[] ComputeScalableColorMPEG7(Bitmap image)
        {
            SCD_Descriptor scdDescriptor = new SCD_Descriptor();
            try
            {
                scdDescriptor.Apply(image, 256, 0);
            }
            catch (Exception)
            {
                return new double[64];
            }
            return scdDescriptor.Norm4BitHistogram;
        }

        private static double[] ComputeColorLayoutMPEG7(Bitmap image)
        {
            CLD_Descriptor cldDescriptor = new CLD_Descriptor();
            cldDescriptor.Apply(image);
            int[] Y = cldDescriptor.CbCoeff;
            int[] CB = cldDescriptor.CbCoeff;
            int[] CR = cldDescriptor.CbCoeff;
            double[] desc = new double[Y.Length + CB.Length + CR.Length];
            int index = 0;
            for (int i = 0; i < Y.Length; i++)
            {
                desc[index++] = Y[i];
            }

            for (int i = 0; i < Y.Length; i++)
            {
                desc[index++] = CB[i];
            }
            for (int i = 0; i < Y.Length; i++)
            {
                desc[index++] = CR[i];
            }
            return desc;
        }

        private static double[] ComputeEdgeHistogramMPEG7(Bitmap image)
        {
            EHD_Descriptor ehdDescriptor = new EHD_Descriptor(30);

            return ehdDescriptor.Apply(image);
        }

        private static double[] DominantColorMPEG7(Bitmap image)
        {
            DCD_Descriptor dcdDescriptor = new DCD_Descriptor();
            dcdDescriptor.extractDescriptor(image);
            return new double[0];
        }

    }
}
