namespace FShake

module TargetBoxes =

    type IConsumer<'R> =
        abstract Consume<'T> : Target<'T> -> 'R

    type IPackedTarget =
        abstract Unpack<'T> : IConsumer<'T> -> 'T

    [<Sealed>]
    type T(p: IPackedTarget) =

        let (tt, kt, vt, key, eq, text, kp) =
            p.Unpack {
                new IConsumer<_> with
                    member __.Consume(t) =
                        (
                            Target.TargetableType t,
                            Target.KeyType t,
                            Target.ValueType t,
                            Target.Key t,
                            Target.KeyEquality t,
                            Target.Text t,
                            Target.KeyPickler t
                        )
            }

        let h = eq.GetHashCode(key)

        override t.GetHashCode() = h

        override t.Equals(o) =
            match o with
            | :? T as o ->
                tt = t.TargetableType
                && eq.Equals(key, o.Key)
            | _ -> false

        override t.ToString() =
            text

        member t.Unpack(c) =
            p.Unpack(c)

        member t.Key = key
        member t.KeyPickler = kp
        member t.KeyType = kt
        member t.TargetableType = tt
        member t.Text = text
        member t.ValueType = vt

    let Create (t: Target<'T>) =
        T({ new IPackedTarget with
                member __.Unpack(c) = c.Consume(t) })

    let Key (t: T) = t.Key
    let KeyPickler (t: T) = t.KeyPickler
    let KeyType (t: T) = t.KeyType
    let TargetableType (t: T) = t.TargetableType
    let Unpack (t: T) c = t.Unpack(c)
    let ValueType (t: T) = t.ValueType

    let Validate (t: T) (v: obj) =
        Unpack t {
            new IConsumer<_> with
                member __.Consume(t: Target<'T>) =
                    match v with
                    | :? 'T as v -> Target.Validate t v
                    | _ -> async.Return false
        }

    let TryUnpack (t: Targetable<'K,'V>) (tb: T) =
        if t.Type = TargetableType tb
            then Some (Key tb :?> 'K)
            else None

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
        interface ITargetable<string,string> with
            member __.DefaultRecipe(key) = async.Return(None)
            member __.KeyEquality = HashIdentity.Structural<string>
            member __.KeyPickler = Pickler.String
            member __.ValuePickler = Pickler.String
            member __.ShowKey(x) = ""
            member __.Validate(x, y) = async.Return(true)

    let runnerType = typedefof<Runner<string,string,Dummy>>

    let forTypes (x: ITypeIndexed<_>) a b t =
        let r =
            runnerType.MakeGenericType(a, b, t)
            |> Activator.CreateInstance :?> IRunner
        r.Run(x)

    type ReadTarget =
        | ReadTarget

        interface ITypeIndexed<Pickler.Reader -> T> with
            member x.Create<'A,'B,'T when 'T : (new : unit -> 'T)
                                      and 'T :> ITargetable<'A,'B>>() =
                let p = Singleton.Instance<'T>.KeyPickler
                let tt = Targetable.Create Singleton.Is<'T>
                fun r ->
                    let key = r.Read(p)
                    Target.Create tt key
                    |> Create

    let readTarget (r: Pickler.Reader) =
        let ( ! ) (r: Pickler.Reader) = r.Read(Pickler.Type)
        forTypes ReadTarget !r !r !r r

    let writeTarget (w: Pickler.Writer) t =
        w.Write(Pickler.Type, KeyType t)
        w.Write(Pickler.Type, ValueType t)
        w.Write(Pickler.Type, TargetableType t)
        w.Write(KeyPickler t, Key t)

    let Pickler =
        Pickler.FromPrimitives readTarget writeTarget

type TargetBox = TargetBoxes.T
