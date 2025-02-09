using System;
using System.Collections.Concurrent;
using FooBooLooGame.Server.Data;

namespace FooBooLooGame.Server.Store;

public class InMemoryGameStore : IGameStore
{
    private readonly ConcurrentDictionary<Guid, UserGame> _userGames = new ConcurrentDictionary<Guid, UserGame>();
    
    public UserGame? GetUserGame(Guid gameId)
    {
        if (this._userGames.TryGetValue(gameId, out var game))
        {
            return game;
        }

        return null;
    }

    public void AddGame(UserGame game)
    {
        this._userGames.TryAdd(game.GameId, game);
    }

    public void DeleteGame(Guid gameId)
    {
        this._userGames.TryRemove(gameId, out var _);
    }
    
}