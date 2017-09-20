module Ale1.Functional.TreeToTruthTable

open Ale1.Common.TreeNode
open Ale1.Functional.BitarrayUtility
open System
open System.Collections
open System.Runtime.InteropServices
open Ale1.Common.TruthTable

// Recursively getall tree variables
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

let AllTreeVariables(inputTree : ITreeNode) : string array =
    inputTree
    |> AllTreeVariablesItteration
    |> List.distinct
    |> List.sortWith (fun a b -> String.Compare(a, b))
    |> List.toArray

// Generate dictionary; between the variable as string and boolean value
let private CreateHeaderToValueMapping (names : string array) (values : BitArray) =
    values 
    |> BitToSeq // To sequence of booleans
    |> Seq.zip names // put names with the values together
    |> dict // Make dictionary

// Traversal of the tree
// Test logic except for not
let private TestLogic (nodeValue : OperandValue) (values : bool * bool) = 
    match nodeValue with
    | OperandValue.Implication ->
        match values with
        | (true, false) -> false
        | (_, _) -> true
    | OperandValue.BiImplication ->
        match values with
        | (true, true) -> true
        | (false, false) -> true
        | (_, _) -> false
    | OperandValue.And ->
        match values with
        | (true, true) -> true
        | (_, _) -> false
    | OperandValue.Or ->
        match values with
        | (false, false) -> false
        | (_, _) -> true
    | OperandValue.Nand ->
        match values with
        | (true, true) -> false
        | (_, _) -> true
let rec private IterateTestTree (inputTree : ITreeNode) (headerToValue : Generic.IDictionary<string, bool>) : bool =
    match inputTree with
    | :? TreeVariable as v -> headerToValue.[v.Name]
    | :? TreeOperand as o-> 
    match o.NodeValue with
    | OperandValue.Not -> 
        not (IterateTestTree o.Left headerToValue)
    | operand ->
        (IterateTestTree o.Left headerToValue,
         IterateTestTree o.Right headerToValue)
         |> TestLogic operand

let private TestTreeBits (inputTree : ITreeNode) (headers : string array) (values : BitArray) = 
    let headerToValue = CreateHeaderToValueMapping headers values
    IterateTestTree inputTree headerToValue

let TestTreeValue (inputTree : ITreeNode) (values : BitArray) = 
    let headers = AllTreeVariables inputTree
    TestTreeBits inputTree headers values

let CreateTruthTable (inputTree : ITreeNode) =
    let headers = AllTreeVariables inputTree // Get a list of variables
    let headerCount = Array.length headers
    let count =  int(2.0 ** float(headerCount)) - 1 // float is required for the Pow method
    
    let truthtableValues = 
        [0..count]
        |> List.map(fun  x ->
            x
            |> BitarrayUtility.IntToBits headerCount
            |> TestTreeBits inputTree headers)
        |> List.toArray
        |> BitArray
    new TruthTable(Headers = headers, Values = truthtableValues)