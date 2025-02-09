using System.Collections.Generic;
using FluentAssertions;
using FooBooLooGame.Server.Data;
using Xunit;

namespace FooBooLooGame.Unit.Tests;

public sealed class UserGameAnswerTests
{
    [Theory]
    [InlineData(7, "Foo", true)]
    [InlineData(14, "Foo", true)]
    [InlineData(11, "Boo", true)]
    [InlineData(22, "Boo", true)]
    [InlineData(77, "FooBoo", true)]
    [InlineData(103, "Loo", true)]
    [InlineData(206, "Loo", true)]
    [InlineData(1, "", false)]
    [InlineData(15, "", false)]
    [InlineData(1, "Foo", false)]
    // these random numbers are invalid and should not be able to happen and thus return false
    [InlineData(1110, "Foo", false)]
    [InlineData(700, "Foo", false)]
    public void GameOtherTest(int randomNumber, string answer, bool expected)
    {
        // ARRANGE
        var gameRules = new List<NumberToWord>
        {
            new(7, "Foo"),
            new(11, "Boo"),
            new(103, "Loo")
        };
        var game = UserGame.Create("GAME", 0, 210, 1000, gameRules.ToArray());
        game.SetNumber(randomNumber);

        // ACT
        var sut = game.ValidateAnswer(answer);
        // ASSERT

        sut.Should().Be(expected);

        if (expected)
        {
            game.CorrectAnswerCount.Should().Be(1);
            game.IncorrectAnswerCount.Should().Be(0);
        }
        else
        {
            game.CorrectAnswerCount.Should().Be(0);
            game.IncorrectAnswerCount.Should().Be(1);
        }
    }
}