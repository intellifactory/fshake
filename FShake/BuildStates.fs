namespace FShake

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.IO
open FsPickler
open FsPickler.Combinators

module BuildStates =

    type TargetState =
        | Built of Snapshot<byte[]>
        | Building
        | Unknown

    type TargetStateBox =
        {
            LockRoot : obj
            mutable State : TargetState
        }

    type BuildState =
        {
            TargetStates : ConcurrentDictionary<TargetBox,TargetStateBox>
        }

    let targetStatePickler =
        Pickler.sum (fun x k1 k2 k3 ->
            match x with
            | Built x -> k1 x
            | Building -> k2 ()
            | Unknown -> k3 ())
        ^+ Pickler.case Built (Snapshots.BuildPickler Pickler.bytes)
        ^+ Pickler.variant Building
        ^. Pickler.variant Unknown

    let targetStateBoxPickler =
        targetStatePickler
        |> Pickler.wrap
            (fun st -> { LockRoot = obj (); State = st })
            (fun box -> box.State)

    let fromPair (KeyValue (k, v)) = (k, v)
    let toPair (k, v) = KeyValuePair(k, v)

    let buildStatePickler =
        Pickler.pairSeq Pickler.TargetBox targetStateBoxPickler
        |> Pickler.wrap
            (fun xs ->
                let xs =
                    xs
                    |> Seq.filter (fun (k, v) ->
                        match v.State with
                        | Unknown -> false
                        | _ -> true)
                    |> Seq.map toPair
                { TargetStates = ConcurrentDictionary(xs) })
            (fun st ->
                Seq.toArray st.TargetStates
                |> Seq.map fromPair)

    let freshBuildState () =
        {
            TargetStates = ConcurrentDictionary()
        }

    let openRW path =
        let file = FileInfo(path)
        let d = file.Directory
        if not d.Exists then
            d.Create()
        File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)

    let Transact path action =
        async {
            let fsp = FsPickler()
            use s = openRW path
            let st =
                match s.Length with
                | 0L -> freshBuildState ()
                | _ ->
                    let r = fsp.Deserialize(buildStatePickler, s)
                    let _ = s.Seek(0L, SeekOrigin.Begin)
                    r
            let! res = action st
            do fsp.Serialize(buildStatePickler, s, st)
            return res
        }

    let freshStateBox () =
        {
            LockRoot = obj ()
            State = Unknown
        }

    let getStateBox state t =
        state.TargetStates.GetOrAdd(t, fun _ -> freshStateBox ())

    exception RecursiveDependency

    let startBuilding p sb =
        lock sb.LockRoot <| fun () ->
            match sb.State with
            | TargetState.Building ->
                raise RecursiveDependency
            | TargetState.Built snap ->
                sb.State <- Building
                snap
                |> Snapshots.Map (unpickle p)
                |> Some
            | TargetState.Unknown ->
                None

    let doneBuilding p snap sb =
        let snap =
            snap
            |> Snapshots.Map (pickle p)
        lock sb.LockRoot <| fun () ->
            sb.State <- Built snap

    let Update state (t: Target<'T>) act =
        Target.Unpack t {
            new Target.IConsumer<_,_> with
                member x.Consume(tt, key) =
                    async {
                        let bt = TargetBoxes.Create t
                        let sb = getStateBox state bt
                        let snap = startBuilding tt.Instance.ValuePickler sb
                        let! result = act snap
                        do doneBuilding tt.Instance.ValuePickler result sb
                        return result
                    }
        }

    let TryFind t state =
        let sb = getStateBox state t
        lock sb.LockRoot <| fun () ->
            match sb.State with
            | TargetState.Built snap ->
                let data = Snapshots.Get snap
                TargetBoxes.Unpack t {
                    new TargetBoxes.IConsumer<_> with
                        member __.Consume(t) =
                            let p = Target.ValuePickler t
                            Some (box (unpickle p data))
                }
            | _ -> None

type BuildState = BuildStates.BuildState
