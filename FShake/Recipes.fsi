namespace FShake

module Recipes =

    type IContext =
        abstract Require<'T> : Target<'T> -> Async<Snapshot<'T>>

    [<Sealed>]
    type Recipe<'T>

    val Bind : ('T1 -> Recipe<'T2>) -> Recipe<'T1> -> Recipe<'T2>
    val Map : ('T1 -> 'T2) -> Recipe<'T1> -> Recipe<'T2>
    val Map2 : ('T1 -> 'T2 -> 'T3) -> Recipe<'T1> -> Recipe<'T2> -> Recipe<'T3>
    val Return : 'T -> Recipe<'T>
    val Ignore : Recipe<'T> -> Recipe<unit>

    type Builder =
        | Do

        member Delay : (unit -> Recipe<'T>) -> Recipe<'T>
        member Combine : Recipe<'T1> * Recipe<'T2> -> Recipe<'T2>
        member For : seq<'T> * ('T -> Recipe<unit>) -> Recipe<unit>
        member Bind : Recipe<'T1> * ('T1 -> Recipe<'T2>) -> Recipe<'T2>
        member Return : 'T -> Recipe<'T>
        member ReturnFrom : Recipe<'T> -> Recipe<'T>

    val Require : Target<'T> -> Recipe<'T>

    val Build : IContext -> Recipe<'T> -> Async<Snapshot<'T>>

type Recipe<'T> = Recipes.Recipe<'T>
