namespace FShake

module Target =

    type IConsumer<'R,'V> =
        abstract Consume<'K> : Targetable<'K,'V> * 'K -> 'R

    type IPackedTarget<'T> =
        abstract Unpack<'R> : IConsumer<'R,'T> -> 'R

    type T<'T> =
        | T of IPackedTarget<'T>

    let Unpack (T p) c =
        p.Unpack(c)

    let Create (t: Targetable<'K,'T>) (key: 'K) =
        T { new IPackedTarget<'T> with
                member __.Unpack(c) = c.Consume(t, key) }

    let TryBuildDefault t =
        Unpack t {
            new IConsumer<_,_> with
                member __.Consume(tt, key) =
                    tt.Instance.DefaultRecipe(key)
        }

    let TargetableType t =
        Unpack t {
            new IConsumer<_,_> with
                member __.Consume(tt, key) = tt.Type
        }

    let KeyType t =
        Unpack t {
            new IConsumer<_,_> with
                member __.Consume(tt: Targetable<'K,'V>, key) = typeof<'K>
        }

    let ValueType t =
        Unpack t {
            new IConsumer<_,_> with
                member __.Consume(tt: Targetable<'K,'V>, key) = typeof<'V>
        }

    let Key t =
        Unpack t {
            new IConsumer<_,_> with
                member __.Consume(tt, key) = box key
        }

    let KeyEquality t =
        Unpack t {
            new IConsumer<_,_> with
                member __.Consume(tt, key) =
                    tt.Instance.KeyEquality
                    |> HashIdentity.Box
        }

    let KeyPickler t =
        Unpack t {
            new IConsumer<_,_> with
                member __.Consume(tt, key) =
                    tt.Instance.KeyPickler
                    |> Pickler.Wrap box unbox
        }

    let Text t =
        Unpack t {
            new IConsumer<_,_> with
                member __.Consume(tt, key) =
                    tt.Instance.ShowKey(key)
        }

    let Validate t value =
        Unpack t {
            new IConsumer<_,_> with
                member __.Consume(tt: Targetable<_,'V>, key) =
                    tt.Instance.Validate(key, value)
        }

    let ValuePickler t =
        Unpack t {
            new IConsumer<_,_> with
                member __.Consume(tt: Targetable<_,'V>, key) =
                    tt.Instance.ValuePickler
        }

type Target<'T> = Target.T<'T>
