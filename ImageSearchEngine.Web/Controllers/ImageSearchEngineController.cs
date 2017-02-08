using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ImageSearchEngine.DTO;
using ImageSearchEngine.Services;
using ImageSearchEngine.DTO.Interfaces;
using ImageSearchEngine.Core;
using System.Web;

namespace TestAccord.Controllers
{
    [RoutePrefix("api/searchEngine")]
    public class ImageSearchEngineController : ApiController
    {
        public IDescriptorManager descriptorManager;
        public ISearchImageProcessor searchImageProcessor;
        private SearchEngineService _serviceEngine;
        private ICacheManager _cacheManager;

        public ImageSearchEngineController(IDescriptorManager descriptorManagerType, ICacheManager cacheManager)
        {
            descriptorManager = descriptorManagerType;
            searchImageProcessor = new SearchImageProcessor();
            _cacheManager = cacheManager;
            _serviceEngine = new SearchEngineService(descriptorManagerType, _cacheManager);
        }

        /// <summary>
        /// Show random images from the database
        /// </summary>
        [System.Web.Http.Route("AllImages/{dbName}/{numberOfImages}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetAllImage(string dbName, int numberOfImages)
        {
            try
            {
                var docs = _serviceEngine.GetAllImage(dbName, numberOfImages);
                if (docs != null)
                    return Json(docs);
                return Json(string.Empty);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        /// <summary>
        /// Search for similar images in the database
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
            try
            {
                var docs = _serviceEngine.GetImage(imageName, dbName, metric, descriptor, providedResults, selectedProcessing);
                if (docs != null)
                    return Json(docs);
                return Json(string.Empty);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        /// <summary>
        /// Search a similar image in the database
        /// </summary>
        [System.Web.Http.Route("GetImagesByConcept")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult GetImagesByConcept([FromBody] Query query)
        {
            try
            {
                var docs = _serviceEngine.GetImagesByConcept(query);
                if (docs != null)
                    return Json(docs);
                return Json(string.Empty);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        /// <summary>)
        /// Search a similar image from a link
        /// </summary>
        [System.Web.Http.Route("SearchImageLink")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult SearchImageLink([FromBody] Query query)
        {
            try
            {
                var docs = _serviceEngine.SearchImageLink(query);
                if (docs != null)
                    return Json(docs);
                return Json(string.Empty);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        /// <summary>
        /// Perform a relevance feedback algorithm
        /// </summary>
        [System.Web.Http.Route("SearchImageRF")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult SearchImageRF([FromBody] Query rfQuery)
        {
            try
            {
                var docs = _serviceEngine.SearchImageRF(rfQuery);
                if (docs != null)
                    return Json(docs);
                return Json(string.Empty);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }
 
        /// <summary>
        /// Search images from the same folder
        /// </summary>
        [System.Web.Http.Route("SearchImagesSameEvent/{imageName}/{dbName}/{metric}/{descriptor}/{providedResults}/{selectedProcessing}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult SearchImagesSameEvent([FromUri] string imageName, [FromUri] string dbName, [FromUri] string metric, [FromUri] string descriptor, [FromUri] int providedResults, [FromUri] string selectedProcessing)
        {
            try
            {
                var docs = _serviceEngine.SearchImagesSameEvent(imageName, dbName, metric, descriptor, providedResults, selectedProcessing);
                if (docs != null)
                    return Json(docs);
                return Json(string.Empty);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        [System.Web.Http.Route("GetDescriptors/{dbName}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetDescriptors([FromUri] string dbName)
        {
            var lstReturnedDescriptors = _serviceEngine.GetDescriptors(dbName);
            return Json(lstReturnedDescriptors);
        }

        [System.Web.Http.Route("GetMetrics/{descriptorName}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetMetrics([FromUri] string descriptorName)
        {

            var lstReturnedDescriptors = _serviceEngine.GetMetrics(descriptorName);
            return Json(lstReturnedDescriptors);
        }

        [System.Web.Http.Route("GetDatabases")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetDatabases()
        {

            var lstReturnedDescriptors = _serviceEngine.GetDatabases();
            return Json(lstReturnedDescriptors);
        }

        [System.Web.Http.Route("AllImages3D/{dbName}/{numberOfImages}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetAllImage3D(string dbName, int numberOfImages)
        {
            try
            {
                var docs = _serviceEngine.GetAllImage3D(dbName, numberOfImages);
                if (docs != null)
                    return Json(docs);
                return Json(string.Empty);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

    }
}
