namespace FShake

module Recipes =

    type IContext =
        abstract Require<'T> : Target<'T> -> Async<Snapshot<'T>>

    type Recipe<'T> =
        {
            Build : IContext -> Async<Snapshot<'T>>
        }

    let defR f =
        { Build = f }

    let Bind (f: 'T1 -> Recipe<'T2>) (x: Recipe<'T1>) : Recipe<'T2> =
        defR <| fun c ->
            async {
                let! s1 = x.Build c
                let y = f (Snapshots.Get s1)
                let! s2 = y.Build c
                return Snapshots.Map2 (fun _ x -> x) s1 s2
            }

    let Map f x =
        defR <| fun c ->
            async {
                let! r = x.Build c
                return Snapshots.Map f r
            }

    let Map2 f x y =
        defR <| fun c ->
            async {
                let! a = x.Build c
                let! b = y.Build c
                return Snapshots.Map2 f a b
            }

    let Return x =
        defR (fun c -> async.Return(Snapshots.Pure x))

    type Builder =
        | Do

        member b.Bind(x, f) = Bind f x
        member b.Return(x) = Return x

    let Require t =
        defR (fun c -> c.Require(t))

    let Build c r =
        r.Build c

type Recipe<'T> = Recipes.Recipe<'T>
