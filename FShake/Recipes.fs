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

    let Delay f =
        defR <| fun c -> f().Build(c)

    let ( >. ) a b =
        Snapshots.Map2 (fun _ x -> x) a b

    let ForEach (xs: seq<'T>) (f: 'T -> Recipe<unit>) =
        defR <| fun c ->
            async {
                let result = ref Snapshots.Unit
                for x in xs do
                    let! s = f(x).Build(c)
                    do result := !result >. s
                return !result
            }

    let Combine a b =
        defR <| fun c ->
            async {
                let! s1 = a.Build c
                let! s2 = b.Build c
                return Snapshots.Map2 (fun _ x -> x) s1 s2
            }

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

        member b.Combine(x, y) = Combine x y
        member b.Delay(f) = Delay f
        member b.For(xs, f) = ForEach xs f
        member b.Bind(x, f) = Bind f x
        member b.Return(x) = Return x
        member b.ReturnFrom(r: Recipe<'T>) = r

    let Require t =
        defR <| fun c ->
            async {
                let! snap = c.Require(t)
                return
                    snap
                    |> Snapshots.WithTargetBox
                        (TargetBoxes.Create t)
            }

    let Build c r =
        r.Build c

    let Ignore r =
        Do {
            let! x = r
            return ()
        }

type Recipe<'T> = Recipes.Recipe<'T>
