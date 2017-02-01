using System;
using System.Collections.Generic;
using System.Linq;
using ImageSearchEngine.DTO;
using ImageSearchEngine.DTO.Enum;

namespace ImageSearchEngine.Services
{
    public class ConfigurationSettings
    {
        public static List<DescriptorMetadata> GetImageDescriptorsPerDb(string dbName)
        {
            var lstReturnedDescriptors = GetConfigurations()
                                            .Single(x => x.Code == dbName)
                                            .DescriptorsCodes;

            return lstReturnedDescriptors;
        }

        public static List<ImageMetric> GetImageMetricsPerDb(string descriptorName)
        {
            var lstReturnedMetrics = new List<ImageMetric>();
            lstReturnedMetrics.Add(new ImageMetric()
            {
                Id = Convert.ToInt32(MetricTypeEnum.Euclidian),
                Name = "Euclidian"
            });
            lstReturnedMetrics.Add(new ImageMetric()
            {
                Id = Convert.ToInt32(MetricTypeEnum.Manhattan),
                Name = "Manhattan"
            });
            lstReturnedMetrics.Add(new ImageMetric()
            {
                Id = Convert.ToInt32(MetricTypeEnum.Cosinus),
                Name = "Cosinus"
            });

            return lstReturnedMetrics;
        }

        private static List<DocumentDatabase> GetConfigurations()
        {
            var lstDatabases = new List<DocumentDatabase>();
            var lstImageDescriptors = GetImageDescriptors();

            DocumentDatabase db1 = new DocumentDatabase() { Id = 1, Code = "medicalDb", Folder = "db", Name = "Cancer database", Icon = "glyphicon glyphicon-plus" };
            db1.DescriptorsCodes.Add(new DescriptorMetadata() { Id = "CSD", Name = "Color Structure Descriptor"});
            db1.DescriptorsCodes.Add(new DescriptorMetadata() { Id = "CLD", Name = "Color Layout Descriptor" });

            lstDatabases.Add(db1);
            return lstDatabases;
        }

        public static List<DocumentDatabase> GetDatabases()
        {
            return GetConfigurations();
        }

        private static List<DocumentDescriptor> GetImageDescriptors()
        {
            var lstReturnedDescriptors = new List<DocumentDescriptor>();
            lstReturnedDescriptors.Add(new DocumentDescriptor()
            {
                Id = 1,
                Name = "Color Structure Descriptor (MPEG 7)"
            });
            lstReturnedDescriptors.Add(new DocumentDescriptor()
            {
                Id = 2,
                Name = "Color Layout Descriptor (MPEG 7)"
            });
            lstReturnedDescriptors.Add(new DocumentDescriptor()
            {
                Id = 3,
                Name = "Color Edge Descriptor (MPEG 7)"
            });
            lstReturnedDescriptors.Add(new DocumentDescriptor()
            {
                Id = 4,
                Name = "Bag of Words (SURF)"
            });
            lstReturnedDescriptors.Add(new DocumentDescriptor()
            {
                Id = 5,
                Name = "Histograms of oriented gradients"
            });
            lstReturnedDescriptors.Add(new DocumentDescriptor()
            {
                Id = 6,
                Name = "Discrete Cosinus Transform"
            });
            lstReturnedDescriptors.Add(new DocumentDescriptor()
            {
                Id = 7,
                Name = "Gabor filters"
            });
            lstReturnedDescriptors.Add(new DocumentDescriptor()
            {
                Id = 8,
                Name = "Principal Component Analysis"
            });
            lstReturnedDescriptors.Add(new DocumentDescriptor()
            {
                Id = 9,
                Name = "Hu"
            });
            lstReturnedDescriptors.Add(new DocumentDescriptor()
            {
                Id = 10,
                Name = "Moments"
            });
            lstReturnedDescriptors.Add(new DocumentDescriptor()
            {
                Id = 11,
                Name = "Zernike"
            });
            return lstReturnedDescriptors;
        }
    }
}
