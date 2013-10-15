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

type TargetBox = TargetBoxes.T
