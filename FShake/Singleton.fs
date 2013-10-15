namespace FShake

[<Sealed>]
type Singleton<'T when 'T : (new : unit -> 'T)> private () =
    static let s = Singleton<'T>()
    static let t = new 'T()
    member s.Instance = t
    member s.Type = typeof<'T>
    static member TheEvidence = s
    static member TheInstance = t

module Singleton =

    let Is<'T when 'T : (new : unit -> 'T)> =
        Singleton<'T>.TheEvidence

    let Instance<'T when 'T : (new : unit -> 'T)> =
        Singleton<'T>.TheInstance
