
namespace Mechanix.PropertyTest

open FsCheck
open FsCheck.Xunit
open Mechanix

type PointMassGen = 
    static member Gen() =
        Arb.generate<double>
        |> Gen.listOfLength 7
        |> Gen.map 
            (fun list -> new PointMass(
                AxisStatus(list.[0], list.[1]),
                AxisStatus(list.[2], list.[3]),
                AxisStatus(list.[4], list.[5]),
                list.[6]))
        |> Arb.fromGen

type PhysicalContextTest() =
    do Arb.register<PointMassGen>() |> ignore
    
    [<Property>]
    let ``Adding entity to context doesn't change it`` (f : PointMass) =
        let context = new PhysicalContext<int>(float 1, 1)
        do context.AddEntity(0, f)
        let copy = context.[0]
        copy.Equals(f)
        