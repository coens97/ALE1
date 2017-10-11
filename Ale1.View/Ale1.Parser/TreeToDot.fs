module Ale1.Functional.TreeToDot

open Ale1.Common.TreeNode
open System

// Change enumeration to the according ASCII character
let private operandToText(operand : OperandValue) : string = 
    match operand with
    | OperandValue.Not -> "~"
    | OperandValue.Implication -> ">"
    | OperandValue.BiImplication -> "="
    | OperandValue.And -> "&"
    | OperandValue.Or -> "|"
    | OperandValue.Nand -> "%"
    | _ -> raise (new ArgumentException("Can't recognise enumuration value of OperandValue when parsing tree")) // No match

// Itteration is a recursive function which "walks through" the tree
// It returns a list of string, because of the assumption that list works best in F# recursion
let rec private iteration (inputTree : ITreeNode) (n : int) =
    match inputTree with
    | :? TreeVariable as v -> (n, [ "node" + n.ToString() + " [ label = \"" + v.Name + "\" ]" ]) // When node match with a variable only return that
    | :? TreeValue as v -> (n, ["node" + n.ToString() + " [ label = \"" + (if v.Value then "1" else "0") + "\" ]" ])
    | :? TreeOperand as o->
        match o.NodeValue with // The NOT operand is the only without a right node
        | OperandValue.Not -> 
            let (m, stringList) = iteration o.Left (n + 1)
            (m,
                ["node" + n.ToString() + " [ label = \"" + operandToText o.NodeValue + "\" ]";
                "node" + n.ToString() + " -- node" + (n + 1).ToString()] @
                stringList)
        | _ ->
            let (rightN, leftStringList) = iteration o.Left (n + 1)
            let (m, rightStringList) = iteration o.Right (rightN + 1)
            (m,
                ["node" + n.ToString() + " [ label = \"" + operandToText o.NodeValue + "\" ]";
                "node" + n.ToString() + " -- node" + (n + 1).ToString();
                "node" + n.ToString() + " -- node" + (rightN + 1).ToString()] @
                leftStringList @
                rightStringList)
    | _ -> raise (new ArgumentException("Can't recognise ITreeNode type when parsing tree")) // No match

let ToText (inputTree : ITreeNode) : string[] =
    let (_, stringList) = iteration inputTree 0

    ["graph logic {" ; "node [ fontname = \"Arial\" ]"] @ // Add static first line of document
    stringList @
    ["}"] // Add last line of document
    |> List.toArray
    