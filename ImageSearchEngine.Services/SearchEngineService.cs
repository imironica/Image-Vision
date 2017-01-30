using ImageSearchEngine.Core;
using ImageSearchEngine.DTO;
using ImageSearchEngine.DTO.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.Services
{
    public class SearchEngineService
    {
        public IDescriptorManager descriptorManager;
        public ISearchImageProcessor searchImageProcessor;

        public SearchEngineService()
        {
            descriptorManager = new DescriptorManagerSimple();
            searchImageProcessor = new SearchImageProcessor();
        }

        public List<DocumentInfo> GetRandomImages(string dbName, int numberOfImages)
        {
            int numberOfShownImages = numberOfImages;
            List<DocumentInfo> lstImages = new List<DocumentInfo>();
            string root = ConfigurationSettings.GetDatabases().Where(x => x.Code == dbName).FirstOrDefault().Folder;

            var descriptorFile = ConfigurationSettings.GetDatabases().Where(x => x.Code == dbName).FirstOrDefault().LstDocumentDescriptorFiles.FirstOrDefault().FileName;
            //TODO
            var path = descriptorFile;

            lstImages = descriptorManager.GetImageDescriptor(path);

            var lstReturnedImages = lstImages.Select(x => new DocumentInfo()
            {
                Description = x.Description,
                Root = root,
                ImageUrl = x.ImageUrl,
                Distance = x.Distance
            }).OrderBy(x => x.Distance);

            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            var rnd = new Random(unixTime);
            return lstReturnedImages.OrderBy(a => rnd.Next()).Take(numberOfShownImages).ToList();
        }
    }
}
