module Ale1.Functional.TruthTableToDisjunctive

open Ale1.Common.TruthTable
let private notStatement(variableAndResult : string * bool) =
    match variableAndResult with
    | (name,false) -> [name]
    | (name,true) -> ["~("] @ [name] @ [")"]
let rec private andStatements (variables : (string * bool) list) = 
    match variables with
    | [variable] -> notStatement variable
    | head :: tail -> 
        ["&("] @ (notStatement head) @ [","] @ (andStatements tail) @ [")"]
    | [] -> []

let private readRow (bits : bool seq) (headers : string []) =
    bits
    |> Seq.toList
    |> List.zip [0..(headers.Length - 1)]
    |> List.map (fun (x,v) -> (headers.[x], v))
    |> andStatements

let rec private itterate (values : ((bool * seq<bool>)list)) (headers : string []) =
    match values with
    | head :: tail -> 
        let (result, bits) = head
        if result then
            let variables = readRow bits headers
            let others = itterate tail headers
            match others with
            | Some other -> Some(["|("] @ other @ [","] @ variables @ [")"])
            | None -> Some(variables)
        else
            itterate tail headers
    | [] -> None

let TruthTableToDisjunctive (input: TruthTable) =
    let count = input.Headers |> Array.length
    let results =
        input.Values
        |> BitarrayUtility.BitToSeq
        |> Seq.toList

    let values =
        [0..(results.Length - 1)]
        |> List.map(fun x -> 
            x 
            |> BitarrayUtility.IntToBits count
            |> BitarrayUtility.BitToSeq
            //|> Seq.rev // Shamelessly DOUBLE SWAP
            )
        |> List.zip results

    let optinalResult =itterate values input.Headers
    match optinalResult with
    | Some result -> String.concat "" result
    | None -> ""