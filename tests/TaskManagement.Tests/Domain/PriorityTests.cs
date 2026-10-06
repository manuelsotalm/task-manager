namespace TaskManagement.Tests.Domain;

using FluentAssertions;
using TaskManagement.Domain.ValueObjects;
using Xunit;

public class PriorityTests
{
    [Theory]
    [InlineData(0, "Highest")]
    [InlineData(1, "High")]
    [InlineData(2, "Medium")]
    [InlineData(3, "Low")]
    [InlineData(4, "Lowest")]
    public void Constructor_AcceptsValidValues(int value, string expectedDisplay)
    {
        // Act
        var priority = new Priority(value);

        // Assert
        priority.Value.Should().Be(value);
        priority.ToDisplayString().Should().Be(expectedDisplay);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Constructor_Throws_ForInvalidValues(int value)
    {
        // Act & Assert
        var action = () => new Priority(value);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Priority must be between 0 and 4.*");
    }

    [Fact]
    public void ImplicitConversion_FromInt_Works()
    {
        // Act
        Priority priority = 2;

        // Assert
        priority.Value.Should().Be(2);
    }

    [Fact]
    public void ImplicitConversion_ToInt_Works()
    {
        // Arrange
        var priority = new Priority(3);

        // Act
        int value = priority;

        // Assert
        value.Should().Be(3);
    }

    [Fact]
    public void StaticProperties_ReturnCorrectValues()
    {
        Priority.Highest.Value.Should().Be(0);
        Priority.High.Value.Should().Be(1);
        Priority.Medium.Value.Should().Be(2);
        Priority.Low.Value.Should().Be(3);
        Priority.Lowest.Value.Should().Be(4);
    }

    [Fact]
    public void ToString_ReturnsDisplayString()
    {
        var priority = new Priority(1);
        priority.ToString().Should().Be("High");
    }

    [Fact]
    public void Equality_WorksCorrectly()
    {
        var p1 = new Priority(2);
        var p2 = new Priority(2);
        var p3 = new Priority(3);

        p1.Should().Be(p2);
        p1.Should().NotBe(p3);
        (p1 == p2).Should().BeTrue();
        (p1 != p3).Should().BeTrue();
    }
}