using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FooBooLooGame.Server.Data;
using Xunit;
using Assert = Xunit.Assert;

namespace FooBooLooGame.Unit.Tests;

public sealed class UserGameTests
{
    [Theory]
    [InlineData("Test Game", 1, 99, 100, 10, "Foo")]
    [InlineData("Game", 11, 120, 1000, 101, "Boo")]
    public void GameTest(string name, int min, int max, int timeLimitInSeconds, int gameRuleKey, string gameRuleValue)
    {
        // ARRANGE
        var gameRules = new Dictionary<int, string>() { { gameRuleKey, gameRuleValue } };
        
        // ACT
        var sut = UserGame.Create(name, min, max, timeLimitInSeconds, [new NumberToWord(gameRuleKey, gameRuleValue)]);

        // ASSERT
        sut.GameName.Should().Be(name);
        sut.GameStartedAt.Should().BeNull();
        sut.CorrectAnswerCount.Should().Be(0);
        sut.NumberRangeMax.Should().Be(max);
        sut.NumberRangeMin.Should().Be(min);
        sut.TimeLimitInSeconds.Should().Be(timeLimitInSeconds);
        sut.GameRules.Should().HaveCount(1);
        Assert.Equal(gameRules, sut.GameRules);
    }
    
    [Fact]
    public async Task GameClockCannotBeReset()
    {
        // ARRANGE
        var sut = UserGame.Create("Test Game", 1, 99, 100, [new NumberToWord(10, "Foo")]);
        
        // ACT
        sut.Start();
        
        // ASSERT
        sut.GameStartedAt.Should().NotBeNull();
        sut.GameStartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(100));
        var startedAt = sut.GameStartedAt!.Value;
        await Task.Delay(1000);
        
        sut.Start();
        sut.GameStartedAt.Value.Should().Be(startedAt);
    }
    
    [Fact]
    public async Task GameExpired()
    {
        // ARRANGE
        var sut = UserGame.Create("Test Game", 1, 99, 1, [new NumberToWord(10, "Foo")]);
        
        // ACT
        sut.Start();
        await Task.Delay(1001);
        
        // ASSERT
        
        sut.IsGameExpired.Should().BeTrue();
    }
     
}