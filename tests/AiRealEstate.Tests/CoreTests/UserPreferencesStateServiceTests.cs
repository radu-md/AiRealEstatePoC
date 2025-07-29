using AiRealEstate.Core.Models;
using AiRealEstate.Core.Services;

namespace AiRealEstate.Tests.CoreTests
{
    public class UserPreferencesStateServiceTests
    {
        private readonly UserPreferencesStateService _service;
        private const string SessionId = "session1";

        public UserPreferencesStateServiceTests()
        {
            _service = new UserPreferencesStateService();
        }

        [Fact]
        public void GetPreferences_ReturnsDefault_WhenNotSet()
        {
            // Act
            var prefs = _service.GetUserPreferences(SessionId);

            // Assert
            Assert.NotNull(prefs);
        }

        [Fact]
        public void SetPreferences_UpdatesPreferences()
        {
            // Arrange
            var newPrefs = new UserPreferences
            {
                City = "Oradea",
                County = "Bihor",
                PropertyType = "Apartament",
                TransactionType = "vanzare",
                MaxPrice = 1000,
                TextFilter = "aproape de o scoala"
            };

            // Act
            _service.UpdatePreferences(SessionId, newPrefs);
            var result = _service.GetUserPreferences(SessionId);

            // Assert
            Assert.Equal(newPrefs.City, result.City);
            Assert.Equal(newPrefs.County, result.County);
            Assert.Equal(newPrefs.PropertyType, result.PropertyType);
            Assert.Equal(newPrefs.TransactionType, result.TransactionType);
            Assert.Equal(newPrefs.MaxPrice, result.MaxPrice);
            Assert.Equal(newPrefs.TextFilter, result.TextFilter);
        }
    }
}
