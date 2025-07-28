using AiRealEstate.Core.Models;
using AiRealEstate.Infrastructure.Services;
using Xunit;

namespace AiRealEstate.Tests.InfrastructureTests
{
    public class QueryBuilderServiceTests
    {
        [Fact]
        public async Task BuildUrlAsync_NullPreferences_ReturnsEmptyString()
        {
            var service = new QueryBuilderService();
            var result = await service.BuildUrlAsync(null);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task BuildUrlAsync_MissingCountyOrCity_ReturnsEmptyString()
        {
            var service = new QueryBuilderService();
            var prefs = new UserPreferences { City = "Cluj-Napoca" }; // Missing County
            var result = await service.BuildUrlAsync(prefs);
            Assert.Equal(string.Empty, result);

            prefs = new UserPreferences { County = "Cluj" }; // Missing City
            result = await service.BuildUrlAsync(prefs);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task BuildUrlAsync_ValidPreferences_ReturnsSearchUrl()
        {
            var service = new QueryBuilderService();
            var prefs = new UserPreferences
            {
                PropertyType = "apartament",
                TransactionType = "vanzare",
                County = "Cluj",
                City = "Cluj-Napoca",
                MaxPrice = 100000,
                TextFilter = "central"
            };
            var result = await service.BuildUrlAsync(prefs);
            Assert.StartsWith("https://www.romimo.ro/", result);
            Assert.EndsWith(".html", result);
        }
    }
}
