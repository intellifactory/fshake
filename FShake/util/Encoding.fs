namespace FShake

[<AutoOpen>]
module EncodingExtensions =

    let private defaultEncoding =
        let emitIdentifier = false
        let throwOnInvalid = false
        UTF8Encoding(emitIdentifier, throwOnInvalid)

    type System.Text.Encoding with
        static member Default = defaultEncoding
