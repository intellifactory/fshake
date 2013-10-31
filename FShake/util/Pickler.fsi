namespace FShake

/// Provides picklers for some FShake types, notably target boxes.
module Pickler =

    [<Sealed>]
    type T<'T>

    [<Sealed>]
    type Reader =
        member Read : T<'T> -> 'T

    [<Sealed>]
    type Writer =
        member Write : T<'T> * 'T -> unit

    val FromPrimitives : (Reader -> 'T) -> (Writer -> 'T -> unit) -> T<'T>

//    val TargetBox : Pickler<TargetBox>
//    val Type : Pickler<Type>

    val String : T<string>
    val DateTime : T<DateTime>
    val Wrap : ('T1 -> 'T2) -> ('T2 -> 'T1) -> T<'T1> -> T<'T2>
    val Unpickle : T<'T> -> byte [] -> 'T
    val Pickle : T<'T> -> 'T -> byte []

    val ReadFromStream : T<'T> -> Stream -> 'T
    val WriteToStream : T<'T> -> Stream -> 'T -> unit

    val List : T<'T> -> T<list<'T>>
    val Choice2 : T<'T1> -> T<'T2> -> T<Choice<'T1,'T2>>
    val Pair : T<'T1> -> T<'T2> -> T<'T1 * 'T2>
    val PairSeq : T<'T1> -> T<'T2> -> T<seq<'T1 * 'T2>>
    val Unit : T<unit>
    val Bytes : T<byte[]>
    val Type : T<Type>

type Pickler<'T> = Pickler.T<'T>
