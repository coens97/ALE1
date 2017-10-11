module Ale1.Functional.BitarrayUtility

open System.Collections

let IntToBits (count : int) (n : int) : BitArray =
    let array = new BitArray([| n |])
    array.Length <- count
    array

let BitToSeq (inputBits : BitArray) = 
    [0..(inputBits.Length - 1)] // Shamelessly making a "for loop"
    |> Seq.map(fun x-> inputBits.Get(x))
    |> Seq.rev // Shamelessly reverse the bits

let BitsToString (inputBits : BitArray) : string = 
    inputBits
    |> BitToSeq
    |> Seq.map(fun x-> if x then "1" else "0")
    |> String.concat("")

let BitsToHex (inputBits : BitArray) : string = 
    // BitArray to byte[]
    let byteSize = (inputBits.Length - 1) / 8 + 1
    let bytes = Array.create<byte> byteSize 0uy
    inputBits.CopyTo(bytes, 0)
    // byte[] -> string
    bytes
    |> Array.map (fun (x : byte) -> System.String.Format("{0:X2}", x))
    |> Array.rev
    |> String.concat System.String.Empty


let StringToBits (inputString : string) : BitArray = 
    [for c in inputString -> c] // To chars
    |> Seq.rev // shamelessly reverse
    |> Seq.map(fun x-> x = '1') // To booleans
    |> Seq.toArray // To bool array
    |> BitArray // To bits