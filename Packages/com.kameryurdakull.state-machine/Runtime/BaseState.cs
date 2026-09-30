using System.Threading;

namespace BlackProcess.StateMachine
{
    /// <summary>Override only the lifecycle methods needed by a state.</summary>
    public abstract class BaseState : IState
    {
        public virtual void Enter(CancellationToken cancellationToken) { }
        public virtual void Tick() { }
        public virtual void FixedTick() { }
        public virtual void Exit() { }
    }
}
