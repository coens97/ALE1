module Ale1.Functional.TruthTableSimplifier

open System.Collections
open Ale1.Common.TruthTable

// From the full bitarray result to simplified rows
let private toSimpleRows  (headerCount : int) (input : BitArray) =
    let rowcount = input.Count
    // Simplified truth table has each row specified unlike the full truth table where the count can be calculated from the rows
    let rows = 
        [0..(rowcount - 1)]
        |> List.map (fun x -> 
            x 
            |> BitarrayUtility.IntToBits headerCount
            |> BitarrayUtility.BitToSeq
            |> Seq.map (fun y -> Some(y))
            |> Seq.toArray)
    // Merge the rows with the results
    input
    |> BitarrayUtility.BitToSeq 
    |> Seq.zip rows 
    |> Seq.map (fun (x, y) -> 
        new SimpleTruthTableRow(Variables = x, Result = y))
    |> Seq.toList

let rec private iterate (a : int) (b : int) (count : int) (rows : SimpleTruthTableRow list) =
    let newRows = rows

    // Call next iteration
    if b + 1 = count then
        if a + 2 = count then
            newRows
        else
            iterate (a + 1) (b + 1) count newRows
    else
        iterate a (b + 1) count newRows

let private simplifyRows (count : int) (rows : SimpleTruthTableRow list) = 
    iterate 0 1 count rows

let toSimpleTruthTable (table : TruthTable) = 
    let headerCount = table.Headers.Length
    let rows = 
        toSimpleRows headerCount table.Values
        |> simplifyRows headerCount
        |> List.toArray

    new SimpleTruthTable(Headers = table.Headers, Rows = rows)