using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var MONGODB_URI = Environment.GetEnvironmentVariable("MONGODB_URI") ?? "mongodb://127.0.0.1:27017";
var DB_NAME = Environment.GetEnvironmentVariable("DB_NAME") ?? "agentsInzynierka";

var client = new MongoClient(MONGODB_URI);
var db = client.GetDatabase(DB_NAME);
var agentsCol = db.GetCollection<BsonDocument>("agents");

// serve static files from ../public
var publicDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "public");
if (Directory.Exists(publicDir))
{
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = new PhysicalFileProvider(publicDir) });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(publicDir) });
}

object ToPlain(BsonValue v)
{
    if (v == null || v.IsBsonNull) return null!;
    if (v.IsBoolean) return v.AsBoolean;
    if (v.IsInt32) return v.AsInt32;
    if (v.IsInt64) return v.AsInt64;
    if (v.IsDouble) return v.AsDouble;
    if (v.IsString) return v.AsString;
    if (v.IsBsonDocument)
    {
        var doc = v.AsBsonDocument;
        return doc.ToDictionary(p => p.Name, p => ToPlain(p.Value));
    }
    if (v.IsBsonArray)
    {
        return v.AsBsonArray.Select(ToPlain).ToList();
    }
    return v.ToString();
}

Dictionary<string, object?> Flatten(BsonDocument doc)
{
    var result = new Dictionary<string, object?>();
    foreach (var el in doc.Elements)
    {
        if (el.Name == "_id") result["_id"] = el.Value.ToString();
        else if (el.Name == "metrics") continue;
        else result[el.Name] = ToPlain(el.Value);
    }

    var metricKeys = new[] { "cpuUsage", "cpuTemperature", "ramUsage", "gpuUsage", "gpuTemperature", "diskUsage", "fanSpeeds" };
    var m = doc.Contains("metrics") && doc["metrics"].IsBsonDocument ? doc["metrics"].AsBsonDocument : new BsonDocument();

    foreach (var k in metricKeys)
    {
        if (!result.ContainsKey(k))
        {
            if (m.Contains(k))
            {
                var val = ToPlain(m[k]);
                if (val is string s)
                {
                    if (bool.TryParse(s, out var b)) result[k] = b;
                    else result[k] = s;
                }
                else if (val is long ln) result[k] = ln == 1;
                else if (val is int i) result[k] = i == 1;
                else if (val is double dbv) result[k] = Math.Abs(dbv - 1.0) < 1e-9;
                else result[k] = val;
            }
            else result[k] = null;
        }
    }

    return result;
}

app.MapGet("/api/agents", async (HttpContext ctx) =>
{
    var limit = 100;
    if (int.TryParse(ctx.Request.Query["limit"], out var q)) limit = Math.Min(100, q);
    var docs = await agentsCol.Find(Builders<BsonDocument>.Filter.Empty).Limit(limit).ToListAsync();
    var list = docs.Select(Flatten).ToList();
    return Results.Json(list);
});

app.MapGet("/api/agents/{agentId}", async (string agentId) =>
{
    var filter = Builders<BsonDocument>.Filter.Eq("agentId", agentId);
    var doc = await agentsCol.Find(filter).FirstOrDefaultAsync();
    if (doc == null) return Results.NotFound();
    return Results.Json(Flatten(doc));
});

// SPA fallback
app.MapFallback(async ctx =>
{
    var index = Path.Combine(publicDir, "index.html");
    if (File.Exists(index))
    {
        ctx.Response.ContentType = "text/html";
        await ctx.Response.SendFileAsync(index);
    }
    else ctx.Response.StatusCode = 404;
});

app.Run();
