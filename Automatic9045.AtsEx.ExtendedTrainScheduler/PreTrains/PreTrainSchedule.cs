using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BveTypes.ClassWrappers;

using AtsEx.Extensions.MapStatements;

namespace Automatic9045.AtsEx.ExtendedTrainScheduler.PreTrains
{
    internal class PreTrainSchedule : ScheduleBase<string>
    {
        private PreTrainSchedule(IReadOnlyList<Node<string>> nodes) : base(nodes)
        {
        }

        public static PreTrainSchedule FromStatements(IEnumerable<PreTrainStatement> statements, IReadOnlyDictionary<string, TrainInfo> trainInfos, Validator validator)
        {
            List<Node<string>> nodes = statements
                .Select(statement =>
                {
                    MapStatement statementSource = statement.Statement.Source;

                    validator.CheckClauseLength(statementSource, 6);
                    double location = statementSource.Location;

                    switch (statement.Type)
                    {
                        case StatementType.Attach:
                            validator.CheckFirstArgIsNotNull(statementSource, 5);
                            string trainKey = validator.GetTrain(statementSource.Clauses[5].Args[0].ToString(), statementSource, trainInfos).Key;
                            return new Node<string>(location, trainKey);

                        case StatementType.Detach:
                            return new Node<string>(location, null);

                        default:
                            throw new NotSupportedException();
                    }
                })
                .ToList();

            return new PreTrainSchedule(nodes);
        }
    }
}
