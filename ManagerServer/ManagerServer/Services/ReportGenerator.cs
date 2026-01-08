using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ReportGenerator
{
    public byte[] GenerateReport(ReportData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text($"System Report - {DateTime.Now}").FontSize(20).Bold();

                page.Content().Column(col =>
                {
                    foreach (var section in data.Sections)
                    {
                        col.Item().Text(section.Title).FontSize(16).Bold();
                        col.Item().Text($"Średnia: {section.Average:F2}");
                        col.Item().Text($"Min: {section.Min:F2}");
                        col.Item().Text($"Max: {section.Max:F2}");
                        col.Item().Text($"Ostatnia wartość: {section.Last:F2}");
                        col.Item().LineHorizontal(1);
                    }
                });
            });
        });

        return document.GeneratePdf();
    }
}

public class ReportData
{
    public List<ReportSection> Sections { get; set; } = new();
}

public class ReportSection
{
    public string Title { get; set; }
    public byte[] ChartPng { get; set; }
    public float Average { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }
    public float Last { get; set; }
}
