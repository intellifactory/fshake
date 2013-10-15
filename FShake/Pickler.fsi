namespace FShake

open System
open FsPickler

/// Provides picklers for some FShake types, notably target boxes.
module Pickler =
    val TargetBox : Pickler<TargetBox>
    val Type : Pickler<Type>
