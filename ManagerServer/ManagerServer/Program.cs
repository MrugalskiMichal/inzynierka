using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Add services to the container.

// Replace the placeholder with your connection string.
var uri = "mongodb://localhost:27017/";
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(uri));

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("timeseriesdb");
});
try
{
    //w mongo database jest tym samym co w sql, ale tabele nazywaj� si� kolekcje
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

    //jak nie ma stw�rz kolekcj�
    db.CreateCollection("metrics", options);
}
catch (MongoException me)
{
    Console.Error.WriteLine(me.Message);
}

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5037", "https://localhost:5037")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<AlertRepository>();
builder.Services.AddSingleton<MetricsRepository>();
builder.Services.AddSingleton<AlertEvaluatorService>();
builder.Services.AddSingleton<EmailSender>();
builder.Services.AddHostedService<AlertBackgroundService>();
builder.Services.AddSingleton<ReportGenerator>();
builder.Services.AddHostedService<ReportBackgroundService>();
builder.Services.AddHostedService<MetricsCleanupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
