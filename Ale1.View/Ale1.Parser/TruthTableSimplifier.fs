module Ale1.Functional.TruthTableSimplifier

open System.Collections
open Ale1.Common.TruthTable
open System.Security.Cryptography.X509Certificates

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

// Slice 2 elements out of array
let sliceRow (a : int) (b : int) (row : Option<bool>[]) =
    [0..(row.Length - 1)]
    |> List.where (fun x -> x <> a && x <> b)
    |> List.map (fun x -> row.[x])
    |> List.toArray
let private simplify (a : int) (count : int) (rows : SimpleTruthTableRow list) =
    let q = rows.Head.Variables
    let trueRows = 
        rows
        |> List.where (fun x -> x.Variables.[a] = Some(true))

    let otherRows = 
        [0..(count - 2)]
        |> List.map (fun x -> 
            if x >= a then (a, x + 1) else (a, x))
        |> List.map (fun (x, y) -> 
            trueRows
            |> List.map (fun z -> (sliceRow x y z.Variables, z.Result)))
        
    rows
let rec private iterate (a : int) (count : int) (rows : SimpleTruthTableRow list) =
    let newRows = 
        simplify a count rows

    // Call next iteration
    if a + 1 = count then
        newRows
    else
        iterate (a + 1) count newRows

let private simplifyRows (count : int) (rows : SimpleTruthTableRow list) = 
    iterate 0 count rows

let toSimpleTruthTable (table : TruthTable) = 
    let headerCount = table.Headers.Length
    let rows = 
        toSimpleRows headerCount table.Values
        |> simplifyRows headerCount
        |> List.toArray

    new SimpleTruthTable(Headers = table.Headers, Rows = rows)