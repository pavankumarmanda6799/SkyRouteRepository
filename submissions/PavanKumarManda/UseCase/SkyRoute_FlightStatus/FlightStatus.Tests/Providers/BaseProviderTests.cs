using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;
using FlightStatus.Api.Providers;

namespace FlightStatus.Tests.Providers
{
    public class BaseProviderTests
    {
        private class TestModel
        {
            public int Value { get; set; }
        }

        private class TestProvider : BaseProvider<TestModel>
        {
            public TestProvider(IWebHostEnvironment env, string fileName) : base(env, fileName) { }

            public Task<IEnumerable<TestModel>> ReadAllPublicAsync() => ReadAllAsync();
        }

        [Fact]
        public async Task ReadAllAsync_FileDoesNotExist_ReturnsEmpty()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(e => e.ContentRootPath).Returns(tempDir);

            var provider = new TestProvider(envMock.Object, "missing.json");

            // Act
            var result = await provider.ReadAllPublicAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task ReadAllAsync_FileExistsWithJson_ReturnsDeserializedItems_CaseInsensitive()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var dataDir = Path.Combine(tempDir, "Data");
            Directory.CreateDirectory(dataDir);

            var fileName = "items.json";
            var filePath = Path.Combine(dataDir, fileName);

            // JSON uses lowercase property name to validate case-insensitive deserialization
            var json = "[ { \"value\": 1 }, { \"VALUE\": 2 } ]";
            await File.WriteAllTextAsync(filePath, json);

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(e => e.ContentRootPath).Returns(tempDir);

            var provider = new TestProvider(envMock.Object, fileName);

            // Act
            var items = await provider.ReadAllPublicAsync();

            // Assert
            Assert.NotNull(items);
            var list = new List<TestModel>(items);
            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0].Value);
            Assert.Equal(2, list[1].Value);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task ReadAllAsync_FileExistsWithNullJson_ReturnsEmpty()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var dataDir = Path.Combine(tempDir, "Data");
            Directory.CreateDirectory(dataDir);

            var fileName = "items.json";
            var filePath = Path.Combine(dataDir, fileName);

            await File.WriteAllTextAsync(filePath, "null");

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(e => e.ContentRootPath).Returns(tempDir);

            var provider = new TestProvider(envMock.Object, fileName);

            // Act
            var items = await provider.ReadAllPublicAsync();

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);

            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}
