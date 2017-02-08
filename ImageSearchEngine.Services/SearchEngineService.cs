using ImageSearchEngine.Core;
using ImageSearchEngine.DTO;
using ImageSearchEngine.DTO.Enum;
using ImageSearchEngine.DTO.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;

namespace ImageSearchEngine.Services
{
    public class SearchEngineService
    {
        public IDescriptorManager _descriptorManager;
        public ISearchImageProcessor _searchImageProcessor;
        public ICacheManager _cacheManager;
        double _conceptMinimumPercentage;
        public SearchEngineService(IDescriptorManager descriptorManager, ICacheManager cacheManager)
        {
            _descriptorManager = descriptorManager;
            _searchImageProcessor = new SearchImageProcessor();
            _cacheManager = cacheManager;
            _conceptMinimumPercentage = 0.60;
        }

        public string GetImageFolderName(string dbName)
        {
            var dbInfo = ConfigurationSettings.GetDatabasesList().Where(x => x.Code == dbName).FirstOrDefault();
            if (dbInfo != null)
                return dbInfo.Folder;
            else
                throw new Exception("Invalid image database!");

        }
        public string GetDescriptorCode(string dbName)
        {
            var dbInfo = ConfigurationSettings.GetDatabasesList().Where(x => x.Code == dbName).FirstOrDefault().DescriptorsCodes.FirstOrDefault();
            if (dbInfo != null)
                return dbInfo.Id;
            else
                throw new Exception("Invalid image database!");

        }



        public List<DocumentInfo> GetAllImage(string dbName, int numberOfImages)
        {
            var root = GetImageFolderName(dbName);
            var lstImages = _cacheManager.GetDbData(dbName);

            var lstReturnedImages = lstImages.Select(x => new DocumentInfo()
            {
                Description = x.Description,
                Root = root,
                ImageUrl = x.DocumentName,
                Distance = x.Distance,
                Concepts = x.Concepts
            }).OrderBy(x => x.Distance);

            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            var rnd = new Random(unixTime);
            return lstReturnedImages.OrderBy(a => rnd.Next()).Take(numberOfImages).ToList();
        }

        public List<DocumentInfo> GetImage(string imageName,
                                           string dbName,
                                           string metric,
                                           string descriptor,
                                           int providedResults,
                                           string selectedProcessing)
        {
            imageName = imageName.Replace("!", "\\");
            var root = GetImageFolderName(dbName);
            var lstImages = _cacheManager.GetDbData(dbName);
            var searchedImageLst = lstImages.Where(x => x.DocumentName.ToUpper() == imageName.ToUpper().Trim());
            if (searchedImageLst != null)
            {
                var searchedImage = searchedImageLst.FirstOrDefault();
                var lstReturnedImages = _searchImageProcessor.SearchSimilarImage(lstImages, searchedImage, metric, root, providedResults, selectedProcessing, descriptor);
                return lstReturnedImages;
            }

            return null;
        }


        public List<DocumentInfo> GetImagesByConcept(Query query)
        {
            var root = GetImageFolderName(query.DbName);
            var lstImages = _cacheManager.GetDbData(query.DbName);

            var searchedImages = lstImages.Where(x => x.Concepts != null
                                                      && x.Concepts.Length > 0
                                                      && x.Concepts.Any(y => y != null &&
                                                                             y.Name.Contains(query.Descriptor) &&
                                                                             y.Percentage > _conceptMinimumPercentage));
            if (searchedImages != null)
            {
                var imgs = searchedImages.Select(x => new DocumentInfo()
                {
                    Description = x.Description,
                    Root = root,
                    ImageUrl = x.DocumentName,
                    Distance = x.Distance,
                    Concepts = x.Concepts
                }).ToList();
                return imgs;
            }
            else
                return null;
        }

