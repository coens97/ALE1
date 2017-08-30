using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1.Common.TreeNode
{
    public class TreeOperand : ITreeNode
    {
        public OperandValue NodeValue { get; set; }
        public ITreeNode Left { get; set; }
        public ITreeNode Right { get; set; }
    }
}
