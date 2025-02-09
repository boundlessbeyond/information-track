using System.Collections.Generic;
using FooBooLooGame.Server.Data;
using FooBooLooGame.Server.Dtos;

namespace FooBooLooGame.Server;

public static class Mappers
{
    public static NumberToWord ToEntity(this NumberToWordDto dto)
    {
        return new NumberToWord(dto.Number, dto.Word);
    }

    public static string[] GameRules(this UserGame game)
    {
        var gameRules = new List<string>();
        gameRules.Add("The rules of the game:");
        foreach (var gameRule in game.GameRules)
        {
            gameRules.Add($"Replace any number divisible by {gameRule.Key} with \"{gameRule.Value}\"");
        }
        gameRules.Add("You score one point for each correct answer.");
        gameRules.Add($"You have {game.TimeLimitInSeconds} seconds to score as many points as you can.");
        gameRules.Add("Good luck!");
        return gameRules.ToArray();
    }
}