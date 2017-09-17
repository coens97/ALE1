﻿module Ale1.Functional.TruthTableSimplifier

open System.Collections
open Ale1.Common.TruthTable
open System.Security.Cryptography.X509Certificates
open System.Windows.Markup
open System.Linq

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

// Slice 1 element out of array
let sliceSingleRow (a : int) (row : Option<bool>[]) =
    [0..(row.Length - 1)]
    |> List.where (fun x -> x <> a)
    |> List.map (fun x -> row.[x])
    |> List.toArray
// Slice 2 elements out of array
let sliceRow (a : int) (b : int) (row : Option<bool>[]) =
    [0..(row.Length - 1)]
    |> List.where (fun x -> x <> a && x <> b)
    |> List.map (fun x -> row.[x])
    |> List.toArray

// Slice 2 elements out of array
let sliceRowReturn (a : int) (b : int) (roworiginal : Option<bool>[] * SimpleTruthTableRow) =
    let (row,original) = roworiginal
    (sliceRow a b row, original, row.[a], row.[b])

// Insert 1 element back into array
let insertSingleInRow (a : int) (aValue : bool option) (row : Option<bool>[]) =
    [0..(row.Length)]
    |> List.map (fun x ->
        match x with
        | i when i < a -> row.[x]
        | i when i = a -> aValue
        | _ -> row.[x - 1]
        )
    |> List.toArray

// Insert 2 elements back into array
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

// Merge combineses results with both *
// 1 *
// 0 *
// * 1
// * 1 --> * *
let private merge (a : int) (count : int) (rows : SimpleTruthTableRow list) =
    if count > 2 then
        let slicedRow = 
            rows
            |> List.map (fun x -> (sliceSingleRow a x.Variables, x))
        let max = count-2
        // Create sequence like (0,1) (0,2) (1,2)
        [0..max]
        |> List.collect (fun x ->
            [(x+1)..max]
            |> List.map(fun y -> (x,y)))
        |> List.fold (fun r1 (x,y) -> 
            r1
            |> List.map(fun q -> sliceRowReturn x y q)
            |> List.groupBy (fun (row, _fullrow, _vala, _valb) -> rowToString row) // Group by same result and other collumns
            |> List.fold (fun r2 (_key, r) -> 
                let anyA = r |> List.exists(fun (_,_,aval,_) -> aval = None)
                let anyB = r |> List.exists(fun (_,_,_,bval) -> bval = None)
                let allResultTrue = r |> List.exists(fun (_,theRow,_,_) -> theRow.Result = false) |> not
                let (sliced,_,_,_) = r.Head
                if anyA && anyB && allResultTrue then
                    let newRow = insertInRow x None y None sliced
                    let tableRow = new SimpleTruthTableRow(Variables = (insertSingleInRow a (Some(true)) newRow), Result = true) // Assuming true result
                    [(newRow, tableRow)] @ r2
                    |> List.where (fun (_,full) -> 
                        r |> List.exists (fun (_,m,_,_) -> Enumerable.SequenceEqual(m.Variables, full.Variables)) |> not)
                else
                    r2) r1
            ) slicedRow
        |> List.map(fun (_sl, res) -> res)
    else
        rows

let private simplify (a : int) (count : int) (rows : SimpleTruthTableRow list) =
    let trueRows = 
        rows
        |> List.where (fun x -> x.Variables.[a] = Some(true))
    
    [0..(count - 2)]
    |> List.map (fun x -> 
        if x >= a then (a, x + 1) else (a, x))
    // Created pairs like (1,0)  (1,2) (1,3)...
    |> List.collect (fun (x, y) -> 
        trueRows
        |> List.map (fun z -> (sliceRow x y z.Variables, z.Variables, z.Result)) // Slice columns, collumn x is true, collumn y can be both
        |> List.groupBy (fun (k, _, _) -> rowToString k) // group by the same row, except for collumn x and y
        |> List.collect (fun (_, rows) -> 
            let simplifyable = 
                rows
                |> List.exists(fun (_slicedrow, _row, result) -> not result)
                |> not
            if simplifyable then
                let (slicedRow, _, _) = rows.Head
                let simplerRow = insertInRow x (Some(true)) y None slicedRow
                [new SimpleTruthTableRow(Variables = simplerRow, Result = true)]
            else
                rows
                |> List.map (fun (_slicedrow, row, result) ->
                    new SimpleTruthTableRow(Variables = row, Result = result))
            )
        )
    |> merge a count 

let private iterate (count : int) (rows : SimpleTruthTableRow list) =
    // All zeros have to be added seperately
    let zeros = 
        rows
        |> List.find (fun x -> 
            x.Variables
            |> Array.exists (fun y -> y = Some(true))
            |> not)
    
    let simplifiedRows = 
        [0..(count - 1)]
        |> List.collect(fun x -> simplify x count rows)

    """let mergedRows = 
        [0..(count - 1)]
        |> List.fold(fun acc x -> 
            merge x count acc
            ) simplifiedRows"""//another merge is not possible
    [zeros] @ simplifiedRows

let toSimpleTruthTable (table : TruthTable) = 
    let headerCount = table.Headers.Length
    let rows = 
        toSimpleRows headerCount table.Values
        |> iterate headerCount
        |> List.groupBy (fun x -> rowToString x.Variables) // Ugly way of doing distinct
        |> List.map (fun (x,y) -> y.Head)
        |> List.toArray

    new SimpleTruthTable(Headers = table.Headers, Rows = rows)