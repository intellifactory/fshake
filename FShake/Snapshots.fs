namespace FShake

open FsPickler
open FsPickler.Combinators

module Snapshots =

    type Snapshot<'T> =
        {
            Targets : list<TargetBox>
            Value : 'T
        }

    let mk ts v =
        {
            Targets = ts
            Value = v
        }

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
                | [] -> async.Return(true)
                | t :: ts ->
                    async {
                        let! ok = validateTargetBox rawLookup t
                        if ok then return! loop ts else return false
                    }
            return! loop snap.Targets
        }

    module P = Pickler

    let targetsPickler =
        P.list Pickler.TargetBox

    let BuildPickler p =
        P.product mk
        ^+ P.field (fun x -> x.Targets) targetsPickler
        ^. P.field (fun x -> x.Value) p

type Snapshot<'T> =
    Snapshots.Snapshot<'T>
