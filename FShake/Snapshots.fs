namespace FShake

module Snapshots =

    type Snapshot<'T> =
        {
            Targets : list<TargetBox>
            Value : 'T
        }

        override __.ToString() =
            string (box __.Value) +
            "(" + String.concat "; " (Seq.map string __.Targets) + ")"

    let mk ts v =
        {
            Targets = ts
            Value = v
        }

    let WithTargetBox tb snap =
        { snap with Targets = tb :: snap.Targets }

    let Map f sn =
        mk sn.Targets (f sn.Value)

    let Map2 f s1 s2 =
        mk (s1.Targets @ s2.Targets) (f s1.Value s2.Value)

    let Pure x =
        mk [] x

    let Get x =
        x.Value

    let validateTargetBox rawLookup t =
        match rawLookup t with
        | None -> async.Return(false)
        | Some (v: obj) -> TargetBoxes.Validate t v

    let Validate rawLookup snap =
        async {
            let rec loop (ts: list<TargetBox>) =
                match ts with
                | [] ->
                    do printfn "Validate: NO TARGETS"
                    async.Return(true)
                | t :: ts ->
                    async {
                        do printfn "Validate/X.."
                        let! ok = validateTargetBox rawLookup t
                        if ok then return! loop ts else return false
                    }
            return! loop snap.Targets
        }

    let targetsPickler =
        Pickler.List TargetBoxes.Pickler

    let BuildPickler p =
        Pickler.Product mk
        ^+ Pickler.Field (fun x -> x.Targets) targetsPickler
        ^. Pickler.Field (fun x -> x.Value) p

    let Unit = Pure ()

type Snapshot<'T> =
    Snapshots.Snapshot<'T>
