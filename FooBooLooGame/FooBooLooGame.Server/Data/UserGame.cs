using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FooBooLooGame.Server.Data;

public class UserGame()
{
    public Guid GameId { get; }
    public string? GameName { get; }
    public int NumberRangeMin { get; }
    public int NumberRangeMax { get; }
    public int TimeLimitInSeconds { get; }
    public DateTime? GameStartedAt { get; private set; }
    private Dictionary<int, string> gameRules = new Dictionary<int, string>();
    public Dictionary<int, string> GameRules => gameRules;
    public int CorrectAnswerCount { get; private set; } = 0;
    public int IncorrectAnswerCount { get; private set; } = 0;
    private int NumberToValidate { get; set; } = 0;

    private UserGame(string gameName, int minRange, int maxRange, int timeLimitInSeconds,
        Dictionary<int, string> gameRules) : this()
    {
        this.GameId = Guid.NewGuid();
        this.GameName = gameName;
        this.NumberRangeMin = minRange;
        this.NumberRangeMax = maxRange;
        this.TimeLimitInSeconds = timeLimitInSeconds;
        this.gameRules = gameRules;
    }

    public static UserGame Create(string gameName, int minRange, int maxRange, int timeLimitInSeconds,
        NumberToWord[] gameRules)
    {
        ValidateGame(gameName, minRange, maxRange, timeLimitInSeconds, gameRules);

        var rulesDictionary = gameRules.ToDictionary(rule => rule.Number, rule => rule.Word);
        UserGame game = new UserGame(gameName, minRange, maxRange, timeLimitInSeconds, rulesDictionary);

        return game;
    }

    public bool ValidateAnswer(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
        {
            this.IncorrectAnswerCount++;
            return false;
        }
        
        var sortedDict = this.gameRules.OrderBy(pair => pair.Key)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        var correctAnswer = new StringBuilder();
        
        foreach (var pair in sortedDict.Where(pair => this.NumberToValidate % pair.Key == 0))
        {
            correctAnswer.Append(pair.Value);
        }

        // TODO - this is a lenient equals check "fooboo" is correct when "FooBoo" is the answer. Valid? 20250209 CL
        var isCorrect = correctAnswer.ToString().Equals(answer, StringComparison.OrdinalIgnoreCase);
        
        if (isCorrect) this.CorrectAnswerCount++;
        else this.IncorrectAnswerCount++;
        
        return isCorrect;
    }

    public bool IsGameExpired
    {
        get
        {
            if (this.GameStartedAt.HasValue == false)
            {
                this.Start();
            }
            var now = DateTime.UtcNow;
            var expiredAt = GameStartedAt!.Value.AddSeconds(this.TimeLimitInSeconds);
            return now >= expiredAt;
        }
    }

    public void Start()
    {
        this.GameStartedAt ??= DateTime.UtcNow;
    }

    public void SetNumber(int number)
    {
        if (number < this.NumberRangeMin || number > this.NumberRangeMax)
        {
            return;
        }
        this.NumberToValidate = number;
    }

    private static void ValidateGame(string gameName, int minRange, int maxRange, int timeLimitInSeconds,
        NumberToWord[] gameRules)
    {
        // TODO - don't throw exception here. Build a ValidationResult class and service that handles failed validation
        // that doesn't rely on throwing exceptions - 20250209 CL
        if (string.IsNullOrEmpty(gameName))
            throw new ArgumentNullException(nameof(gameName));
        ArgumentOutOfRangeException.ThrowIfNegative(minRange);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxRange, minRange);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeLimitInSeconds);
        ArgumentOutOfRangeException.ThrowIfEqual(minRange, maxRange);
        if (gameRules == null || gameRules.Length == 0)
            throw new ArgumentNullException(nameof(gameRules));
        foreach (var gameRule in gameRules)
        {
            if (gameRule.Number < minRange || gameRule.Number > maxRange)
                throw new ArgumentOutOfRangeException(nameof(gameRule));
            
            if (string.IsNullOrWhiteSpace(gameRule.Word))
                throw new ArgumentNullException(nameof(gameRule.Word));
        }
    }
}

public record NumberToWord(int Number, string Word);