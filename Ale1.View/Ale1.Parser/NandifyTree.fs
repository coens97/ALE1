module Ale1.Functional.NandifyTree

open Ale1.Common.TreeNode

let rec private itterate (input: ITreeNode) =
    match input with
    | :? TreeVariable as v -> input // When node match with a variable only return that
    | :? TreeOperand as o->
        match o.NodeValue with // The NOT operand is the only without a right node
        | OperandValue.Not ->
            upcast new TreeOperand(NodeValue = OperandValue.Not, Left = itterate o.Left)
        | OperandValue.And | OperandValue.Nand ->
            let left = itterate o.Left
            let right = itterate o.Right
            upcast new TreeOperand(NodeValue = o.NodeValue, Left = left, Right = right)
        | OperandValue.Or ->
            let left = new TreeOperand(NodeValue = OperandValue.Not, Left = itterate o.Left)
            let right = new TreeOperand(NodeValue = OperandValue.Not, Left = itterate o.Right)
            upcast new TreeOperand(NodeValue = OperandValue.Nand, Left = left, Right = right)
        | OperandValue.Implication ->
            let left = new TreeOperand(NodeValue = OperandValue.Not, Left = itterate o.Left)
            itterate (new TreeOperand(NodeValue = OperandValue.Or, Left = left, Right = itterate o.Right))
        | OperandValue.BiImplication ->
            let leftIt = itterate o.Left
            let rightIt = itterate o.Right
            let left = new TreeOperand(NodeValue = OperandValue.And, 
                Left = new TreeOperand(NodeValue = OperandValue.Not, Left = leftIt), 
                Right = new TreeOperand(NodeValue = OperandValue.Not, Left = rightIt))
            let right = new TreeOperand(NodeValue = OperandValue.And, Left = leftIt, Right = rightIt)
            itterate (new TreeOperand(NodeValue = OperandValue.Or, Left = left, Right = right))
        

let Nandify (input: ITreeNode) =
    itterate input

