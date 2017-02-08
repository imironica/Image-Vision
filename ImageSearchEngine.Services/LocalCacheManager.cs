using ImageSearchEngine.DTO;
using ImageSearchEngine.DTO.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ImageSearchEngine.Services
{
    public class LocalCacheManager : ICacheManager
    {
        IDescriptorManager _dbManager;
        private ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();
        private object sync = new object();

        public LocalCacheManager(IDescriptorManager dbManager)
        {
            _dbManager = dbManager;
        }

        private void SetDb(string key, object entry)
        {
            _cache[key] = entry;
        }

        public List<DocumentInfo> GetDbData(string dbName)
        {
            if (!_cache.ContainsKey(dbName))
                lock (sync)
                {
                    if (!_cache.ContainsKey(dbName))
                    {
                        var db = _dbManager.GetImageDescriptor(dbName);
                        if (db != null)
                        {
                            _cache[dbName] = db;
                        }
                        else
                        {
                            throw new Exception("The database couldn't be found!");
                        }
                    }
                }
            return (List<DocumentInfo>)_cache[dbName];
        }

        public T GetItem<T>(string key)
        {
            if (_cache.ContainsKey(key))
                return (T)_cache[key];
            else
                return default(T);
        }

        public void InsertItem(string key, object value)
        {
            _cache[key] = value;
        }

        public bool Remove(string key)
        {
            object item;
            return _cache.TryRemove(key, out item);
        }

    }
}
