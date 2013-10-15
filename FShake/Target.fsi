namespace FShake

open System
open FsPickler

/// Targets provide an abstract representation for a build goal,
/// such as a file. Technically, a `Target` value speializes `Targetable`
/// to a given key.
module Target =

    /// A first-class target value.
    [<Sealed>]
    type T<'T>

    /// Utility type used to unpack targets.
    type IConsumer<'R,'V> =

        /// Called immediately by `Unpack`.
        abstract Consume<'K> : Targetable<'K,'V> * 'K -> 'R

    /// Creates a new target value.
    val Create : Targetable<'K,'T> -> 'K -> T<'T>

    /// Extracts the key.
    val internal Key : T<'T> -> obj

    /// Extracts the equality.
    val internal KeyEquality : T<'T> -> HashIdentity.T<obj>

    /// The reified key type.
    val internal KeyType : T<'T> -> Type

    /// Extracts the pickler for keys.
    val internal KeyPickler : T<'T> -> Pickler<obj>

    /// Computes the reified type of the associated targetable.
    val internal TargetableType : T<'T> -> Type

    /// Computes the textual representation.
    val internal Text : T<'T> -> string

    /// Attempts to apply the default build recipe.
    val internal TryBuildDefault : T<'T> -> Async<option<'T>>

    /// Validates a value.
    val internal Validate : T<'T> -> 'T -> Async<bool>

    /// Gets the pickler for values.
    val ValuePickler : T<'T> -> Pickler<'T>

    /// The reified value type.
    val internal ValueType : T<'T> -> Type

    /// Unpacks a target value.
    val Unpack : T<'T> -> IConsumer<'R,'T> -> 'R

type Target<'T> = Target.T<'T>
