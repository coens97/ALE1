﻿using Ale1.Common.TreeNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1.Parser.Test
{
    class ParserTestVectors
    {
        public static Tuple<string, ITreeNode>[] GetVectors =>
            new[]
            {
                // Basic AND operand
                new Tuple<string, ITreeNode>(
                    "&(a,b)",
                    new TreeOperand
                    {
                        NodeValue = OperandValue.And,
                        Left = new TreeVariable
                        {
                            Name = "a"
                        },
                        Right = new TreeVariable
                        {
                            Name = "b"
                        }
                    }),
                // Basic OR operand with many spaces
                new Tuple<string, ITreeNode>(
                    "   |    (   a  ,   b   )    ",
                    new TreeOperand
                    {
                        NodeValue = OperandValue.Or,
                        Left = new TreeVariable
                        {
                            Name = "a"
                        },
                        Right = new TreeVariable
                        {
                            Name = "b"
                        }
                    }),
                // Basic NOT operand
                new Tuple<string, ITreeNode>(
                    "~(a)",
                    new TreeOperand
                    {
                        NodeValue = OperandValue.Not,
                        Left = new TreeVariable
                        {
                            Name = "a"
                        }
                    }),
                // Combination of all operands
                new Tuple<string, ITreeNode>(
                    "~(>(=(|(a,b),c),&(d,e)))",
                    new TreeOperand
                    {
                        NodeValue = OperandValue.Not,
                        Left = new TreeOperand
                        {
                            NodeValue = OperandValue.Implication,
                            Left = new TreeOperand
                            {
                                NodeValue = OperandValue.BiImplication,
                                Left = new TreeOperand
                                {
                                    NodeValue = OperandValue.Or,
                                    Left = new TreeVariable
                                    {
                                        Name = "a"
                                    },
                                    Right = new TreeVariable
                                    {
                                        Name = "b"
                                    }
                                },
                                Right = new TreeVariable
                                {
                                    Name = "c"
                                }
                            },
                            Right = new TreeOperand
                            {
                                NodeValue = OperandValue.And,
                                Left = new TreeVariable
                                {
                                    Name = "d"
                                },
                                Right = new TreeVariable
                                {
                                    Name = "e"
                                }
                            }
                        }
                    })
            };
    }
}