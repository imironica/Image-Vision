using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using ImageSearchEngine.DTO;
using ImageSearchEngine.Services;
using ImageSearchEngine.DTO.Interfaces;
using ImageSearchEngine.Core;
using System.Web;
using System.Net;
using System.Drawing;
using ImageSearchEngine.DTO.Enum;

namespace TestAccord.Controllers
{
    [RoutePrefix("api/searchEngine")]
    public class ImageSearchEngineController : ApiController
    {
        public IDescriptorManager descriptorManager;
        public ISearchImageProcessor searchImageProcessor;

        public ImageSearchEngineController(IDescriptorManager descriptorManagerType)
        {
            descriptorManager = descriptorManagerType;
            searchImageProcessor = new SearchImageProcessor();
        }

        private List<DocumentInfo> GetDocumentsDb(string dbName)
        {
            if (HttpContext.Current.Application[dbName] == null)
                HttpContext.Current.Application[dbName] = descriptorManager.GetImageDescriptor(dbName);
            return (List<DocumentInfo>)HttpContext.Current.Application[dbName];
        }

        /// <summary>
        /// Show random images from the database
        /// </summary>
        [System.Web.Http.Route("AllImages/{dbName}/{numberOfImages}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetAllImage(string dbName, int numberOfImages)
        {
            string root = ConfigurationSettings.GetDatabases().Where(x => x.Code == dbName).FirstOrDefault().Folder;
            var lstImages = GetDocumentsDb(dbName);

            try
            {
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
                return Json(lstReturnedImages.OrderBy(a => rnd.Next()).Take(numberOfImages));
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }


        /// <summary>
        /// Search a similar image in the database
        /// </summary>
        [System.Web.Http.Route("SearchImage/{imageName}/{dbName}/{metric}/{descriptor}/{providedResults}/{selectedProcessing}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetImage([FromUri] string imageName, 
                                          [FromUri] string dbName, 
                                          [FromUri] string metric, 
                                          [FromUri] string descriptor, 
                                          [FromUri] int providedResults, 
                                          [FromUri] string selectedProcessing)
        {
            imageName = imageName.Replace("!", "\\");
            var root = ConfigurationSettings.GetDatabases().Where(x => x.Code == dbName).FirstOrDefault().Folder;
            var lstImages = GetDocumentsDb(dbName);

            var searchedImage = lstImages.Where(x => x.DocumentName.ToUpper() == imageName.ToUpper().Trim()).FirstOrDefault();
            var lstReturnedImages = searchImageProcessor.SearchSimilarImage(lstImages, searchedImage, metric, root, providedResults, selectedProcessing, descriptor);
            return Json(lstReturnedImages);
        }

        /// <summary>
        /// Search a similar image in the database
        /// </summary>
        [System.Web.Http.Route("GetImagesByConcept")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult GetImagesByConcept([FromBody] Query query)
        {
       
            var root = ConfigurationSettings.GetDatabases().Where(x => x.Code == query.DbName).FirstOrDefault().Folder;
            var lstImages = GetDocumentsDb(query.DbName);

            var searchedImages = lstImages.Where(x => x.Concepts != null && x.Concepts.Length > 0 && x.Concepts.Any(y=>y!= null && y.Name.Contains(query.Descriptor) && y.Percentage > 0.6));

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
                return Json(imgs);
            }
            else
                return null;
        }

        /// <summary>)
        /// Search a similar image from a link
        /// </summary>
        [System.Web.Http.Route("SearchImageLink")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult SearchImageLink([FromBody] Query query)
        {
            var root = ConfigurationSettings.GetDatabases().Where(x => x.Code == query.DbName).FirstOrDefault().Folder;
            var lstImages = GetDocumentsDb(query.DbName);

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

            var lstReturnedImages = searchImageProcessor.SearchSimilarImage(lstImages, searchedImage, query.Metric, root, query.ProvidedResults, ProcessingTypeEnum.OneCPU.ToString(), query.Descriptor);
            return Json(lstReturnedImages);
        }

        /// <summary>
        /// Perform a relevance feedback algorithm
        /// </summary>
        [System.Web.Http.Route("SearchImageRF")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult SearchImageRF([FromBody] Query rfQuery)
        {
            ///ROCCHIO
            var dbName = rfQuery.DbName;
            var descriptor = rfQuery.Descriptor;
            var providedResults = rfQuery.ProvidedResults;
            var images = rfQuery.Images;

            var results = JsonConvert.DeserializeObject<List<DocumentInfo>>(images.ToString());
            string descLabel = rfQuery.Descriptor;
            var descriptorFile = ConfigurationSettings.GetDatabases().Where(x => x.Code == dbName).FirstOrDefault().DescriptorsCodes.FirstOrDefault().Id;
            var root = ConfigurationSettings.GetDatabases().Where(x => x.Code == dbName).FirstOrDefault().Folder;
            var lstImages = GetDocumentsDb(rfQuery.DbName);


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

            return Json(lstReturnedImages);
        }

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

        /// <summary>
        /// Search images from the same folder
        /// </summary>
        [System.Web.Http.Route("SearchImagesSameEvent/{imageName}/{dbName}/{metric}/{descriptor}/{providedResults}/{selectedProcessing}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult SearchImagesSameEvent([FromUri] string imageName, [FromUri] string dbName, [FromUri] string metric, [FromUri] string descriptor, [FromUri] int providedResults, [FromUri] string selectedProcessing)
        {
            var searchNameCategory = imageName.Substring(1);
            string categoryName = searchNameCategory.Substring(0, searchNameCategory.IndexOf('!') < 0 ? 0 : searchNameCategory.IndexOf('!'));
            imageName = imageName.Replace("!", "\\");
            var root = ConfigurationSettings.GetDatabases().Where(x => x.Code == dbName).FirstOrDefault().Folder;
            var descriptorFile = ConfigurationSettings.GetDatabases().Where(x => x.Code == dbName).FirstOrDefault().DescriptorsCodes.FirstOrDefault().Id;

            var lstAllImages = GetDocumentsDb(dbName);

            var lstImages = lstAllImages.Where(x => x.ImageUrl.Contains(categoryName)).ToList();
            var searchedImage = lstImages.Where(x => x.ImageUrl.Replace("\\", "") == imageName.Replace("\\", "")).FirstOrDefault();
            var lstReturnedImages = searchImageProcessor.SearchSimilarImage(lstImages, searchedImage, metric, root, providedResults, selectedProcessing, descriptor);
            return Json(lstReturnedImages);
        }

        [System.Web.Http.Route("GetDescriptors/{dbName}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetDescriptors([FromUri] string dbName)
        {
            var lstReturnedDescriptors = ConfigurationSettings.GetImageDescriptorsPerDb(dbName);
            return Json(lstReturnedDescriptors);
        }

        [System.Web.Http.Route("GetMetrics/{descriptorName}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetMetrics([FromUri] string descriptorName)
        {

            var lstReturnedDescriptors = ConfigurationSettings.GetImageMetricsPerDb(descriptorName);
            return Json(lstReturnedDescriptors);
        }

        [System.Web.Http.Route("GetDatabases")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetDatabases()
        {

            var lstReturnedDescriptors = ConfigurationSettings.GetDatabases();
            return Json(lstReturnedDescriptors);
        }

        [System.Web.Http.Route("AllImages3D/{dbName}/{numberOfImages}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetAllImage3D(string dbName, int numberOfImages)
        {
            int numberOfShownImages = numberOfImages;
            
            var lstImages = GetDocumentsDb(dbName);
            var descriptorFile = "PCA";

            try
            {
                var lstReturnedImages = lstImages.Select(obj => new DocumentPCAInfo()
                {
                    x = obj.LstDescriptors[descriptorFile][0],
                    y = obj.LstDescriptors[descriptorFile][1],
                    z = obj.LstDescriptors[descriptorFile][2],
                    image = "../" + obj.Root + "/" + obj.DocumentName
                });

                int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                var rnd = new Random(unixTime);
                return Json(lstReturnedImages.OrderBy(a => rnd.Next()).Take(numberOfShownImages));
            }
            catch (Exception ex)
            {
                return Json(ex);
            }

        }

    }
}
