module Ale1.Parser.TreeToText

open Ale1.Common.TreeNode
open System.Text
open System

// Change enumeration to the according ASCII character
let private operandToText(operand : OperandValue) : string = 
    match operand with
    | OperandValue.Not -> "~"
    | OperandValue.Implication -> ">"
    | OperandValue.BiImplication -> "="
    | OperandValue.And -> "&"
    | OperandValue.Or -> "|"
    | _ -> raise (new ArgumentException("Can't recognise enumuration value of OperandValue when parsing tree")) // No match

let rec private iteration (inputTree : ITreeNode) : string list = 
    match inputTree with
    | :? TreeVariable as v -> [ v.Name ] // When node match with a variable only return that
    | :? TreeOperand as o->
        match o.NodeValue with // The NOT operand is the only without a right node
        | OperandValue.Not -> 
            [operandToText o.NodeValue; "("] @
            iteration o.Left @
            [ ")" ]
        | _ ->
            [operandToText o.NodeValue; "("] @
            iteration o.Left @
            [ "," ] @
            iteration o.Right @
            [ ")" ]
    | _ -> raise (new ArgumentException("Can't recognise ITreeNode type when parsing tree")) // No match

let ToText (inputTree : ITreeNode) : string =
    iteration inputTree
    |> String.concat("") // Put the results together in one string