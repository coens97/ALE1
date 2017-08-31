module Ale1.Parser.TextToTree

open Ale1.Common.TreeNode
open System

// Shamelessly stolen from Stackoverflow: https://stackoverflow.com/questions/3722591/pattern-matching-on-the-beginning-of-a-string-in-f
// It is an active pattern which matches the beggining of the string, return the rest of the string
let private (|Prefix|_|) (p:string) (s:string) =
    if s.StartsWith(p) then
        Some(s.Substring(p.Length))
    else
        None
 // Active pattern to select which operand
let private  (|PrefixOperand|_|) (s:string) =
    match s with
    | Prefix "~" rest -> Some(OperandValue.Not, rest)
    | Prefix ">" rest -> Some(OperandValue.Implication, rest)
    | Prefix "=" rest -> Some(OperandValue.BiImplication, rest)
    | Prefix "&" rest -> Some(OperandValue.And, rest)
    | Prefix "|" rest -> Some(OperandValue.Or, rest)
    | _ -> None // Didn't match any operator

let private removeBraces (s:string) = // "(something)" -> "something"
    if s.StartsWith("(") & s.EndsWith(")") then
        s.Substring(1, s.Length - 2)
    else
        raise (new ArgumentException("Expected an ( at the begining and ) at the end of this string: " + s))
    
let private splitComma (s:string) =
    // findComma recursively finds the appropriate comma, count the braces to make sure the right comma is picked
    let rec findComma (nrBraces: int) (pos: int) (s: char list) : int =
        match s with
        |[] -> raise (new ArgumentException("Could not find comma inside braces")) // Empty array, finished without finding
        |'(' :: tail -> findComma (nrBraces + 1) (pos + 1) tail
        |')':: tail -> findComma (nrBraces - 1) (pos + 1) tail
        |',' :: _tail when nrBraces = 0 -> pos // Found the right comma, return position
        |',' :: tail -> findComma nrBraces (pos + 1) tail
        | _ :: tail -> findComma nrBraces (pos + 1) tail // Matched any other character
    
    let commaPosition = [for c in s -> c] |> findComma 0 0 // String -> char list, then call recursive function with initial values
    (s.Substring(0, commaPosition), s.Substring(commaPosition + 1, s.Length - (commaPosition + 1)))

let rec private iterate (inputText : string) : ITreeNode =
    match inputText with
    | PrefixOperand (operand, rest) ->
        let innerText = removeBraces rest
        match operand with
        | OperandValue.Not -> // Not only has left node
            let left = iterate innerText
            upcast new TreeOperand(NodeValue = operand, Left = left)
        | _ -> // Others have both left and right node
            let (leftText, rightText) = splitComma innerText
            let left = iterate leftText
            let right = iterate rightText
            upcast new TreeOperand(NodeValue = operand, Left = left, Right = right)
    | _ -> // If there is no opperand, assume it is a variable
        upcast new TreeVariable(Name = inputText)

let Parse (inputText : string) = // Called from outside
    inputText.Replace(" ", "") 
    |> iterate // Itterate is called without whitespaces