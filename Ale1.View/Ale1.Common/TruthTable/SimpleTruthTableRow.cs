using Microsoft.FSharp.Core;

namespace Ale1.Common.TruthTable
{
    public class SimpleTruthTableRow
    {
        public FSharpOption<bool>[] Variables { get; set; }
        public bool Result { get; set; }
    }
}
