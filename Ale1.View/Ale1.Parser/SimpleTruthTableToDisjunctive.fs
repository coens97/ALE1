module Ale1.Functional.SimpleTruthTableToDisjunctive

open Ale1.Common.TruthTable
open System

let private notStatement(variableAndResult : string * bool) =
    match variableAndResult with
    | (name,true) -> [name]
    | (name,false) -> ["~("] @ [name] @ [")"]

let rec private andStatements (variables : (string * (bool option)) list) = 
    match variables with
    | [variable] -> 
        match variable with
        | (v,Some x) -> Some(notStatement (v,x))
        | _ -> None
    | head :: tail -> 
        match head with
        | (v,Some x) -> 
            match (andStatements tail) with
            | Some recurRes -> 
            Some(["&("] @ (notStatement (v,x)) @ [","] @ recurRes @ [")"])
            | _ -> Some(notStatement (v,x))
        | _ -> andStatements tail

let private readRow (bits : SimpleTruthTableRow) (headers : string []) =
    bits.Variables
    |> Array.toList
    |> List.rev
    |> List.zip [0..(headers.Length - 1)]
    |> List.map (fun (x,v) -> (headers.[x], v))
    |> andStatements

let rec private itterate (values : SimpleTruthTableRow list) (headers : string []) =
    match values with
    | head :: tail -> 
        if head.Result then
            let variables = readRow head headers
            let others = itterate tail headers
            match (others, variables) with
            | (Some other, Some vars) -> Some(["|("] @ other @ [","] @ vars @ [")"])
            | (None, Some vars) -> Some(vars)
            | (Some other, None) -> Some(other)
            | (None, None) -> None
            | _ -> raise (new Exception("Couldnt match"))
        else
            itterate tail headers
    | [] -> None

let TruthTableToDisjunctive (input: SimpleTruthTable) =
    let optinalResult =itterate (input.Rows |> Array.toList) input.Headers
    match optinalResult with
    | Some result -> String.concat "" result
    | None -> ""