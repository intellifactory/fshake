namespace FShake

open System
open FsPickler
open FsPickler.Combinators
module TB = TargetBoxes

module Pickler =

    let Type =
        Pickler.string
        |> Pickler.wrap
            (fun s -> Type.GetType(s, throwOnError = true))
            (fun t -> t.AssemblyQualifiedName)

    type ITypeIndexed<'R> =
        abstract Create<'A,'B,'T when 'T : (new : unit -> 'T)
                                 and 'T :> ITargetable<'A,'B>> : unit -> 'R

    type IRunner =
        abstract Run<'R> : alg: ITypeIndexed<'R> -> 'R

    [<Struct>]
    type Runner<'A,'B,'T when 'T : (new : unit -> 'T)
                         and 'T :> ITargetable<'A,'B>> =
        interface IRunner with
            member r.Run(alg) = alg.Create<'A,'B,'T>()

    [<Sealed>]
    type Dummy() =
        interface ITargetable<int,int> with
            member __.DefaultRecipe(key) = async.Return(None)
            member __.KeyEquality = HashIdentity.Structural<int>
            member __.KeyPickler = Pickler.int
            member __.ValuePickler = Pickler.int
            member __.ShowKey(x) = ""
            member __.Validate(x, y) = async.Return(true)

    let runnerType = typedefof<Runner<int,int,Dummy>>

    let forTypes (x: ITypeIndexed<_>) a b t =
        let r =
            runnerType.MakeGenericType(a, b, t)
            |> Activator.CreateInstance :?> IRunner
        r.Run(x)

    type ReadTarget =
        | ReadTarget

        interface ITypeIndexed<Reader -> TargetBox> with
            member x.Create<'A,'B,'T when 'T : (new : unit -> 'T)
                                      and 'T :> ITargetable<'A,'B>>() =
                let p = Singleton.Instance<'T>.KeyPickler
                let tt = Targetable.Create Singleton.Is<'T>
                fun r ->
                    let key = r.Read(p)
                    Target.Create tt key
                    |> TargetBoxes.Create

    let readTarget (r: Reader) =
        let ( ! ) (r: Reader) = r.Read(Type)
        forTypes ReadTarget !r !r !r r

    let writeTarget (w: Writer) t =
        w.Write(Type, TB.KeyType t)
        w.Write(Type, TB.ValueType t)
        w.Write(Type, TB.TargetableType t)
        w.Write(TB.KeyPickler t, TB.Key t)

    let TargetBox =
        Pickler.FromPrimitives(readTarget, writeTarget)
