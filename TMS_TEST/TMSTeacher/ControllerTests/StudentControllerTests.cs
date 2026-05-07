using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMS_SharedLibrary.Models;
using TMSTeacher.Controllers;
using TMSTeacher.Services;
using Xunit;
using Microsoft.Graph.Users.Item;

namespace TMS_TEST.TMSTeacher.ControllerTests
{
    public class StudentControllerTests
    {
        [Fact]
        public async Task Download_GeneratesCorrectFileNameAndType()
        {
            var mockStudentService = new Mock<IStudentAPIService>();
            var mockUserService = new Mock<IUserAPIService>();
            var mockRequestAdapter = new Mock<IRequestAdapter>();
            var graphClient = new GraphServiceClient(mockRequestAdapter.Object);
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var mockPdfService = new Mock<ReportsPDFService>();
            var mockLogger = new Mock<ILogger<StudentController>>();

            var student = new Student
            {
                StudentId = 1,
                User = new User { Oid = "oid-123" }
            };

            var loans = new List<ToyLoan>
            {
                new ToyLoan { LoanId = 1 }
            };

            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };

            mockStudentService.Setup(s => s.Get(1))
                .ReturnsAsync(student);

            mockStudentService.Setup(s => s.GetBorrowHistory(1))
                .ReturnsAsync(loans);

            var graphUser = new Microsoft.Graph.Models.User
            {
                GivenName = "John",
                Surname = "Doe"
            };

            mockRequestAdapter
                .Setup(x => x.SendAsync(
                    It.IsAny<RequestInformation>(),
                    It.IsAny<ParsableFactory<Microsoft.Graph.Models.User>>(),
                    It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(graphUser);

            mockPdfService.Setup(p => p.GenerateBorrowHistoryPdf(loans, "John Doe"))
                .Returns(pdfBytes);

            var controller = new StudentController(
                graphClient,
                mockTokenAcquisition.Object,
                mockStudentService.Object,
                mockLogger.Object,
                mockUserService.Object,
                mockPdfService.Object
            );

            var result = await controller.Download(1);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.StartsWith("BorrowHistory_John Doe_", fileResult.FileDownloadName);
        }
    }
}