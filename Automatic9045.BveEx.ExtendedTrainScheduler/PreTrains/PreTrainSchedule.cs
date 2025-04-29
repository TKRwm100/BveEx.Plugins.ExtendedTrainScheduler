using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BveTypes.ClassWrappers;

using BveEx.Extensions.MapStatements;

namespace Automatic9045.BveEx.ExtendedTrainScheduler.PreTrains
{
    internal class PreTrainSchedule : ScheduleBase<PreTrain>
    {
        private PreTrainSchedule(IReadOnlyList<Node<PreTrain>> nodes) : base(nodes)
        {
        }

        public static PreTrainSchedule FromStatements(IEnumerable<PreTrainStatement> statements, IReadOnlyDictionary<string, TrainInfo> trainInfos, Validator validator)
        {
            List<Node<PreTrain>> nodes = statements
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
                            double offset = 2 <= statementSource.Clauses[5].Args.Count ? Convert.ToDouble(statementSource.Clauses[5].Args[1]) : 0;
                            PreTrain item = new PreTrain(trainKey, offset);
                            return new Node<PreTrain>(location, item);

                        case StatementType.Detach:
                            return new Node<PreTrain>(location, null);

                        default:
                            throw new NotSupportedException();
                    }
                })
                .ToList();

            return new PreTrainSchedule(nodes);
        }
    }
}
