namespace FShake

type Rules =
    {
        Lookup : TargetBox -> option<Recipe<obj>>
    }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Rules =
    open System

    let Append (a: Rules) (b: Rules) : Rules =
        {
            Lookup = fun t ->
                match a.Lookup t with
                | None -> b.Lookup t
                | r -> r
        }

    let Zero =
        {
            Lookup = fun t -> None
        }

    let Concat rules =
        Seq.fold Append Zero rules

    let DefineRule (tt: Targetable<'T1,'T2>) (decide: 'T1 -> option<Recipe<'T2>>) : Rules =
        {
            Lookup = fun t ->
                match TargetBoxes.TryUnpack tt t with
                | None -> None
                | Some key ->
                    match decide key with
                    | None -> None
                    | Some r -> Some (Recipes.Map box r)
        }

    let TryFind target rules =
        let t = TargetBoxes.Create target
        match rules.Lookup t with
        | None -> None
        | Some r -> Some (Recipes.Map unbox r)
