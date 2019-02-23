namespace Mechanix.PropertyTest

open FsCheck
open FsCheck.Xunit
open Mechanix
open System.Runtime.Serialization.Formatters.Binary

type ForceGenerator =
    static member Gen() =
        Arb.generate<double>
        |> Gen.three
        |> Gen.map (fun (x, y, z) -> Force(x, y, z))
        |> Arb.fromGen

type ForceTest() =
    do Arb.register<ForceGenerator>() |> ignore

    [<Property>]
    let ``Addition is transitive`` (f1 : Force) (f2 : Force) =
        f1.Add(f2).Equals(f2.Add(f1))

    [<Property>]
    let ``Deserialized equals origin`` (f : Force) =
        let formater = new BinaryFormatter()
        let stream = new System.IO.MemoryStream()
        do formater.Serialize(stream, f)
        do stream.Position <- int64 0
        f.Equals(formater.Deserialize stream)
        