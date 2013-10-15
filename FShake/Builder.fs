namespace FShake

module Builder =
    open System
    open System.IO

    [<Sealed>]
    exception NoMatchingRule of string with

        override x.Message =
            match x :> exn with
            | NoMatchingRule t -> "No matching rule to build target " + t
            | _ -> "impossible"

    type Config =
        {
            RootDirectory : string
        }

    let getBuildStatePath config =
        Path.Combine(config.RootDirectory, "build", "state.bin")

    let transact cfg action =
        BuildStates.Transact (getBuildStatePath cfg) action

    [<Sealed>]
    type Builder(rules, state) =

        member b.BuildRecipe(t) =
            async {
                match Rules.TryFind t rules with
                | None ->
                    let! r = Target.TryBuildDefault t
                    return
                        match r with
                        | None -> raise (NoMatchingRule (Target.Text t))
                        | Some r -> Snapshots.Pure r
                | Some recipe -> return! Recipes.Build b recipe
            }

        member b.Build(t: Target<_>) =
            printfn "Building %s" (Target.Text t)
            function
            | None -> b.BuildRecipe(t)
            | Some snap ->
                printfn "FOUND SNAPSHOT"
                let lookup x = BuildStates.TryFind x state
                async {
                    let! ok = Snapshots.Validate lookup snap
                    do printfn "SNAPSHOT VALIDATED: %b" ok
                    if ok then return snap else
                        return! b.BuildRecipe(t)
                }
            |> BuildStates.Update state t

        interface Recipes.IContext with
            member b.Require(t) = b.Build(t)

    let BuildTarget cfg rules t =
        transact cfg <| fun state ->
            async {
                let builder = Builder(rules, state)
                let! snap = builder.Build(t)
                return Snapshots.Get snap
            }

    let CleanTarget (rules: Rules) (t: Target<'T>) : Async<unit> =
        failwith "TODO"
