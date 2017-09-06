module Ale1.Functional.TextToTree

open Ale1.Common.TreeNode
open System

let private charsToString(input: char list) = // Converts a list of characters to a string
    String.Concat(Array.ofList(input))

// Active pattern to select which operand
let private (|PrefixOperand|_|) (s:char list) = // Returns both the operand what is behind the operand
    match s with
    | '~' :: '(' :: tail  -> Some(OperandValue.Not, tail)
    | '>' :: '(' ::  tail -> Some(OperandValue.Implication, tail)
    | '=' :: '(' ::  tail -> Some(OperandValue.BiImplication, tail)
    | '&' :: '(' ::  tail -> Some(OperandValue.And, tail)
    | '|' :: '(' ::  tail -> Some(OperandValue.Or, tail)
    | _ -> None // Didn't match any operator

let rec private splitVariable (s: char list) : char list * char list =
    match s with
    | ',':: _tail  -> ([], s)
    | ')' ::  _tail -> ([], s)
    | head :: tail -> 
        let (var, rest) = splitVariable tail
        ([head] @ var, rest)
    | _ -> raise (new ArgumentException("Could not find the end of a variable. Is a , or ) missing?"))

let rec private iterate (inputText : char list) : ITreeNode * char list =
    match inputText with
    | PrefixOperand (operand, rest) ->
        let (tree, rest) = parseOperand operand rest
        match rest with
        | ')' :: tail -> (tree, tail)
        | _ -> raise (new ArgumentException("Expected a closing brace at:" + charsToString rest)) 
    | _ -> // String does not start with an operand, so assume it is a variable
        let (variable, tail) = splitVariable inputText
        (upcast new TreeVariable(Name = charsToString variable), tail)
and private parseOperand (operand: OperandValue) (inputText : char list) : ITreeNode * char list =
    match operand with
    | OperandValue.Not -> // Not, only has left node
        let (left, tail) = iterate inputText
        (upcast new TreeOperand(NodeValue = operand, Left = left), tail)
    | _ -> 
        let (left, rightNodeText) = iterate inputText
        match rightNodeText with
        | ',' :: rightText -> 
            let (right, rest) = iterate rightText
            (upcast new TreeOperand(NodeValue = operand, Left = left, Right = right), rest)
        | _ -> raise (new ArgumentException("Expected a comma at:" + String.Concat(Array.ofList(rightNodeText))))

let Parse (inputText : string) = // Called from outside
    let noSpaces = inputText.Replace(" ", "")
    let (tree, _tail) = [for c in noSpaces -> c] |> iterate // Itterate is called without whitespaces in char list format
    tree