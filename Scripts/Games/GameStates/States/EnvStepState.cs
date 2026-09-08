using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.GameStates;

public record EnvStepState(Vector2I Coords, int Priority, ApplyStep Step);
