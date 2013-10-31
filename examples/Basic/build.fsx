/// Adaptation of the simple example from Neil Mitchell's Shake:
/// http://neilmitchell.blogspot.com/2012/02/shake-better-make.html
///
/// Still lots of work needed to make this parctical.


#r "System.IO.Compression"
#r "System.IO.Compression.FileSystem"
#r "../../build/FShake.dll"

open FShake
open System.IO
open System.IO.Compression

let ( *> ) = Files.Match

let createZip (out: string) (files: seq<string>) =
    let f = FileInfo(out)
    printfn "creating %s" out
    if f.Exists then
        f.Delete()
    use zip = ZipFile.Open(out, ZipArchiveMode.Create)
    for f in files do
        let e = zip.CreateEntryFromFile(f, Path.GetFileName(f))
        ()

let rules =
    Rules.Concat [
        "result.zip" *> fun p -> Recipes.Do { // the monad tracks deps
            do! Files.Need ["list.txt"]
            let! contents = Files.ReadLines "list.txt" 
            do! Files.Need contents // including dynamic deps
            return createZip p contents
        }
    ]

/// This will build the zip file IF NECESSARY - tracking changes to
/// all files specified in list.txt.
Builder.BuildTarget { RootDirectory = "." } rules
    (Files.CreateTarget "result.zip")
|> Async.RunSynchronously

