using System;

namespace FooBooLooGame.Server.Data;

public interface IGameStore
{
    public UserGame? GetUserGame(Guid gameId);
    void AddGame(UserGame game);
    void DeleteGame(Guid gameId);
}
