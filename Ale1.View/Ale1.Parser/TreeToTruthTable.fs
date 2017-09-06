module Ale1.Functional.TreeToTruthTable

open Ale1.Common.TreeNode
open System
open System.Linq

let rec private AllTreeVariablesItteration(inputTree : ITreeNode) : string list = 
    match inputTree with
    | :? TreeVariable as v -> [v.Name] // When node match with a variable only return that
    | :? TreeOperand as o->
        match o.NodeValue with // The NOT operand is the only without a right node
        | OperandValue.Not -> 
            AllTreeVariablesItteration o.Left
        | _ ->
            AllTreeVariablesItteration o.Left @
            AllTreeVariablesItteration o.Right
    | _ -> raise (new ArgumentException("Can't recognise ITreeNode type when parsing tree")) // No match

let AllTreeVariables(inputTree : ITreeNode) : string list =
    inputTree
    |> AllTreeVariablesItteration
    |> List.distinct
    |> List.sortWith (fun a b -> String.Compare(a, b))