using System;

namespace FooBooLooGame.Server.Dtos;

public record CreateGameDto(string GameName, int MinNumber, int MaxNumber, int TimeLimit, NumberToWordDto[] NumbersToWords);

public record NumberToWordDto(int Number, string Word);

public record SubmitAnswer(Guid GameId, string Answer);
