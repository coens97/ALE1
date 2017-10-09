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

let similarRow (a: SimpleTruthTableRow) (b: SimpleTruthTableRow) (skipCollumn: int) =
    if a.Result <> b.Result then
        false
    else
        ([0..(a.Variables.Length-1)] |> List.toArray) // collumn indexes
        |> Array.zip3 a.Variables b.Variables
        |> Array.exists (fun (x, y, collumn) -> 
            (collumn = skipCollumn || x.IsNone || y.IsNone || x.Value = y.Value) |> not)
        |> not

let sameRow (a: SimpleTruthTableRow) (b: SimpleTruthTableRow)  =
    if a.Result <> b.Result then
        false
    else
        a.Variables
        |> Array.zip b.Variables
        |> Array.exists (fun (x, y) -> (x = y) |> not)
        |> not

let containsRow (rows :SimpleTruthTableRow list) (row: SimpleTruthTableRow) = 
    rows
    |> List.exists (fun x -> sameRow x row)

let rec private iterate (count : int) (rows : SimpleTruthTableRow list) =
    let  simplifiedRows = new List<SimpleTruthTableRow>() // Using mutable in F# :^)
    let rowsToBeRemoved = Set.empty<int>

    let unchangedRows = 
        rows
        |> List.zip [0..(rows.Length - 1)] //Combine row with the index
        |> List.where(fun (index,row) -> 
            if rowsToBeRemoved.Contains(index) then
                false // Delete current row
            else
                [0..(count-1)] // List of column indexes
                |> List.exists (fun collumn ->
                    let simplifyAble = 
                        [(index + 1)..(rows.Length - 1)] // List of indexes after current row
                        |> List.map (fun x -> (x, rows.[x])) // Combine row with index again
                        |> List.exists (fun (secondIndex, secondRow) ->
                            let similar = similarRow row secondRow collumn
                            if similar then
                                rowsToBeRemoved.Add(secondIndex)
                                true
                            else
                                false
                            )
                    if simplifyAble then//move simplifyable up
                        let rowVars = 
                            row.Variables
                            |> Array.zip ([0..(count - 1)] |> List.toArray)
                            |> Array.map (fun (i,c)-> if i = collumn then None else c)
                        let newRow = new SimpleTruthTableRow(Variables = rowVars, Result = row.Result)
                        if containsRow rows newRow |> not && containsRow (simplifiedRows |> List.ofSeq) newRow |> not then
                            simplifiedRows.Add(newRow)
                        false // Delete current row
                    else
                        true // Keep row
                )
            )
        |> List.map(fun (_,r) -> r) // Remove row index

    // Combine old and new rows
    if simplifiedRows.Count = 0 then
        unchangedRows
    else
        let mergedrows = unchangedRows @ (simplifiedRows |> List.ofSeq)
        iterate count mergedrows

let toSimpleTruthTable (table : TruthTable) = 
    let headerCount = table.Headers.Length
    let rows = 
        toSimpleRows headerCount table.Values
        |> iterate headerCount
        //|> distinctRows
        |> List.toArray

    new SimpleTruthTable(Headers = table.Headers, Rows = rows)
