using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BveTypes.ClassWrappers;

using BveEx.Extensions.MapStatements;

namespace Automatic9045.BveEx.ExtendedTrainScheduler
{
    internal class Validator
    {
        private readonly Action<string, string, int, int> OnError;

        public Validator(Action<string, string, int, int> onError)
        {
            OnError = onError;
        }

        public void CheckClauseLength(MapStatement statement, int length)
        {
            if (statement.Clauses.Count != length)
            {
                ThrowError(SyntaxException.DefaultMessage, statement);
            }
        }

        public void CheckFirstArgIsNotNull(MapStatement statement, int clauseIndex)
        {
            if (statement.Clauses[clauseIndex].Args[0] is null)
            {
                ThrowError(SyntaxException.DefaultMessage, statement);
            }
        }

        public (string Key, TrainInfo Info) GetTrain(string trainKey, MapStatement statement, IReadOnlyDictionary<string, TrainInfo> trainInfos)
        {
            string formattedTrainKey = trainKey.ToLowerInvariant();

            if (!trainInfos.TryGetValue(formattedTrainKey, out TrainInfo trainInfo))
            {
                ThrowError($"キー '{formattedTrainKey}' の他列車は存在しません。", statement);
                return (null, null);
            }

            return (formattedTrainKey, trainInfo);
        }

        public (string Key, TrainInfo Info) GetTrain(MapStatement statement, IReadOnlyDictionary<string, TrainInfo> trainInfos)
        {
            string trainKey = statement.Clauses[4].Keys[0].ToString();
            return GetTrain(trainKey, statement, trainInfos);
        }

        public void ThrowError(string message, MapStatement source)
        {
            OnError(message, source.FileName, source.Clauses[0].LineIndex, source.Clauses[0].CharIndex);
        }
    }
}
