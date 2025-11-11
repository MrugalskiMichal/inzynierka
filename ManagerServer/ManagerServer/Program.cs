using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Replace the placeholder with your connection string.
var uri = "mongodb://localhost:27017/";
try
{
    //w mongo database jest tym samym co w sql, ale tabele nazywaj¹ siê kolekcje
    var client = new MongoClient(uri);

    var db = client.GetDatabase("timeseriesdb");

    //tworzymy TSDB
    var timeSeriesOptions = new TimeSeriesOptions
    (
        timeField: "timestamp",
        metaField: "agentId",
        TimeSeriesGranularity.Seconds
    );
    var options = new CreateCollectionOptions
    {
        TimeSeriesOptions = timeSeriesOptions
    };

    //jak nie ma stwórz kolekcjê
    db.CreateCollection("metrics", options);
}
catch (MongoException me)
{
    Console.Error.WriteLine(me.Message);
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
