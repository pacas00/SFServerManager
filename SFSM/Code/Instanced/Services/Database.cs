using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace SFServerManager.Code.Instanced.Services
{

    //SINGLETON
    //THERE IS ONLY ONE
    public class DatabaseService
    {
        private readonly LiteDatabase db;
        private readonly IServiceProvider _services;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(IServiceProvider services, ILogger<DatabaseService> logger)
        {
            db = new LiteDatabase("DataStorage.db");
            _services = services;
            _logger = logger;
        }

        public ILiteCollection<T> GetCollection<T>(string CollectionName)
        {
            return db.GetCollection<T>(CollectionName);
        }

        public bool HasCollection(string CollectionName)
        {
            return db.CollectionExists(CollectionName);
        }

        public IEnumerable<T> FindAll<T>(string CollectionName)
        {
            return GetCollection<T>(CollectionName).FindAll();
        }
        public IEnumerable<T> Find<T>(string CollectionName, BsonExpression expression, int Skip = 0, int Max = Int32.MaxValue)
        {
            return GetCollection<T>(CollectionName).Find(expression, Skip, Max);
        }

        public BsonValue Insert<T>(string CollectionName, T Object)
        {
            return GetCollection<T>(CollectionName).Insert(Object);
        }
        
        public bool Update<T>(string CollectionName, T Object)
        {
            return GetCollection<T>(CollectionName).Update(Object);
        }
        
        public bool EnsureIndex<T>(string CollectionName, string IndexName, BsonExpression Expression, bool Unique = false)
        {
            return GetCollection<T>(CollectionName).EnsureIndex(IndexName, Expression, Unique);
        }

        public ILiteQueryable<T> Query<T>(string CollectionName)
        {
            return GetCollection<T>(CollectionName).Query();
        }
        
        public bool Delete<T>(string CollectionName, BsonValue ID)
        {
            return GetCollection<T>(CollectionName).Delete(ID);
        }
    }
}
