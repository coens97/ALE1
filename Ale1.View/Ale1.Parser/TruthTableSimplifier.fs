module Ale1.Functional.TruthTableSimplifier

open System.Collections
open Ale1.Common.TruthTable

// From the full bitarray result to simplified rows
let private toSimpleRows (count : int) (input : BitArray) =
    // Simplified truth table has each row specified unlike the full truth table where the count can be calculated from the rows
    let rows = 
        [0..(count - 1)]
        |> List.map (fun x -> 
            x 
            |> BitarrayUtility.IntToBits count
            |> BitarrayUtility.BitToSeq
            |> Seq.map (fun y -> Some(y))
            |> Seq.toArray)
    // Merge the rows with the results
    input
    |> BitarrayUtility.BitToSeq 
    |> Seq.zip rows 
    |> Seq.map (fun (x, y) -> 
        new SimpleTruthTableRow(Variables = x, Result = y))
    |> Seq.toArray

let toSimpleTruthTable (table : TruthTable) = 
    let headerCount = table.Headers.Length

    let rows = toSimpleRows headerCount table.Values
    new SimpleTruthTable(Headers = table.Headers, Rows = rows)