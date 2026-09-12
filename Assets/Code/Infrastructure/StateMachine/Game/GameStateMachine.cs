using Code.Infrastructure.StateMachine.Game.States;
using VContainer.Unity;

namespace Code.Infrastructure.StateMachine.Game
{
    public class GameStateMachine : StateMachine<IGameState>, ITickable
    {
        public GameStateMachine(GameStateFactory gameStateFactory) : base(gameStateFactory)
        {
        }

        public void Tick()
        {
            Update();
        }
    }
}