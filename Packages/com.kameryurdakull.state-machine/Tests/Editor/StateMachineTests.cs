using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using StateMachineCore = BlackProcess.StateMachine.StateMachine;

namespace BlackProcess.StateMachine.Tests
{
    public sealed class StateMachineTests
    {
        [Test]
        public void ChangeState_CancelsAndExitsOldStateBeforeEnteringNewState()
        {
            var calls = new List<string>();
            var first = new RecordingState("first", calls);
            var second = new RecordingState("second", calls);
            using (var machine = new StateMachineCore())
            {
                machine.ChangeState(first);
                machine.ChangeState(second);

                Assert.That(first.Token.IsCancellationRequested, Is.True);
                Assert.That(machine.CurrentState, Is.SameAs(second));
                Assert.That(calls, Is.EqualTo(new[] { "first enter", "first exit", "second enter" }));
            }
        }

        [Test]
        public void TickAndFixedTick_OnlyReachActiveState()
        {
            var state = new RecordingState("active", new List<string>());
            using (var machine = new StateMachineCore())
            {
                machine.ChangeState(state);
                machine.Tick();
                machine.FixedTick();
                machine.Stop();
                machine.Tick();
                machine.FixedTick();
            }

            Assert.That(state.TickCount, Is.EqualTo(1));
            Assert.That(state.FixedTickCount, Is.EqualTo(1));
        }

        [Test]
        public void ChangeState_RejectsNullAndKeepsSameState()
        {
            var state = new RecordingState("active", new List<string>());
            using (var machine = new StateMachineCore())
            {
                Assert.Throws<ArgumentNullException>(() => machine.ChangeState(null));
                Assert.That(machine.ChangeState(state), Is.True);
                Assert.That(machine.ChangeState(state), Is.False);
                Assert.That(state.EnterCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void Dispose_CancelsCurrentStateAndRejectsFurtherUse()
        {
            var state = new RecordingState("active", new List<string>());
            var machine = new StateMachineCore();
            machine.ChangeState(state);
            machine.Dispose();

            Assert.That(state.Token.IsCancellationRequested, Is.True);
            Assert.Throws<ObjectDisposedException>(() => machine.Tick());
        }

        private sealed class RecordingState : BaseState
        {
            private readonly string _name;
            private readonly List<string> _calls;

            public RecordingState(string name, List<string> calls)
            {
                _name = name;
                _calls = calls;
            }

            public CancellationToken Token { get; private set; }
            public int EnterCount { get; private set; }
            public int TickCount { get; private set; }
            public int FixedTickCount { get; private set; }

            public override void Enter(CancellationToken cancellationToken)
            {
                Token = cancellationToken;
                EnterCount++;
                _calls.Add($"{_name} enter");
            }

            public override void Tick() => TickCount++;
            public override void FixedTick() => FixedTickCount++;
            public override void Exit() => _calls.Add($"{_name} exit");
        }
    }
}
