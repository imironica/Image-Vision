using ImageSearchEngine.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageSearchEngine.DTO.Interfaces;
using System.Configuration;
using MongoDB.Driver;

namespace ImageSearchEngine.Core
{
    public class DescriptorManagerMongoDb : IDescriptorManager
    {
        public void WriteImageDescriptor(string filePath, List<DocumentInfo> lstImages)
        {
            string db = ConfigurationManager.AppSettings["db"].ToString();
            string connectionString = ConfigurationManager.AppSettings["dbIp"].ToString();
            // Create a MongoClient object by using the connection string
            var client = new MongoClient(connectionString);

            //Use the MongoClient to access the server
            var database = client.GetDatabase(db);
            var collection = database.GetCollection<DocumentInfo>(filePath);
            collection.InsertMany(lstImages);
        }

        public List<DocumentInfo> GetImageDescriptor(string filePath)
        {
            string db = ConfigurationManager.AppSettings["db"].ToString();
            string connectionString = ConfigurationManager.AppSettings["dbIp"].ToString();
            // Create a MongoClient object by using the connection string
            var client = new MongoClient(connectionString);

            //Use the MongoClient to access the server
            var database = client.GetDatabase(db);
            var collection = database.GetCollection<DocumentInfo>(filePath);

            var values = collection.AsQueryable<DocumentInfo>().ToList();

            return values;
        }

    
    }
}
