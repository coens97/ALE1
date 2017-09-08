module Ale1.Functional.BitarrayUtility

open System.Collections

let IntToBits (n : int) (count : int) : BitArray =
    let array = new BitArray([| n |])
    array.Length <- count
    array

let BitsToString (inputBits : BitArray) : string = 
    [0..(inputBits.Length - 1)] // Shamelessly making a "for loop"
    |> List.map(fun x-> if inputBits.Get(x) then "1" else "0")
    |> String.concat("")
