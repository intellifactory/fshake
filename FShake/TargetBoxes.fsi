namespace FShake

module TargetBoxes =

    type IConsumer<'R> =
        abstract Consume<'T> : Target<'T> -> 'R

    [<Sealed>]
    type T

    val Create : Target<'T> -> T
    val Key : T -> obj
    val KeyPickler : T -> Pickler<obj>
    val KeyType : T -> Type
    val TargetableType : T -> Type
    val TryUnpack : Targetable<'K,'V> -> T -> option<'K>
    val Unpack : T -> IConsumer<'R> -> 'R
    val Validate : T -> obj -> Async<bool>
    val ValueType : T -> Type
    val Pickler : Pickler<T>

type TargetBox = TargetBoxes.T
