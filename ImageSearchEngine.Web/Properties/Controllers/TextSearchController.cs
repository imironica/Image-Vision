using ImageSearchEngine.Core;
using ImageSearchEngine.DTO;
using ImageSearchEngine.DTO.Enum;
using ImageSearchEngine.DTO.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ImageSearchEngine.Web.Controllers
{
    [RoutePrefix("api/searchEngine")]
    public class TextSearchController : ApiController
    {

        public IDescriptorManager descriptorManager;
        public TextSearchController()
        {
            descriptorManager = new DescriptorManagerSimple();
        }


        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

       
        public string Get(int id)
        {
            return "value";
        }

 
        [Route("Text")]
        [HttpPost]
        public IHttpActionResult Text([FromBody] TextSearchDTO query)
        {
            string text = query.Text;
            if (string.IsNullOrEmpty(text.ToString()))
                return Json(SentimentEnum.Neutral);
            List<string> lstPositiveValues = new List<string>();
            List<string> lstNegativeValues = new List<string>();
            using (var sr = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/positive-words.txt")))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lstPositiveValues.Add(line.Trim().ToLower());
                }
            }
            using (var sr = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/negative-words.txt")))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lstNegativeValues.Add(line.Trim().ToLower());
                }
            }

            var result = SentimentClassification.Clasify(text.ToString(), lstPositiveValues, lstNegativeValues);
            return Json(result);

        }
        [System.Web.Http.Route("GetTopics")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetTopics()
        {
            List<string> lstTopics = new List<string>();
            lstTopics.Add("Computers");
            lstTopics.Add("Routers");
            lstTopics.Add("Speakers");
            return Json(lstTopics);
        }

        [System.Web.Http.Route("AllTexts/{topic}/{orderOption}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetTexts([FromUri]string topic, [FromUri] string orderOption)
        {
            var numberOfShownImages = 50;
            List<TextInfo> lstDocs = new List<TextInfo>();
            try
            {
                var path = "";
                if(topic == "Computers")
                    path = System.Web.Hosting.HostingEnvironment.MapPath("~/textDescriptorsComputer.txt");
                if (topic == "Routers")
                    path = System.Web.Hosting.HostingEnvironment.MapPath("~/textDescriptorsRouter.txt");
                if (topic == "Speakers")
                    path = System.Web.Hosting.HostingEnvironment.MapPath("~/textDescriptorsSpeaker.txt");

                if (HttpContext.Current.Application[path] == null)
                    HttpContext.Current.Application[path] = descriptorManager.GetTextDescriptor(path);
                lstDocs = (List<TextInfo>)HttpContext.Current.Application[path];

                var lstReturned = lstDocs.Select(x => new TextInfo()
                {
                    Text = x.Text,
                    Sentiment = x.Sentiment
                });

                if (orderOption == "1")
                {
                    int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                    var rnd = new Random(unixTime);
                    return Json(lstReturned.OrderBy(a => rnd.Next()).Take(numberOfShownImages));
                }
                if (orderOption == "2")
                {
                    return Json(lstReturned.Where(x => x.Sentiment == SentimentEnum.Positive).OrderByDescending(a => a.PositiveScore).Take(numberOfShownImages));
                }
                if (orderOption == "3")
                {
                    return Json(lstReturned.Where(x => x.Sentiment == SentimentEnum.Negative).OrderByDescending(a => a.NegativeScore).Take(numberOfShownImages));
                }

                return Json(lstReturned.Take(numberOfShownImages));
            }
            catch (Exception ex)
            {
                return Json(ex);
            }

        }


 
    }
}