using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BveTypes.ClassWrappers;

using AtsEx.Extensions.MapStatements;

namespace Automatic9045.AtsEx.ExtendedTrainScheduler.Tracks
{
    internal class TrackOperator
    {
        private static readonly string DummyTrainKey = string.Empty;

        private readonly IReadOnlyDictionary<string, TrackSchedule> Schedules;

        public TrackOperator(IReadOnlyDictionary<string, TrackSchedule> schedules)
        {
            Schedules = schedules;
        }

        public static TrackOperator Create(IEnumerable<Statement> source, IReadOnlyDictionary<string, TrainInfo> trainInfos, Action<string, string, int, int> onError)
        {
            Validator validator = new Validator(onError);

            Dictionary<string, TrackSchedule> schedules = source
                .GroupBy(statement =>
                {
                    MapStatement statementSource = statement.Source;

                    validator.CheckClauseLength(statementSource, 6);
                    validator.CheckFirstArgIsNotNull(statementSource, 5);

                    string trainKey = validator.GetTrain(statementSource, trainInfos).Key ?? DummyTrainKey;
                    return trainKey;
                })
                .ToDictionary(x => x.Key, TrackSchedule.FromStatements);
            schedules.Remove(DummyTrainKey);

            foreach (KeyValuePair<string, TrackSchedule> item in schedules)
            {
                item.Value.DefaultValue = trainInfos[item.Key].TrackKey;
            }

            return new TrackOperator(schedules);
        }

        public void Tick(IReadOnlyDictionary<string, Train> trains)
        {
            foreach (KeyValuePair<string, TrackSchedule> item in Schedules)
            {
                Train train = trains[item.Key];

                double location = train.Location;
                string track = item.Value.GetValue(location);

                train.TrainInfo.TrackKey = track;
            }
        }
    }
}
