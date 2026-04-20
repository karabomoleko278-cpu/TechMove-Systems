using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using TechMoveSystems.Models;
using TechMoveSystems.Services;
using Xunit;

namespace TechMove.Tests;

public class BusinessLogicTests
{
    private readonly CurrencyCalculator _currencyCalculator = new();
    private readonly WorkflowService _workflowService = new();

    [Theory]
    [InlineData(10, 19.2456, 192.46)]
    [InlineData(0, 19.2456, 0)]
    public void CurrencyMath_ShouldBePrecise(decimal usd, decimal rate, decimal expected)
    {
        var result = _currencyCalculator.ConvertUsdToZar(usd, rate);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-1, 19)]
    [InlineData(12, 0)]
    public void CurrencyMath_ShouldRejectInvalidInputs(decimal usd, decimal rate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _currencyCalculator.ConvertUsdToZar(usd, rate));
    }

    [Fact]
    public void FileValidation_ShouldRejectExe()
    {
        var service = new FileStorageService(new TestEnvironment());
        var file = CreateFormFile("malicious.exe");

        var isValid = service.IsValidPdf(file);

        Assert.False(isValid);
    }

    [Fact]
    public void FileValidation_ShouldAcceptPdf()
    {
        var service = new FileStorageService(new TestEnvironment());
        var file = CreateFormFile("signed-agreement.pdf");

        var isValid = service.IsValidPdf(file);

        Assert.True(isValid);
    }

    [Fact]
    public void Workflow_ShouldBlockExpiredContract()
    {
        var contract = new Contract
        {
            Status = ContractStatus.Expired,
            EndDate = DateTime.Today.AddDays(-1)
        };

        var error = _workflowService.GetCreateRequestError(contract);

        Assert.NotNull(error);
    }

    [Fact]
    public void Workflow_ShouldAllowActiveContract()
    {
        var contract = new Contract
        {
            Status = ContractStatus.Active,
            EndDate = DateTime.Today.AddDays(30)
        };

        var canCreate = _workflowService.CanCreateRequest(contract);

        Assert.True(canCreate);
    }

    private static IFormFile CreateFormFile(string fileName)
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        return new FormFile(stream, 0, stream.Length, "file", fileName);
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "TechMove.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
