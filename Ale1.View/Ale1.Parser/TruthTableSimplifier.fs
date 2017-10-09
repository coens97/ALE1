module Ale1.Functional.TruthTableSimplifier
open System.Collections.Generic
open System.Collections
open Ale1.Common.TruthTable
open System

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
            //|> Seq.rev // Shamelessly DOUBLE SWAP
            |> Seq.map (fun y -> Some(y))
            |> Seq.toArray)
    // Merge the rows with the results
    input
    |> BitarrayUtility.BitToSeq 
    |> Seq.rev // Shamelessly DOUBLE SWAP
    |> Seq.zip rows 
    |> Seq.map (fun (x, y) -> 
        new SimpleTruthTableRow(Variables = x, Result = y))
    |> Seq.toList

let private makeColumnCombinations(count: int) =
    // Create all collumn combinations
    // e.g. count = 3
    // (0,1) (0,2) (1,0) (1,2)
    [0..(count - 1)]
    |> List.collect (fun x -> 
        [0..(count - 2)]
        |> List.map(fun a -> 
            if x >= a then (a, x + 1) else (a, x))
     )

let private sameRow (a :int) (b :int) (mainRow: SimpleTruthTableRow) (compareRow: SimpleTruthTableRow) = 
    [0..(mainRow.Variables.Length - 1)] 
    |> List.toArray 
    |> Array.zip3 mainRow.Variables compareRow.Variables // Combine index with values of both arrays
    |> Array.exists(fun (p,q,i) -> (i = a || i = b || p.IsNone || q.IsNone || p.Value = q.Value) |> not)
    |> not

let rec private iterate (count : int) (rows : SimpleTruthTableRow list) =
    let rec checkCollumns (count :int) (rows : SimpleTruthTableRow list) (collumns: (int * int) list) =
        let rec checkRows (a :int) (b :int) (tailCollumns: (int * int) list) (rows : SimpleTruthTableRow list) (tailRows: SimpleTruthTableRow list) =
            match tailRows with
            | row::newTailRows ->
                if row.Variables.[a].IsNone then
                    checkRows a b collumns rows newTailRows // skip row with variable None
                else
                    let aValue = row.Variables.[a].Value
                    let similarRows = rows |> List.where(fun x -> sameRow a b row x)
                    let sameRows = similarRows |> List.where(fun x -> x.Variables.[a].Value = aValue)
                    let optionalRows = similarRows |> List.where(fun x -> x.Variables.[a].IsNone)
                    let allResultSame = 
                        (sameRows @ optionalRows) 
                        |> List.exists(fun x -> (x.Result = row.Result) |> not)
                        |> not
                    if allResultSame then
                        let otherRows = (rows |> List.where(fun x -> (sameRow a b row x) |> not)) @ (similarRows |> List.where(fun x -> x.Variables.[a].Value <> aValue))
                        let newVariables = row.Variables |> Array.mapi(fun i x -> if i = b then None else x) // add collumn with star
                        let newRow = new SimpleTruthTableRow(Variables = newVariables, Result = row.Result)
                        iterate count (otherRows @ optionalRows @ [newRow])
                    else
                        checkRows a b collumns rows newTailRows
            | [] -> checkCollumns count rows tailCollumns
            //End: checkRows

        match collumns with
        | (a,b)::tail -> checkRows a b tail rows rows
        | [] -> rows // checked all collumn combinations
        // End: checkCollumns
    makeColumnCombinations count
    |> checkCollumns count rows

let toSimpleTruthTable (table : TruthTable) = 
    let headerCount = table.Headers.Length
    let rows = 
        toSimpleRows headerCount table.Values
        |> iterate headerCount
        //|> distinctRows
        |> List.toArray

    new SimpleTruthTable(Headers = table.Headers, Rows = rows)
