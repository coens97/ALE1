module Ale1.Parser.TreeToText

open Ale1.Common.TreeNode
open System.Text
open System

let private iteration (inputTree : ITreeNode) : string list = 
    match inputTree with
    | :? TreeVariable as v -> [ v.Name ] // When node match with a variable only return that
    | :? TreeOperand as o -> [ "TODO Finish" ]
    | _ -> raise (new ArgumentException("Can't recognise type when parsing tree")) // No match

let ToText (inputTree : ITreeNode) : string =
    iteration inputTree
    |> String.concat("") // Put the results together in one string

