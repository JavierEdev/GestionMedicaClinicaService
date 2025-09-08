using GestionClinica.Domain.Services;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GestionClinica.Infrastructure.Logging;

public class MongoSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "ClinicaCitas";
    public string Collection { get; set; } = "AuditLogs";
}

public class MongoAuditLogService : IAuditLogService
{
    private readonly IMongoCollection<BsonDocument> _col;
    public MongoAuditLogService(IOptions<MongoSettings> opt)
    {
        var client = new MongoClient(opt.Value.ConnectionString);
        _col = client.GetDatabase(opt.Value.Database).GetCollection<BsonDocument>(opt.Value.Collection);
    }
    public Task WriteAsync(string area, string action, object payload)
    {
        var doc = new BsonDocument {
            {"timestamp", DateTime.UtcNow}, {"area", area}, {"action", action},
            {"payload", BsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(payload))}
        };
        return _col.InsertOneAsync(doc);
    }
}