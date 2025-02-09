using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FooBooLooGame.Server.Data;
using FooBooLooGame.Server.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FooBooLooGame.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameStore _gameStore;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameStore gameStore, ILogger<GameController> logger)
    {
        _gameStore = gameStore;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Route("create-game")]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameDto gameDto, CancellationToken ct = default)
    {
        if ((gameDto?.NumbersToWords?.Any() ?? false) == false)
        {
            this._logger.LogError("Game numbers are required.");
            return BadRequest();
        }

        var entity = gameDto.NumbersToWords.Select(x => x.ToEntity()).ToArray();
        var game = UserGame.Create(gameDto.GameName, gameDto.MinNumber, gameDto.MaxNumber, gameDto.TimeLimit, entity);
        this._gameStore.AddGame(game);
        await Task.CompletedTask;
        var result = new ObjectResult(game.GameId) { StatusCode = (int)HttpStatusCode.Created };
        return result;
    }

    [HttpPost]
    [Route("start-game/{gameId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartGame([FromRoute] Guid gameId, CancellationToken ct = default)
    {
        var currentGame = this._gameStore.GetUserGame(gameId);
        if (currentGame == null)
        {
            this._logger.LogError("Game not found.");
            return NotFound();
        }

        currentGame.Start();

        return await Task.FromResult(Ok(currentGame.GameRules()));
    }

    [HttpGet]
    [Route("get-random-number/{gameId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRandomNumber([FromRoute] Guid gameId, CancellationToken ct = default)
    {
        var currentGame = this._gameStore.GetUserGame(gameId);

        if (currentGame == null)
        {
            this._logger.LogError("Game not found.");
            return NotFound();
        }

        var random = new Random(Guid.NewGuid().GetHashCode());
        var randomNumber = random.Next(currentGame.NumberRangeMin, currentGame.NumberRangeMax);
        currentGame.SetNumber(randomNumber);
        return await Task.FromResult(Ok(randomNumber));
    }

    [HttpGet]
    [Route("get-current-game/{gameId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentGame([FromRoute] Guid gameId, CancellationToken ct = default)
    {
        var currentGame = this._gameStore.GetUserGame(gameId);
        if (currentGame == null)
        {
            this._logger.LogError("Game not found.");
            return NotFound();
        }

        return await Task.FromResult(Ok(currentGame));
    }

    [HttpPost]
    [Route("submit-answer/{gameId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateAnswer([FromRoute] Guid gameId, string answer,
        CancellationToken ct = default)
    {
        var currentGame = this._gameStore.GetUserGame(gameId);
        if (currentGame == null)
        {
            this._logger.LogError("Game not found.");
            return NotFound();
        }

        if (currentGame.IsGameExpired)
        {
            return new ObjectResult("Time's up!")
            {
                StatusCode = (int)HttpStatusCode.UnprocessableEntity,
                ContentTypes = { "application/problem+json" }
            };
        }

        var result = currentGame.ValidateAnswer(answer);
        return await Task.FromResult(Ok(result));
    }

    [HttpGet]
    [Route("end-game")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EndGame([FromQuery] Guid gameId, CancellationToken ct = default)
    {
        var currentGame = this._gameStore.GetUserGame(gameId);
        if (currentGame == null)
        {
            this._logger.LogError("Game not found.");
            return NotFound();
        }

        var score = currentGame.CorrectAnswerCount;
        this._gameStore.DeleteGame(currentGame.GameId);
        return await Task.FromResult(Ok(score));
    }
}