        public List<DocumentInfo> SearchImageLink(Query query)
        {
            var root = GetImageFolderName(query.DbName);
            var lstImages = _cacheManager.GetDbData(query.DbName);

            DocumentInfo searchedImage = null;
            var request = WebRequest.Create(query.ImageLink);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                var image = Bitmap.FromStream(stream);
                double[] desc = DescriptorComputation.ComputeDescriptor(query.Descriptor, (Bitmap)image);
                var valueDesc = new Dictionary<string, double[]>();
                valueDesc.Add(query.Descriptor, desc);
                searchedImage = new DocumentInfo() { LstDescriptors = valueDesc };
            }

            var lstReturnedImages = _searchImageProcessor.SearchSimilarImage(lstImages, searchedImage, query.Metric, root, query.ProvidedResults, ProcessingTypeEnum.OneCPU.ToString(), query.Descriptor);
            return lstReturnedImages;
        }


        public List<DocumentInfo> SearchImageRF(Query rfQuery)
        {
            var dbName = rfQuery.DbName;
            var descriptor = rfQuery.Descriptor;
            var providedResults = rfQuery.ProvidedResults;
            var images = rfQuery.Images;

            var results = JsonConvert.DeserializeObject<List<DocumentInfo>>(images.ToString());
            string descLabel = rfQuery.Descriptor;
            var descriptorFile = ConfigurationSettings.GetDatabasesList().Where(x => x.Code == dbName).FirstOrDefault().DescriptorsCodes.FirstOrDefault().Id;
            var root = GetImageFolderName(dbName);
            var lstImages = _cacheManager.GetDbData(rfQuery.DbName);

            List<DocumentInfo> selectedRelevantImages = (from image in lstImages
                                                         join result in results on image.ImageUrl equals result.ImageUrl
                                                         where result.Selected == true
                                                         select image).ToList();

            if (rfQuery.RFAlgorithm == "2") //Rocchio
            {
                RocchioRF(descLabel, lstImages, selectedRelevantImages);
            }

            ////RFE
            if (rfQuery.RFAlgorithm == "1")
            {
                RfeRF(descLabel, lstImages, selectedRelevantImages);

            }
            foreach (var item in lstImages.Where(w => results.Where(x => x.Selected).Select(y => y.ImageUrl).Contains(w.ImageUrl)))
            {
                item.Distance = 0;
            }
            foreach (var item in lstImages.Where(w => results.Where(x => !x.Selected).Select(y => y.ImageUrl).Contains(w.ImageUrl)))
            {
                item.Distance = 1000000;
            }

            var lstReturnedImages = lstImages.Select(x => new DocumentInfo()
            {
                Description = x.Description,
                Root = root,
                ImageUrl = x.ImageUrl,
                Distance = x.Distance,
                Concepts = x.Concepts
            }).OrderBy(x => x.Distance).Take(providedResults);

            return lstReturnedImages.ToList();
        }

        #region RF
        private static void RfeRF(string descLabel, List<DocumentInfo> lstImages, List<DocumentInfo> selectedRelevantImages)
        {
            var featureSize = selectedRelevantImages[0].LstDescriptors[descLabel].Length;
            double[] weights = new double[featureSize];
            double[] meanList = new double[featureSize];

            foreach (var relevantImage in selectedRelevantImages)
            {
                for (int i = 0; i < featureSize; i++)
                {
                    meanList[i] += relevantImage.LstDescriptors[descLabel][i];
                }
            }

            for (int i = 0; i < featureSize; i++)
            {
                meanList[i] /= selectedRelevantImages.Count;
            }

            foreach (var relevantImage in selectedRelevantImages)
            {
                for (int i = 0; i < featureSize; i++)
                {
                    weights[i] += (meanList[i] - relevantImage.LstDescriptors[descLabel][i]) * (meanList[i] - relevantImage.LstDescriptors[descLabel][i]);
                }
            }

            var features = selectedRelevantImages[0].LstDescriptors[descLabel];
            double distance = 0;
            double[] featuresImage;
            for (int i = 0; i < lstImages.Count; i++)
            {
                distance = 0;
                featuresImage = lstImages[i].LstDescriptors[descLabel];
                for (int j = 0; j < features.Length; j++)
                {
                    if (weights[j] != 0)
                        distance += (featuresImage[j] - features[j]) * (featuresImage[j] - features[j]) / weights[j];
                    else
                        distance += (featuresImage[j] - features[j]) * (featuresImage[j] - features[j]) * 2;
                }
                lstImages[i].Distance = distance;
            }
        }

