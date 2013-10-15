namespace FShake

open System
open System.IO
open FsPickler
open FsPickler.Combinators

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
