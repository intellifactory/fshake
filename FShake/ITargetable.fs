namespace FShake

open System
open System.Collections.Generic
open FsPickler

/// Provides operations on a `'Key` and `'Value` pair of types.
type ITargetable<'Key,'Value> =

    /// Default build recipe for a given key.
    abstract DefaultRecipe : 'Key -> Async<option<'Value>>

    /// Gets a short readable representation for a key.
    abstract ShowKey : 'Key -> string

    /// Validates a key-value pair. For example, for file targetables,
    /// check that the file exists on disk and has not been changed since
    /// the time the key-value pair was computed last.
    abstract Validate : 'Key * 'Value -> Async<bool>

    /// Equality on keys.
    abstract KeyEquality : IEqualityComparer<'Key>

    /// Pickler for keys.
    abstract KeyPickler : Pickler<'Key>

    /// Pickler for values.
    abstract ValuePickler : Pickler<'Value>
