using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BveTypes.ClassWrappers;

using BveEx.Extensions.MapStatements;
using BveEx.Extensions.PreTrainPatch;

namespace Automatic9045.BveEx.ExtendedTrainScheduler.PreTrains
{
    internal class PreTrainOperator : IPreTrainLocationConverter
    {
        private readonly PreTrainSchedule Schedule;

        public IReadOnlyDictionary<string, Train> Trains { private get; set; } = null;
        public SectionManager SectionManager { private get; set; } = null;

        private PreTrainOperator(PreTrainSchedule schedule)
        {
            Schedule = schedule;
        }

        public static PreTrainOperator Create(IEnumerable<Statement> attachToTrain, IEnumerable<Statement> detach,
            IReadOnlyDictionary<string, TrainInfo> trainInfos, Action<string, string, int, int> onError)
        {
            List<PreTrainStatement> statements = new List<PreTrainStatement>();
            statements.AddRange(attachToTrain.Select(statement => new PreTrainStatement(statement, StatementType.Attach)));
            statements.AddRange(detach.Select(statement => new PreTrainStatement(statement, StatementType.Detach)));
            statements.Sort();

            Validator validator = new Validator(onError);
            PreTrainSchedule schedule = PreTrainSchedule.FromStatements(statements, trainInfos, validator);

            return new PreTrainOperator(schedule);
        }

        public PreTrainLocation Convert(PreTrainLocation source)
        {
            PreTrain item = Schedule.GetValue(source.Location);
            if (item is null) return source;

            double location = Trains[item.TrainKey].Location + item.Offset;
            return PreTrainLocation.FromLocation(location, SectionManager);
        }
    }
}
