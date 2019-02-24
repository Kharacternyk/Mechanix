namespace Mechanix.PropertyTest

open FsCheck
open FsCheck.Xunit
open Mechanix
open System

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
    do Arb.register<ForceGenerator>() |> ignore

    [<Property>]
    let ``Adding entity to context doesn't change it`` (f : PointMass) =
        let context = new PhysicalContext<int>(float 1, 1)
        do context.AddEntity(0, f)
        let copy = context.[0]
        copy.Equals(f)
        
    [<Property>]
    let ``Constant forces folds into one even`` dt (e : PointMass) (fl : list<Force>) =
        dt > float 0 ==>
        lazy
        let context = new PhysicalContext<int>(dt, 1)
        let laws =
            fl
            |> List.map (fun f -> new System.Func<PhysicalContext<int>, Force>(fun (c : PhysicalContext<int>) -> f))
            |> List.toArray
        do context.AddEntity(0, e, laws)
        do context.Tick()
        let evenf = List.fold (fun (s : Force) f -> s.Add(f)) Force.Zero fl
        context.[0].Equals(e.Next(dt, evenf))