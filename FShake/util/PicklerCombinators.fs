namespace FShake

[<AutoOpen>]
module PicklerCombinators =

    /// Experimental support for n-way product types such as records.
    /// See `product` and `field` combinators.
    module ProductInternals =

        /// Internal type for type-checking intermediate values.
        type Part<'R,'X,'Z> =
            private
            | P of ('R -> 'Z) * ('X -> 'Z -> 'R) * Pickler<'Z>

        let private pp f g t =
            P (f, g, t)

        let private finish () =
            pp ignore (fun r () -> r) Pickler.Unit

        /// Internal type for type-checking intermediate values.
        type Wrap<'T> =
            internal
            | W of 'T

            /// Defines an extra field.
            static member ( ^+ ) (W f, x) =
                f x

            /// Defines the last field.
            static member ( ^. ) (W f, W x) =
                f (x (finish ()))

        let internal defProduct e p =
            match p with
            | P (f, g, t) ->
                Pickler.Wrap (g e) f t

        let internal defField proj tf p =
            match p with
            | P (g, h, tr) ->
                pp
                    (fun rr -> (proj rr, g rr))
                    (fun c fx -> h (c (fst fx)) (snd fx))
                    (Pickler.Pair tf tr)

    /// Experimental support for n-way sum types such as unions.
    /// See `sum`.
    module SumInternals =

        /// Internal type for type-checking intermediate values.
        type Part<'U,'T,'X,'Y> =
            private
            | P of Pickler<'X> * ('X -> 'U) * (('X -> 'Y) -> ('T -> 'Y))

        let private defP p f g =
            P (p, f, g)

        let private defLastCase inj p =
            defP p inj (fun h t -> t h)

        let private defNextCase inj p (P (tr, xu, f)) =
            defP (Pickler.Choice2 p tr)
                (function
                    | Choice1Of2 x -> inj x
                    | Choice2Of2 x -> xu x)
                (fun g h ->
                    f (fun x -> g (Choice2Of2 x))
                        (h (fun x -> g (Choice1Of2 x))))

        let private defSum ev (P (tr, xu, f)) =
            Pickler.Wrap xu (fun u -> f (fun x -> x) (ev u)) tr

        /// Internal type for type-checking intermediate values.
        type Case<'T1,'T2> =
            internal
            | C of 'T1 * 'T2

            /// Adds a case.
            static member ( ^+ ) (C (i1, p1), W x) =
                W (defNextCase i1 p1 x)

            /// Adds the last case.
            static member ( ^. ) (C (i1, p1), C (i2, p2)) =
                W (defNextCase i1 p1 (defLastCase i2 p2))

        /// Internal type for type-checking intermediate values.
        and Wrap<'T> =
            internal
            | W of 'T

            /// Adds a case.
            static member ( ^+ ) (W f, W x) =
                f x

            /// Adds the last case.
            static member ( ^. ) (W f, C (inj, p)) =
                f (defLastCase inj p)

        let internal makeCase inj p =
            C (inj, p)

        let internal makeSum f =
            W (defSum f)

    module Pickler =

        /// Starts defining a pickler for an n-ary product, such as
        /// record. Example:
        ///
        ///    type Person =
        ///        {
        ///            Address : string
        ///            Age : int
        ///            Name : string
        ///        }
        ///
        ///    let makePerson name age address =
        ///        {
        ///            Address = address
        ///            Age = age
        ///            Name = name
        ///        }
        ///
        ///    let personPickler =
        ///        Pickler.product makePerson
        ///        ^+ Pickler.field (fun p -> p.Name) Pickler.string
        ///        ^+ Pickler.field (fun p -> p.Age) Pickler.int
        ///        ^. Pickler.field (fun p -> p.Address) Pickler.string
        ///
        /// The implementation is not currently efficient, though it
        /// may improve in the future.
        let Product f =
            ProductInternals.W (ProductInternals.defProduct f)

        /// See `product`.
        let Field f p =
            ProductInternals.W (ProductInternals.defField f p)

        /// Starts defining a pickler for an n-ary sum type, such as
        /// a union type. For example:
        ///
        ///    type UnionT =
        ///        | Case1
        ///        | Case2 of int
        ///        | Case3 of string * int
        ///
        ///    let unionTPickler =
        ///        Pickler.sum (fun x k1 k2 k3 ->
        ///            match x with
        ///            | Case1 -> k1 ()
        ///            | Case2 x -> k2 x
        ///            | Case3 (x, y) -> k3 (x, y))
        ///        ^+ Pickler.variant Case1
        ///        ^+ Pickler.case Case2 Pickler.int
        ///        ^. Pickler.case Case3 (Pickler.pair Pickler.string Pickler.int)
        ///
        /// Note that the implementation is not currently efficient,
        /// though it may improve in the future.
        let Sum f =
            SumInternals.makeSum f

        /// See `sum`.
        let Case inj p =
            SumInternals.makeCase inj p

        /// Useful for union cases without arguments.
        let Variant v =
            Case (fun () -> v) Pickler.Unit
