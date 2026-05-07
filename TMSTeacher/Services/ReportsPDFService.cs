using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TMS_SharedLibrary.Models;

namespace TMSTeacher.Services
{
    public class ReportsPDFService
    {
        public virtual byte[] GenerateBorrowHistoryPdf(IEnumerable<ToyLoan> loans, string studentName)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text($"Borrow History for {studentName}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(5);

                            // Table Header
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Toy Name").SemiBold();
                                row.RelativeItem().Border(1).Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Borrowed Date").SemiBold();
                                row.RelativeItem().Border(1).Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Due Date").SemiBold();
                                row.RelativeItem().Border(1).Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Returned Date").SemiBold();
                                row.RelativeItem().Border(1).Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Status").SemiBold();
                            });

                            // Table Rows
                            foreach (var item in loans)
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Border(1).Padding(5)
                                        .Text(item.Toy.Name);
                                    row.RelativeItem().Border(1).Padding(5)
                                        .Text(item.BorrowDate.ToString("yyyy-MM-dd"));
                                    row.RelativeItem().Border(1).Padding(5)
                                        .Text(item.DueDate.ToString("yyyy-MM-dd"));
                                    row.RelativeItem().Border(1).Padding(5)
                                        .Text(item.ReturnDate?.ToString("yyyy-MM-dd") ?? "-");

                                    var status = item.ReturnDate == null
                                        ? (item.DueDate < DateOnly.FromDateTime(DateTime.Today) ? "Overdue" : "Active")
                                        : "Returned";

                                    var statusColor = status == "Overdue" ? Colors.Red.Medium :
                                                     status == "Active" ? Colors.Orange.Medium :
                                                     Colors.Green.Medium;

                                    row.RelativeItem().Border(1).Padding(5)
                                        .Text(status).FontColor(statusColor).SemiBold();
                                });
                            }

                            if (!loans.Any())
                            {
                                column.Item().Text("No borrow history found for this student.")
                                    .Italic().FontColor(Colors.Grey.Medium);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on ");
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
