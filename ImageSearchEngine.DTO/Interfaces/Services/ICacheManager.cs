using ImageSearchEngine.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO.Interfaces
{
    public interface ICacheManager
    {
            List<DocumentInfo> GetDbData(string dbName);
            T GetItem<T>(string key);
            void InsertItem(string key, object value);
            bool Remove(string key);
    }
}
