namespace FShake

open System
open System.IO
open FsPickler
open FsPickler.Combinators

module FileTargets =

    type FileSnapshot = DateTime
    type LocalFilePath = string

    [<Sealed>]
    type FileTargetable() =
        interface ITargetable<LocalFilePath,FileSnapshot> with
            member __.DefaultRecipe(p) =
                async {
                    let f = FileInfo(p)
                    if f.Exists then
                        return Some f.LastWriteTimeUtc
                    else
                        return None
                }
            member __.KeyEquality = HashIdentity.Structural
            member __.KeyPickler = Pickler.string
            member __.ValuePickler = Pickler.auto<_>
            member __.ShowKey(p) = p
            member __.Validate(p, s) =
                async {
                    let f = FileInfo(p)
                    return f.LastWriteTimeUtc = s
                }

    let Targetable =
        Targetable.Create Singleton.Is<FileTargetable>
