namespace FShake

module BuildStates =

    [<Sealed>]
    type BuildState

    val Transact : path: string -> (BuildState -> Async<'T>) -> Async<'T>

    val TryFind : TargetBox -> BuildState -> option<obj>

    val Update :
        state: BuildState ->
        target: Target<'T> ->
        action: (option<Snapshot<'T>> -> Async<Snapshot<'T>>) ->
        Async<Snapshot<'T>>

type BuildState = BuildStates.BuildState
