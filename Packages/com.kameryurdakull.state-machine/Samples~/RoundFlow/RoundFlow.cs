using System;
using System.Threading;
using Kamer.StateMachine;
using UnityEngine;
using StateMachineCore = Kamer.StateMachine.StateMachine;

namespace Kamer.StateMachine.Samples.RoundFlow
{
    public enum RoundPhase
    {
        HeroSelection,
        TrinketSelection,
        Map,
        Combat
    }

    /// <summary>Replace these events with your project's event bus or UI callbacks.</summary>
    public sealed class RoundSignals
    {
        public event Action HeroSelected;
        public event Action TrinketSelected;
        public event Action RoomEntered;
        public event Action CombatFinished;

        public void SelectHero() => HeroSelected?.Invoke();
        public void SelectTrinket() => TrinketSelected?.Invoke();
        public void EnterRoom() => RoomEntered?.Invoke();
        public void FinishCombat() => CombatFinished?.Invoke();
    }

    /// <summary>Minimal round flow. Wire the four public methods to UI buttons to try it.</summary>
    public sealed class RoundFlow : MonoBehaviour
    {
        private StateMachineCore _machine;
        private RoundSignals _signals;

        private void Awake()
        {
            _signals = new RoundSignals();
            _machine = new StateMachineCore();
        }

        private void Start()
        {
            _machine.ChangeState(new RoundState(RoundPhase.HeroSelection, _signals, OnHeroSelected));
        }

        private void Update() => _machine.Tick();
        private void FixedUpdate() => _machine.FixedTick();
        private void OnDestroy() => _machine?.Dispose();

        public void SelectHero() => _signals.SelectHero();
        public void SelectTrinket() => _signals.SelectTrinket();
        public void EnterRoom() => _signals.EnterRoom();
        public void FinishCombat() => _signals.FinishCombat();

        private void OnHeroSelected() => _machine.ChangeState(
            new RoundState(RoundPhase.TrinketSelection, _signals, OnTrinketSelected));

        private void OnTrinketSelected() => _machine.ChangeState(
            new RoundState(RoundPhase.Map, _signals, OnRoomEntered));

        private void OnRoomEntered() => _machine.ChangeState(
            new RoundState(RoundPhase.Combat, _signals, OnCombatFinished));

        private void OnCombatFinished() => _machine.ChangeState(
            new RoundState(RoundPhase.Map, _signals, OnRoomEntered));
    }

    internal sealed class RoundState : BaseState
    {
        private readonly RoundPhase _phase;
        private readonly RoundSignals _signals;
        private readonly Action _advance;

        public RoundState(RoundPhase phase, RoundSignals signals, Action advance)
        {
            _phase = phase;
            _signals = signals;
            _advance = advance;
        }

        public override void Enter(CancellationToken cancellationToken)
        {
            switch (_phase)
            {
                case RoundPhase.HeroSelection: _signals.HeroSelected += _advance; break;
                case RoundPhase.TrinketSelection: _signals.TrinketSelected += _advance; break;
                case RoundPhase.Map: _signals.RoomEntered += _advance; break;
                case RoundPhase.Combat: _signals.CombatFinished += _advance; break;
                default: throw new ArgumentOutOfRangeException();
            }

            Debug.Log($"Round phase: {_phase}");
        }

        public override void Exit()
        {
            switch (_phase)
            {
                case RoundPhase.HeroSelection: _signals.HeroSelected -= _advance; break;
                case RoundPhase.TrinketSelection: _signals.TrinketSelected -= _advance; break;
                case RoundPhase.Map: _signals.RoomEntered -= _advance; break;
                case RoundPhase.Combat: _signals.CombatFinished -= _advance; break;
            }
        }
    }
}
