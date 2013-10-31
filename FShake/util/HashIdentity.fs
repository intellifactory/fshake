namespace FShake

module HashIdentity =

    type T<'T> = IEqualityComparer<'T>

    let Box (eq: T<'T>) : T<obj> =
        HashIdentity.FromFunctions
            (fun x -> eq.GetHashCode(unbox x))
            (fun x y ->
                match y with
                | :? 'T as y -> eq.Equals(unbox x, y)
                | _ -> false)
