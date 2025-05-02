using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using WS381219_OOP;

namespace WS381219_OOP.Tests
{
    public class GAUtilsTests
    {
        [Fact]
        public void Shuffle_ContainSameElements()
        {
            // Arrange
            var originalList = new List<int> { 1, 2, 3, 4, 5 };
            var shuffledList = new List<int>(originalList);
            var random = new Random();

            // Act
            GAUtils.Shuffle(shuffledList, random);

            // Assert
            Assert.Equal(originalList.Count, shuffledList.Count);
            Assert.True(originalList.All(shuffledList.Contains));
        }

        [Fact]
        public void Shuffle_ChangesOrder()
        {
            // Arrange
            var originalList = new List<int> { 1, 2, 3, 4, 5 };
            var shuffledList = new List<int>(originalList);
            var random = new Random();

            // Act
            GAUtils.Shuffle(shuffledList, random);

            // Assert
            Assert.NotEqual(originalList, shuffledList); // Order should change most of the time
        }

        [Fact]
        public void Shuffle_EmptyList_ShouldNotThrow()
        {
            // Arrange
            var emptyList = new List<int>();
            var random = new Random();

            // Act & Assert
            var exception = Record.Exception(() => GAUtils.Shuffle(emptyList, random));
            Assert.Null(exception); // No exception should be thrown
        }

        [Fact]
        public void Shuffle_SingleElementList_RemainsUnchanged()
        {
            // Arrange
            var singleElementList = new List<int> { 42 };
            var random = new Random();

            // Act
            GAUtils.Shuffle(singleElementList, random);

            // Assert
            Assert.Single(singleElementList);
            Assert.Equal(42, singleElementList[0]); // Single element should remain unchanged
        }
    }
}