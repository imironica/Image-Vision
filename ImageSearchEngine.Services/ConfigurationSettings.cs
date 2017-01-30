using System;
using System.Collections.Generic;
using System.Linq;
using ImageSearchEngine.DTO;
using ImageSearchEngine.DTO.Enum;

namespace ImageSearchEngine.Services
{
    public class ConfigurationSettings
    {
        public static List<DocumentDescriptor> GetImageDescriptorsPerDb(string dbName)
        {
            var lstReturnedDescriptors = GetConfigurations()
                                            .Single(x => x.Code == dbName)
                                            .LstDocumentDescriptorFiles;
            List<DocumentDescriptor> lstImageDescriptors = new List<DocumentDescriptor>();
            foreach (var desc in lstReturnedDescriptors)
            {
                lstImageDescriptors.Add(desc.DocumentDescriptor);
            }
            return lstImageDescriptors;
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

            DocumentDatabase db1 = new DocumentDatabase() { Id = 1, Code = "Preclin", Folder = "db", Name = "Medical", Icon = "glyphicon glyphicon-plus" };
            db1.LstDocumentDescriptorFiles.Add(
                new DocumentDescriptorFile()
                {
                    Id = 1,
                    FileName = "~/PRECLIN_CSD.txt",
                    DocumentDescriptor = lstImageDescriptors[0]
                });
            db1.LstDocumentDescriptorFiles.Add(
               new DocumentDescriptorFile()
               {
                   Id = 2,
                   FileName = "~/PRECLIN_CLD.txt",
                   DocumentDescriptor = lstImageDescriptors[1]
               });
            db1.LstDocumentDescriptorFiles.Add(
               new DocumentDescriptorFile()
               {
                   Id = 3,
                   FileName = "~/PRECLIN_EHD.txt",
                   DocumentDescriptor = lstImageDescriptors[2]
               });
            db1.LstDocumentDescriptorFiles.Add(
              new DocumentDescriptorFile()
              {
                  Id = 4,
                  FileName = "~/PRECLIN_PCA.txt",
                  DocumentDescriptor = lstImageDescriptors[7],
                  UsePCA = true
              });

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
