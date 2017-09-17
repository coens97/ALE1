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

let insertInRow (a : int) (aValue : bool option) (b : int) (bValue : bool option) (row : Option<bool>[]) =
    let (low, lowValue, high, highValue) = if a < b then (a, aValue, b, bValue) else (b, bValue,a, aValue)
    [0..(row.Length + 1)]
    |> List.map (fun x ->
        match x with
        | i when i < low -> row.[x]
        | i when i = low -> lowValue
        | i when i < high -> row.[x - 1]
        | i when i = high -> highValue
        | _ -> row.[x - 2]
        )
    |> List.toArray

let rowToString (row: Option<bool>[]) =
    row
    |> Array.map(fun x ->
        match x with
        | Some(true) -> "1"
        | Some(false) -> "0"
        | None -> "*")
    |> String.concat ""

let private simplify (a : int) (count : int) (rows : SimpleTruthTableRow list) =
    let trueRows = 
        rows
        |> List.where (fun x -> x.Variables.[a] = Some(true))

    let simplifiedRows = 
        [0..(count - 2)]
        |> List.map (fun x -> 
            if x >= a then (a, x + 1) else (a, x))
        // Created pairs like (1,0)  (1,2) (1,3)...
        |> List.map (fun (x, y) -> 
            trueRows
            |> List.map (fun z -> (sliceRow x y z.Variables, z.Variables, z.Result)) // Slice columns, collumn x is true, collumn y can be both
            |> List.groupBy (fun (k, _, _) -> rowToString k) // group by the same row, except for collumn x and y
            |> List.map (fun (_, rows) -> 
                let simplifyable = 
                    rows
                    |> List.exists(fun (_slicedrow, _row, result) -> not result)
                    |> not
                if simplifyable then
                    rows
                    |> List.map (fun (_slicedrow, row, result) ->
                        new SimpleTruthTableRow(Variables = row, Result = result))
                else
                    rows
                    |> List.map (fun (_slicedrow, row, result) ->
                        new SimpleTruthTableRow(Variables = row, Result = result))
                )
            )
        //|> List.groupBy (fun (x, _, _) -> x)
        
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