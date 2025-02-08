using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BveEx.Extensions.MapStatements;

namespace Automatic9045.BveEx.ExtendedTrainScheduler.PreTrains
{
    internal struct PreTrainStatement : IComparable<PreTrainStatement>
    {
        public Statement Statement { get; }
        public StatementType Type { get; }

        public PreTrainStatement(Statement statement, StatementType type)
        {
            Statement = statement;
            Type = type;
        }

        public int CompareTo(PreTrainStatement other)
        {
            return Math.Sign(Statement.Source.Location - other.Statement.Source.Location);
        }
    }

    internal enum StatementType
    {
        Attach,
        Detach,
    }
}
