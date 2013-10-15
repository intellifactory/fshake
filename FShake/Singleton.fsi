namespace FShake

open System

/// Represents evidence that a given type has a unit constructor
/// and a unique singleton instance that can be shared by the program.
[<Sealed>]
type Singleton<'T when 'T : (new : unit -> 'T)> =

    /// The singleton instance for the type.
    member Instance : 'T

    /// The reified `'T` type.
    member Type : Type

/// Utilities for working with singleton types.
module Singleton =

    /// Gets the singleton instance for the type.
    val Instance<'T when 'T : (new : unit -> 'T)> : 'T

    /// Constructs evidence that a type is singleton.
    val Is<'T when 'T : (new : unit -> 'T)> : Singleton<'T>

