using LocalizationTool.Scripts.Data;
using LocalizationTool.Scripts.General;
using NUnit.Framework;

namespace LocalizationTool.Scripts.Tests
{
    public class TestWhenAddingKeys
    {
        [SetUp]
        public void SetUp()
        {
            CacheData.ClearData();
            LocalizationManager.AddNewCategory("None");
            LocalizationManager.AddNewLanguage("English");
            LocalizationManager.AddNewLanguage("Spanish");
        }

        [TestCase(1000)]
        public void AddNewKey_RangeIsPositive_LenghtIsEqualsToRange(int range)
        {
            // Arrange

            // Act
            for (var i = 0; i < range; i++)
            {
                LocalizationManager.AddNewKey($"{i}", "None");
            }
            // Assert
            Assert.AreEqual(range, CacheData.Keys.Count);
        }
    }
}