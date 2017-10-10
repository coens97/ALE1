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

let rowToString (row: Option<bool>[]) =
    row
    |> Array.map(fun x ->
        match x with
        | Some(true) -> "1"
        | Some(false) -> "0"
        | None -> "*")
    |> String.concat ""

let distinctRows (rows: SimpleTruthTableRow list) =
    rows
    |> List.groupBy (fun x -> rowToString x.Variables) // Ugly way of doing distinct
    |> List.map (fun (_x,y) -> y.Head)

let private makeColumnCombinations(count: int) =
    // Create all collumn combinations
    // e.g. count = 3
    // (0,1) (0,2) (1,2) 
    [0..(count - 2)]
    |> List.collect (fun x -> 
        [(x+1)..(count - 1)]
        |> List.map(fun a -> (x, a))
     )

let private sameRow (a :int) (b :int) (mainRow: SimpleTruthTableRow) (compareRow: SimpleTruthTableRow) = 
    [0..(mainRow.Variables.Length - 1)] 
    |> List.toArray 
    |> Array.zip3 mainRow.Variables compareRow.Variables // Combine index with values of both arrays
    |> Array.exists(fun (p,q,i) -> (i = a || i = b || p.IsNone || q.IsNone || p.Value = q.Value) |> not)
    |> not

let private findSimilarResults (similarRows: SimpleTruthTableRow list) (row: SimpleTruthTableRow) (index : int) (value: bool) = 
    let sameRows = similarRows |> List.where(fun x -> x.Variables.[index].IsSome && x.Variables.[index].Value = value)
    let optionalRows = similarRows |> List.where(fun x -> x.Variables.[index].IsNone)
    let allResultSame = 
        (sameRows @ optionalRows) 
        |> List.exists(fun x -> (x.Result = row.Result) |> not)
        |> not
    (optionalRows, allResultSame && sameRows.Length + optionalRows.Length > 1)

let rec private iterate (count : int) (rows : SimpleTruthTableRow list) =
    let rec checkCollumns (collumns: (int * int) list) =
        let rec checkRows (a :int) (b :int) (tailCollumns: (int * int) list) (tailRows: SimpleTruthTableRow list) =
            match tailRows with
            | row::newTailRows ->
                if row.Variables.[a].IsNone || row.Variables.[b].IsNone then
                    checkRows a b tailCollumns newTailRows
                else
                    let similarRows = rows |> List.where(fun x -> sameRow a b row x)
                    let (leftOptional, leftSimplify) = findSimilarResults similarRows row a row.Variables.[a].Value
                    let (rightOptional, rightSimplify) = findSimilarResults similarRows row b row.Variables.[b].Value
                    match (leftSimplify, rightSimplify) with
                    | (true, true) -> 
                        let otherRows = 
                            (rows |> List.where(fun x -> (sameRow a b row x) |> not)) @ 
                            (similarRows |> List.where(fun x -> (x.Variables.[a].IsSome && x.Variables.[a].Value <> row.Variables.[a].Value) ||
                                (x.Variables.[b].IsSome && x.Variables.[b].Value <> row.Variables.[b].Value))) @
                                leftOptional @
                                rightOptional
                            |> distinctRows
                        let newVariablesLeft = row.Variables |> Array.mapi(fun i x -> if i = b then None else x) // add collumn with star
                        let newRowLeft = new SimpleTruthTableRow(Variables = newVariablesLeft, Result = row.Result)
                        let newVariablesRight = row.Variables |> Array.mapi(fun i x -> if i = b then None else x) // add collumn with star
                        let newRowRight = new SimpleTruthTableRow(Variables = newVariablesRight, Result = row.Result)
                        iterate count (otherRows @  [newRowLeft;newRowRight])
                    | (true, false) -> 
                        let otherRows = (rows |> List.where(fun x -> (sameRow a b row x) |> not)) @ (similarRows |> List.where(fun x -> x.Variables.[a].IsSome && x.Variables.[a].Value <> row.Variables.[a].Value))
                        let newVariables = row.Variables |> Array.mapi(fun i x -> if i = b then None else x) // add collumn with star
                        let newRow = new SimpleTruthTableRow(Variables = newVariables, Result = row.Result)
                        iterate count (otherRows @ leftOptional @ [newRow])
                    | (false, true) -> 
                        let otherRows = (rows |> List.where(fun x -> (sameRow a b row x) |> not)) @ (similarRows |> List.where(fun x -> x.Variables.[b].IsSome && x.Variables.[b].Value <> row.Variables.[b].Value))
                        let newVariables = row.Variables |> Array.mapi(fun i x -> if i = a then None else x) // add collumn with star
                        let newRow = new SimpleTruthTableRow(Variables = newVariables, Result = row.Result)
                        iterate count (otherRows @ rightOptional @ [newRow])
                    | _ -> checkRows a b tailCollumns newTailRows
            | [] -> checkCollumns tailCollumns
            //End: checkRows

        match collumns with
        | (a,b)::tail -> checkRows a b tail rows
        | [] -> rows // checked all collumn combinations
        // End: checkCollumns
    makeColumnCombinations count
    |> checkCollumns

let toSimpleTruthTable (table : TruthTable) = 
    let headerCount = table.Headers.Length
    let rows = 
        toSimpleRows headerCount table.Values
        |> iterate headerCount
        //|> distinctRows
        |> List.toArray

    new SimpleTruthTable(Headers = table.Headers, Rows = rows)
