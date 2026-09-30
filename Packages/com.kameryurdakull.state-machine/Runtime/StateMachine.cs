using System;
using System.Threading;

namespace BlackProcess.StateMachine
{
    /// <summary>Runs one state at a time. The owner drives Tick and FixedTick and disposes the machine.</summary>
    public sealed class StateMachine : IDisposable
    {
        private IState _currentState;
        private CancellationTokenSource _stateCancellation;
        private bool _isTransitioning;
        private bool _isDisposed;

        public IState CurrentState => _currentState;

        public bool ChangeState(IState nextState)
        {
            if (nextState == null) throw new ArgumentNullException(nameof(nextState));
            EnsureAvailable();

            if (ReferenceEquals(_currentState, nextState)) return false;

            _isTransitioning = true;
            try
            {
                ExitCurrentState();
                _currentState = nextState;
                _stateCancellation = new CancellationTokenSource();

                try
                {
                    nextState.Enter(_stateCancellation.Token);
                }
                catch
                {
                    _stateCancellation.Cancel();
                    _stateCancellation.Dispose();
                    _stateCancellation = null;
                    _currentState = null;
                    throw;
                }

                return true;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public void Tick()
        {
            EnsureAvailable();
            _currentState?.Tick();
        }

        public void FixedTick()
        {
            EnsureAvailable();
            _currentState?.FixedTick();
        }

        public void Stop()
        {
            EnsureAvailable();
            _isTransitioning = true;
            try
            {
                ExitCurrentState();
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            EnsureAvailable();
            try
            {
                Stop();
            }
            finally
            {
                _isDisposed = true;
            }
        }

        private void ExitCurrentState()
        {
            var previousState = _currentState;
            var cancellation = _stateCancellation;
            _currentState = null;
            _stateCancellation = null;

            if (cancellation == null) return;

            try
            {
                cancellation.Cancel();
            }
            finally
            {
                try
                {
                    previousState.Exit();
                }
                finally
                {
                    cancellation.Dispose();
                }
            }
        }

        private void EnsureAvailable()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(StateMachine));
            if (_isTransitioning) throw new InvalidOperationException("A state transition is already in progress.");
        }
    }
}
