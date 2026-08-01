# CupkekGames BehaviourTrees

ScriptableObject-driven behaviour tree framework. Trees are authored as assets and run by `BehaviourTreeRunner`; the package ships a custom graph editor view.

## What's inside

**Runtime** (`CupkekGames.BehaviourTrees.asmdef`)

- `BehaviourTree` — `ScriptableObject` asset that owns a tree of `BTNode`s
- `BehaviourTreeRunner` / `BehaviourTreeRunnerMono` — drive a tree per-tick
- `BehaviourTreeManager` — registry for runners
- `BTNode` + `BTNodeRuntimeState` — node base + per-instance runtime state
- Bases: `BTNodeAction`, `BTNodeComposite`, `BTNodeDecorator`, `BTNodeRoot`
- Built-in nodes: `DebugNode`, `DelayNode`, `ParallelNode`, `RepeatNode`, `SequencerNode`, `PersistentContextNodeCondition`, `PersistentContextNodeToggle`

**Editor** (`CupkekGames.BehaviourTrees.Editor.asmdef`)

- `BehaviourTreeEditor` / `BehaviourTreeRunnerMonoEditor` — inspector tooling
- `BehaviourTreeView` / `BTNodeView` — graph view for authoring trees

## Asmdef + namespace

`CupkekGames.BehaviourTrees` (runtime), `CupkekGames.BehaviourTrees.Editor` (editor). Pluralized from the original `CupkekGames.BehaviourTree` to avoid the namespace = class collision (`class BehaviourTree`).

## Upgrading from 0.1.x

0.2.0 removed the pre-graph child fields (`BTNodeComposite.Children`, `BTNodeDecorator.Child`) and the `Migrate Legacy Trees` tool. Tree assets authored on 0.1.x must be converted while still on 0.1.4: run `CupkekGames/BehaviourTrees/Migrate Legacy Trees`, save, then upgrade.

## Dependencies

- `com.cupkekgames.singletons` (UPM) — `BehaviourTreeManager` extends `Singleton<T>`

## Repository

[Cupkek-Games/CupkekGames-BehaviourTrees](https://github.com/Cupkek-Games/CupkekGames-BehaviourTrees)
