namespace FShake

module Examples =

    let gcc (input: FileTargets.FileSnapshot) =
        printfn "GCC.."

    let takeSnapshot (path: string) : FileTargets.FileSnapshot =
        let f = System.IO.FileInfo(path)
        f.LastWriteTimeUtc

    let rules =
        Rules.Concat [
            Rules.DefineRule FileTargets.Targetable (fun file ->
                printfn "TRYING FILE %O" file
                if file.EndsWith(".o") then
                    printfn "MATCHED"
                    let recipe =
                        Recipes.Do {
                            do printfn "Executing recipe"
                            let! f =
                                Target.Create FileTargets.Targetable
                                    (System.IO.Path.ChangeExtension(file, ".c"))
                                |> Recipes.Require
                            do gcc f
                            return takeSnapshot file
                        }
                    Some recipe
                else None)
        ]

    let cfg : Builder.Config =
        {
            RootDirectory = "."
        }

    let test () =
        printfn "STARTING IN %s" cfg.RootDirectory
        let snap =
            Builder.BuildTarget cfg rules
                (Target.Create FileTargets.Targetable "hello.o")
            |> Async.RunSynchronously
        printfn "DONE IN %s" cfg.RootDirectory

    do test ()
