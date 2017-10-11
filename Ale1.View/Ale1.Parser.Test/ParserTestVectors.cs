using Ale1.Common.TreeNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1.Functional.Test
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
                // Basic AND operand with 1
                new Tuple<string, ITreeNode>(
                    "&(1,b)",
                    new TreeOperand
                    {
                        NodeValue = OperandValue.And,
                        Left = new TreeValue
                        {
                            Value = true
                        },
                        Right = new TreeVariable
                        {
                            Name = "b"
                        }
                    }),
                // Basic AND operand with 0
                new Tuple<string, ITreeNode>(
                    "&(0,b)",
                    new TreeOperand
                    {
                        NodeValue = OperandValue.And,
                        Left = new TreeValue
                        {
                            Value = false
                        },
                        Right = new TreeVariable
                        {
                            Name = "b"
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

        // With these test vectors the input text is not the same the text constructed from the tree
        public static Tuple<string, ITreeNode>[] GetSpecialVectors =>
            new[]
            {
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
            };

        public static string[] GetSyntaxErrorVectors =>
            new[]
            {
                "&(a)b)",
                "~(a,b)",
                "|(a)",
                "&,a,b)",
                "&(a,b,",
                "&(a,~(b)))"
            };
    }
}