        private static void RocchioRF(string descLabel, List<DocumentInfo> lstImages, List<DocumentInfo> selectedRelevantImages)
        {
            var featureSize = selectedRelevantImages[0].LstDescriptors[descLabel].Length;
            double[] weights = new double[featureSize];
            double[] meanList = new double[featureSize];


            foreach (var relevantImage in selectedRelevantImages)
            {
                for (int i = 0; i < featureSize; i++)
                {
                    meanList[i] += relevantImage.LstDescriptors[descLabel][i];
                }
            }
            for (int i = 0; i < featureSize; i++)
            {
                meanList[i] /= selectedRelevantImages.Count;
            }

            var features = meanList;
            double distance = 0;
            double[] featuresImage;
            for (int i = 0; i < lstImages.Count; i++)
            {
                distance = 0;
                featuresImage = lstImages[i].LstDescriptors[descLabel];
                for (int j = 0; j < features.Length; j++)
                {
                    distance += (featuresImage[j] - features[j]) * (featuresImage[j] - features[j]);
                }
                lstImages[i].Distance = distance;
            }
        }
        #endregion

        public List<DocumentInfo> SearchImagesSameEvent(string imageName, string dbName, string metric, string descriptor, int providedResults, string selectedProcessing)
        {
            var searchNameCategory = imageName.Substring(1);
            string categoryName = searchNameCategory.Substring(0, searchNameCategory.IndexOf('!') < 0 ? 0 : searchNameCategory.IndexOf('!'));
            imageName = imageName.Replace("!", "\\");
            var root = GetImageFolderName(dbName);
            var descriptorFile = GetDescriptorCode(dbName);

            var lstAllImages = _cacheManager.GetDbData(dbName);

            var lstImages = lstAllImages.Where(x => x.ImageUrl.Contains(categoryName)).ToList();
            var searchedImageLst = lstImages.Where(x => x.ImageUrl.Replace("\\", "") == imageName.Replace("\\", ""));
            if (searchedImageLst != null)
            {
                var searchedImage = searchedImageLst.FirstOrDefault();
                var lstReturnedImages = _searchImageProcessor.SearchSimilarImage(lstImages, searchedImage, metric, root, providedResults, selectedProcessing, descriptor);
                return lstReturnedImages;
            }
            return null;
        }

        public List<DescriptorMetadata> GetDescriptors(string dbName)
        {
            var lstReturnedDescriptors = ConfigurationSettings.GetDocumentDescriptorsPerDb(dbName);
            return lstReturnedDescriptors;
        }

        public List<ImageMetric> GetMetrics(string descriptorName)
        {
            var lstReturnedDescriptors = ConfigurationSettings.GetImageMetricsPerDb(descriptorName);
            return lstReturnedDescriptors;
        }

        public List<DocumentDatabase> GetDatabases()
        {
            var lstReturnedDescriptors = ConfigurationSettings.GetDatabasesList();
            return lstReturnedDescriptors;
        }

        public List<DocumentPCAInfo> GetAllImage3D(string dbName, int numberOfImages)
        {
            int numberOfShownImages = numberOfImages;
            var root = GetImageFolderName(dbName);
            var lstImages = _cacheManager.GetDbData(dbName);
            var descriptorFile = "PCA";


            var lstReturnedImages = lstImages.Select(obj => new DocumentPCAInfo()
            {
                x = obj.LstDescriptors[descriptorFile][0],
                y = obj.LstDescriptors[descriptorFile][1],
                z = obj.LstDescriptors[descriptorFile][2],
                image = "../" + root + "/" + obj.DocumentName
            });

            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            var rnd = new Random(unixTime);
            return lstReturnedImages.OrderBy(a => rnd.Next()).Take(numberOfShownImages).ToList();
        }
    }
}
