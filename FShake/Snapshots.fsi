namespace FShake

open FsPickler

module Snapshots =

    [<Sealed>]
    type Snapshot<'T>

    val Map : ('A -> 'B) -> Snapshot<'A> -> Snapshot<'B>
    val Map2 : ('A -> 'B -> 'C) -> Snapshot<'A> -> Snapshot<'B> -> Snapshot<'C>
    val Pure : 'T -> Snapshot<'T>
    val Get : Snapshot<'T> -> 'T
    val Validate : (TargetBox -> option<obj>) -> Snapshot<'T> -> Async<bool>
    val BuildPickler : Pickler<'T> -> Pickler<Snapshot<'T>>

type Snapshot<'T> =
    Snapshots.Snapshot<'T>
