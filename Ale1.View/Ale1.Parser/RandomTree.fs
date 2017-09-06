module Ale1.Functional.RandomTree
open System

let rand = Random()
let private charsToString(input: char list) = // Converts a list of characters to a string
    String.Concat(Array.ofList(input))

let private toBeUsedVariables (n : int) : char list = // Return a, b....
    [1..n]
    |> List.map (fun x -> (char ((int 'a') + x)))

let private randomSwap (left : char list) (right : char list) : char list * char list = 
    if rand.Next(0,1) = 0 then
        (left, right)
    else
        (right, left)

let private randomOperand : char =
    match rand.Next(0,3) with
    | 0 -> '>'
    | 1 -> '='
    | 2 -> '&'
    | 3 -> '|'
    | _ -> raise (Exception("Got an unexpected random number"))
    
let private takeVariable  (alphabet: char list) : char list * char list =
    match alphabet with
    | head :: tail ->
        if rand.Next(0, 8) = 1 || List.length alphabet = 1 then // randomly keep a variable in the list or if it is the last variable left
            ([head], alphabet)
        else
            ([head], tail)
    | _ -> raise(new Exception("Alphabet is empty"))

let rec private iterate (alphabet: char list) (openVariables : int) : char list * char list =
    if (rand.Next(0,4) = 0 && List.length alphabet < 4) || List.length alphabet = openVariables then 
        takeVariable alphabet
    else
        if rand.Next(0,4) = 0 then // small change to get not operand
            let (res, alphabet) = iterate alphabet openVariables
            (['~';'('] @
             res @
             [')'], alphabet)
        else
            let operand = randomOperand
            let (firstLeaf, newAlphabet) = iterate alphabet (openVariables + 1)
            let (secondLeaf, newerAlphabet) = iterate newAlphabet openVariables
            let (left, right) = randomSwap firstLeaf secondLeaf
            ([operand;'('] @
             left @
             [','] @
             right @
             [')'], newerAlphabet)

let Make (n : int) : string =
    let alphabet = toBeUsedVariables n
    let (result, _alphabet) = iterate alphabet 0
    result
    |> charsToString