namespace FShake

open System
open System.IO
open System.Text

module Files =

    let TakeSnapshot (path: string) =
        Recipes.Do {
            let f = FileInfo(path)
            return (f.LastAccessTime : FileTargets.FileSnapshot)
        }

    let Match (pattern: string) (main: string -> Recipe<unit>) =
        Rules.DefineRule FileTargets.Targetable (fun path ->
            if pattern = path then
                Some <| Recipes.Do {
                    do! main path
                    return! TakeSnapshot path
                }
            else None)

    let CreateTarget (path: string) =
        Target.Create FileTargets.Targetable path

    let Need (paths: seq<string>) =
        Recipes.Do {
            for p in paths do
                do!
                    CreateTarget p
                    |> Recipes.Require
                    |> Recipes.Ignore
            return ()
        }

    let ReadLinesWithEncoding enc file =
        Recipes.Do {
            do! Need [file]
            return File.ReadLines(file, enc)
        }

    let ReadLines file =
        ReadLinesWithEncoding Encoding.Default file
