# State Machine

A small state machine for Unity with explicit `Tick` / `FixedTick` calls and a cancellation token for each active state. The runtime assembly has no external dependencies.

```csharp
var machine = new BlackProcess.StateMachine.StateMachine();
machine.ChangeState(new MyState());
machine.Tick();       // Unity Update
machine.FixedTick();  // Unity FixedUpdate
machine.Dispose();    // Unity OnDestroy
```

Implement `IState` or derive from `BaseState`. Pass the token received by `Enter` to asynchronous work and unsubscribe from events in `Exit`. Import **Round Flow** from the package's Samples section for an event-driven example.

Full Turkish documentation, installation instructions, and UniTask/VContainer guidance: [GitHub README](https://github.com/kameryurdakull/state-machine#readme).
