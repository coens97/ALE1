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

let identicalRow (a: SimpleTruthTableRow) (b: SimpleTruthTableRow)  =
    if a.Result <> b.Result then
        false
    else
        a.Variables
        |> Array.zip b.Variables
        |> Array.exists (fun (x, y) -> (x = y) |> not)
        |> not

let containsRow (rows :SimpleTruthTableRow list) (row: SimpleTruthTableRow) = 
    rows
    |> List.exists (fun x -> identicalRow x row)

let private sameRow (a :int) (mainRow: SimpleTruthTableRow) (compareRow: SimpleTruthTableRow) = 
    [0..(mainRow.Variables.Length - 1)] 
    |> List.toArray 
    |> Array.zip3 mainRow.Variables compareRow.Variables // Combine index with values of both arrays
    |> Array.exists(fun (p,q,i) -> (i = a || p.IsNone || q.IsNone || p.Value = q.Value) |> not)
    |> not

let private findSimilarResults (rows: SimpleTruthTableRow list) (row: SimpleTruthTableRow) (index : int) = 
    let same = rows |> List.where(fun x -> sameRow index row x)
    let allResultSame = 
        same
        |> List.exists(fun x -> (x.Result = row.Result) |> not)
        |> not
    (allResultSame && same.Length > 1, same)

let rec private iterate (rows : SimpleTruthTableRow list) =
    let toBeAdded = new List<SimpleTruthTableRow>()// Use a mutable list #functional
    let toBeRemoved = new List<SimpleTruthTableRow>()// Use a mutable list #functional
    let rec checkRow (current : SimpleTruthTableRow list) =
        let rec checkCollumn (row : SimpleTruthTableRow) (t : SimpleTruthTableRow list) (collumn : int list) =
            match collumn with
            | head :: tail ->
                let (found, same) = findSimilarResults rows row head
                if found then
                    let variables = row.Variables |> Array.mapi(fun i x -> if i = head then None else x) // add collumn with star
                    let row = new SimpleTruthTableRow(Variables = variables, Result = row.Result)
                    if containsRow rows row |> not then
                        toBeAdded.Add(row)
                        toBeRemoved.AddRange(same)
                    //iterate (others @ [row])
                checkCollumn row t tail
            | [] -> checkRow t
        match current with
        | head :: tail ->
            [0..(head.Variables.Length - 1)]
            |> checkCollumn head tail 
        | [] -> rows
    checkRow rows
    if toBeAdded.Count > 0 then
        let newRows = 
            rows
            |> List.where(fun x -> containsRow (toBeRemoved |> List.ofSeq) x |> not)
        let addedRows = toBeAdded |> List.ofSeq |> distinctRows
        iterate (newRows @ addedRows)
    else
        rows

let toSimpleTruthTable (table : TruthTable) = 
    let headerCount = table.Headers.Length
    let rows = 
        toSimpleRows headerCount table.Values
        |> iterate
        //|> distinctRows
        |> List.toArray

    new SimpleTruthTable(Headers = table.Headers, Rows = rows)
