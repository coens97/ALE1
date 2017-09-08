module Ale1.Functional.BitarrayUtility

open System.Collections

let IntToBits (count : int) (n : int) : BitArray =
    let array = new BitArray([| n |])
    array.Length <- count
    array

let BitToSeq (inputBits : BitArray) = 
    [0..(inputBits.Length - 1)] // Shamelessly making a "for loop"
    |> Seq.map(fun x-> inputBits.Get(x))

let BitsToString (inputBits : BitArray) : string = 
    inputBits
    |> BitToSeq
    |> Seq.map(fun x-> if x then "1" else "0")
    |> String.concat("")

let StringToBits (inputString : string) : BitArray = 
    [for c in inputString -> c] // To chars
    |> Seq.map(fun x-> x = '1') // To booleans
    |> Seq.toArray // To bool array
    |> BitArray // To bits