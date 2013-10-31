namespace FShake

module Pickler =

    type Writer =
        {
            BinaryWriter : BinaryWriter
        }

    type Reader =
        {
            BinaryReader : BinaryReader
        }

    type T<'T> =
        {
            Pickle : Writer -> 'T -> unit
            Unpickle : Reader -> 'T
        }

    type Writer with
        member w.Write(p, x) =
            p.Pickle w x

    type Reader with
        member r.Read(p) =
            p.Unpickle r

    let FromPrimitives read write =
        {
            Pickle = write
            Unpickle = read
        }

    let Int64 =
        FromPrimitives
            (fun r -> r.BinaryReader.ReadInt64())
            (fun w x -> w.BinaryWriter.Write(x))

    let String =
        FromPrimitives
            (fun r ->
                let notNull = r.BinaryReader.ReadBoolean()
                if notNull then
                    r.BinaryReader.ReadString()
                else null)
            (fun w x ->
                match x with
                | null ->
                    w.BinaryWriter.Write(false)
                | s ->
                    w.BinaryWriter.Write(true)
                    w.BinaryWriter.Write(s))

    let Wrap f1 f2 p =
        FromPrimitives
            (fun r -> f1 (p.Unpickle r))
            (fun w x -> p.Pickle w (f2 x))

    let DateTime =
        Int64
        |> Wrap DateTime.FromBinary
            (fun d -> d.ToBinary())

    let ReadFromStream p s =
        use r = new BinaryReader(s, encoding = Encoding.Default, leaveOpen = true)
        p.Unpickle { BinaryReader = r }

    let WriteToStream p s v =
        use w = new BinaryWriter(s, encoding = Encoding.Default, leaveOpen = true)
        p.Pickle { BinaryWriter = w } v

    let Unpickle p bytes =
        use s = new MemoryStream(bytes: byte[])
        ReadFromStream p s

    let Pickle p v =
        use s = new MemoryStream()
        WriteToStream p s v
        s.ToArray()

    let Unit =
        FromPrimitives
            (fun r -> ())
            (fun w () -> ())

    let Bytes =
        FromPrimitives
            (fun r ->
                let n = r.BinaryReader.ReadInt32()
                r.BinaryReader.ReadBytes(n))
            (fun w x ->
                w.BinaryWriter.Write(Array.length x)
                w.BinaryWriter.Write(x))

    let Seq p =
        FromPrimitives
            (fun r ->
                let out = ResizeArray()
                let rec loop () =
                    let n = r.BinaryReader.ReadBoolean()
                    if n then
                        let v = r.Read(p)
                        out.Add(v)
                        loop ()
                loop ()
                out.ToArray() :> seq<_>)
            (fun w xs ->
                for x in xs do
                    w.BinaryWriter.Write(true)
                    w.Write(p, x)
                w.BinaryWriter.Write(false))

    let List p =
        Wrap List.ofSeq List.toSeq (Seq p)

    let Pair p1 p2 =
        FromPrimitives
            (fun r ->
                let a = r.Read(p1)
                let b = r.Read(p2)
                (a, b))
            (fun w (x, y) ->
                w.Write(p1, x)
                w.Write(p2, y))

    let PairSeq p1 p2 =
        Seq (Pair p1 p2)

    let Choice2 p1 p2 =
        FromPrimitives
            (fun r ->
                let first = r.BinaryReader.ReadBoolean()
                if first then
                    let a = r.Read(p1)
                    Choice1Of2 a
                else
                    let b = r.Read(p2)
                    Choice2Of2 b)
            (fun w x ->
                match x with
                | Choice1Of2 x ->
                    w.BinaryWriter.Write(true)
                    w.Write(p1, x)
                | Choice2Of2 x ->
                    w.BinaryWriter.Write(false)
                    w.Write(p2, x))

    let Type =
        String
        |> Wrap
            (fun s -> Type.GetType(s, throwOnError = true))
            (fun t -> t.AssemblyQualifiedName)

type Pickler<'T> = Pickler.T<'T>
