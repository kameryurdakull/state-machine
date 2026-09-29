using System.Threading;

namespace Kamer.StateMachine
{
    /// <summary>A state owned by one state machine at a time.</summary>
    public interface IState
    {
        void Enter(CancellationToken cancellationToken);
        void Tick();
        void FixedTick();
        void Exit();
    }
}
