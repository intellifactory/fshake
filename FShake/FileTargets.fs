namespace FShake

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
            member __.KeyPickler = Pickler.String
            member __.ValuePickler = Pickler.DateTime
            member __.ShowKey(p) = p
            member __.Validate(p, s) =
                async {
                    let f = FileInfo(p)
                    return f.LastWriteTimeUtc = s
                }

    let Targetable =
        Targetable.Create Singleton.Is<FileTargetable>
