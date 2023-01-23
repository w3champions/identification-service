using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using W3ChampionsIdentificationService.Config;
using W3ChampionsIdentificationService.DatabaseModels;

namespace W3ChampionsIdentificationService
{
    public class MongoDbRepositoryBase
    {
        private readonly MongoClient _mongoClient;
        private readonly IAppConfig _appConfig;

        public MongoDbRepositoryBase(MongoClient mongoClient, IAppConfig appConfig)
        {
            _appConfig = appConfig;
            _mongoClient = mongoClient;
        }

        protected IMongoDatabase CreateClient()
        {
            var database = _mongoClient.GetDatabase(_appConfig.DatabaseName);
            return database;
        }

        protected Task<T> LoadFirst<T>(Expression<Func<T, bool>> expression)
        {
            var mongoCollection = CreateCollection<T>();
            return mongoCollection.Find(expression).FirstOrDefaultAsync();
        }

        protected Task<T> LoadFirst<T>(string id) where T : IIdentifiable
        {
            return LoadFirst<T>(x => x.Id.ToLower() == id.ToLower());
        }

        protected Task Insert<T>(T element)
        {
            var mongoCollection = CreateCollection<T>();
            return mongoCollection.InsertOneAsync(element);
        }

        protected async Task<List<T>> LoadAll<T>(Expression<Func<T, bool>> expression = null, int? limit = null, int? offset = null)
        {
            if (expression == null) expression = l => true;
            var mongoCollection = CreateCollection<T>();
            return await mongoCollection
                .Find(expression)
                .Skip(offset)
                .Limit(limit)
                .ToListAsync();
        }

        protected Task<List<T>> LoadSince<T>(DateTimeOffset since) where T : IVersionable
        {
            return LoadAll<T>(m => m.LastUpdated > since);
        }

        protected IMongoCollection<T> CreateCollection<T>(string collectionName = null)
        {
            var mongoDatabase = CreateClient();
            var mongoCollection = mongoDatabase.GetCollection<T>((collectionName ?? typeof(T).Name));
            return mongoCollection;
        }

        protected async Task Upsert<T>(T insertObject, Expression<Func<T, bool>> identityQuerry)
        {
            var mongoDatabase = CreateClient();
            var mongoCollection = mongoDatabase.GetCollection<T>(typeof(T).Name);
            await mongoCollection.FindOneAndReplaceAsync(
                identityQuerry,
                insertObject,
                new FindOneAndReplaceOptions<T> { IsUpsert = true });
        }

        protected Task UpsertTimed<T>(T insertObject, Expression<Func<T, bool>> identityQuerry) where T : IVersionable
        {
            insertObject.LastUpdated = DateTimeOffset.UtcNow;
            return Upsert(insertObject, identityQuerry);
        }

        protected Task Upsert<T>(T insertObject) where T : IIdentifiable
        {
            return Upsert(insertObject, x => x.Id == insertObject.Id);
        }

        protected Task UpsertMany<T>(List<T> insertObject) where T : IIdentifiable
        {
            if (!insertObject.Any()) return Task.CompletedTask;

            var collection = CreateCollection<T>();
            var bulkOps = insertObject
                .Select(record => new ReplaceOneModel<T>(Builders<T>.Filter
                .Where(x => x.Id == record.Id), record)
                { IsUpsert = true })
                .Cast<WriteModel<T>>().ToList();
            return collection.BulkWriteAsync(bulkOps);
        }

        protected async Task Delete<T>(Expression<Func<T, bool>> deleteQuery)
        {
            var mongoDatabase = CreateClient();
            var mongoCollection = mongoDatabase.GetCollection<T>(typeof(T).Name);
            await mongoCollection.DeleteOneAsync<T>(deleteQuery);
        }

        protected Task Delete<T>(string id) where T : IIdentifiable
        {
            return Delete<T>(x => x.Id == id);
        }

        protected async Task UnsetOne<T>(FieldDefinition<T> fieldName, string id) where T : IIdentifiable
        {
            // $unset
            var mongoDatabase = CreateClient();
            var mongoCollection = mongoDatabase.GetCollection<T>(typeof(T).Name);
            var updateDefinition = Builders<T>.Update.Unset(fieldName);
            await mongoCollection.UpdateOneAsync(x => x.Id == id, updateDefinition);
        }
    }
}