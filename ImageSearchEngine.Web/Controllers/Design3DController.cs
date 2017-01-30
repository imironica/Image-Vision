using ImageSearchEngine.Core;
using ImageSearchEngine.DTO;
using ImageSearchEngine.DTO.Enum;
using ImageSearchEngine.DTO.Interfaces;
using ImageSearchEngine.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ImageSearchEngine.Web.Controllers
{
    [RoutePrefix("api/searchEngine")]
    public class Design3DController : ApiController
    {

        public IDescriptorManager descriptorManager;
        public Design3DController()
        {
            descriptorManager = new DescriptorManagerSimple();
        }




    }
}