module Ale1.Functional.NandifyTree

open Ale1.Common.TreeNode

let rec private itterate (input: ITreeNode) =
    match input with
    | :? TreeVariable as v -> input // When node match with a variable only return that
    | :? TreeOperand as o->
        match o.NodeValue with // The NOT operand is the only without a right node
        | OperandValue.Not ->
            let leaf = itterate o.Left
            upcast new TreeOperand(NodeValue = OperandValue.Nand, Left = leaf, Right = leaf)
        | OperandValue.Nand ->
            let left = itterate o.Left
            let right = itterate o.Right
            upcast new TreeOperand(NodeValue = OperandValue.Nand, Left = left, Right = right)
        | OperandValue.And ->
            let prop = new TreeOperand(NodeValue = OperandValue.Nand, Left = itterate o.Left, Right = itterate o.Right)
            itterate (new TreeOperand(NodeValue = OperandValue.Not, Left = prop))
        | OperandValue.Or ->
            let left = itterate (new TreeOperand(NodeValue = OperandValue.Not, Left = itterate o.Left))
            let right = itterate (new TreeOperand(NodeValue = OperandValue.Not, Left = itterate o.Right))
            upcast new TreeOperand(NodeValue = OperandValue.Nand, Left = left, Right = right)
        | OperandValue.Implication ->
            let left = itterate (new TreeOperand(NodeValue = OperandValue.Not, Left = itterate o.Left))
            itterate (new TreeOperand(NodeValue = OperandValue.Or, Left = left, Right = itterate o.Right))
        | OperandValue.BiImplication ->
            let leftIt = itterate o.Left
            let rightIt = itterate o.Right
            let left = itterate(new TreeOperand(NodeValue = OperandValue.And, 
                    Left = itterate (new TreeOperand(NodeValue = OperandValue.Not, Left = leftIt)), 
                    Right = itterate (new TreeOperand(NodeValue = OperandValue.Not, Left = rightIt))))
            let right = itterate (new TreeOperand(NodeValue = OperandValue.And, Left = leftIt, Right = rightIt))
            itterate (new TreeOperand(NodeValue = OperandValue.Or, Left = left, Right = right))
        

let Nandify (input: ITreeNode) =
    itterate input

