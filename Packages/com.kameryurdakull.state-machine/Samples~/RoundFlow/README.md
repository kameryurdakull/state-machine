# Round Flow sample

Import the sample from Package Manager, add `RoundFlow` to a GameObject, and call its four public methods from UI buttons or gameplay code. The sequence is `HeroSelection → TrinketSelection → Map → Combat → Map`.

`RoundSignals` stands in for a typed event bus. Each state subscribes in `Enter` and unsubscribes in `Exit`, so inactive states do not react to events. The `RoundFlow` MonoBehaviour owns and disposes the machine and calls `Tick` and `FixedTick` from the correct Unity callbacks.
