using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BveTypes.ClassWrappers;

using AtsEx.Extensions.MapStatements;

namespace Automatic9045.AtsEx.ExtendedTrainScheduler.Speed
{
    internal class SpeedOperator
    {
        private readonly IReadOnlyDictionary<TrainStopObject, TimeSpan> DepartureTimes;
        private readonly Action<string, string, int, int> OnError;

        public SpeedOperator(IReadOnlyDictionary<TrainStopObject, TimeSpan> departureTimes, Action<string, string, int, int> onError)
        {
            DepartureTimes = departureTimes;
            OnError = onError;
        }

        public static SpeedOperator Create(IEnumerable<Statement> stopUntil,
            IReadOnlyDictionary<string, TrainInfo> trainInfos, Action<string, string, int, int> onError)
        {
            Validator validator = new Validator(onError);

            Dictionary<TrainStopObject, TimeSpan> departureTimes = new Dictionary<TrainStopObject, TimeSpan>();
            foreach (Statement statement in stopUntil)
            {
                MapStatement statementSource = statement.Source;

                validator.CheckClauseLength(statementSource, 6);

                TrainInfo trainInfo = validator.GetTrain(statementSource, trainInfos).Info;
                if (!(trainInfo is null))
                {
                    try
                    {
                        double deceleration = Convert.ToDouble(statementSource.Clauses[5].Args[0]) / 3.6;
                        TimeSpan departureTime = CompatibleTimeSpanFactory.Parse(statementSource.Clauses[5].Args[1]);
                        double acceleration = Convert.ToDouble(statementSource.Clauses[5].Args[2]) / 3.6;
                        double speed = Convert.ToDouble(statementSource.Clauses[5].Args[3]) / 3.6;

                        TrainStopObject stop = new TrainStopObject(statementSource.Location, deceleration, 1, acceleration, speed);
                        trainInfo.Insert(stop);
                        departureTimes[stop] = departureTime;
                    }
                    catch (SyntaxException ex)
                    {
                        validator.ThrowError(ex.Message, statementSource);
                    }
                }
            }

            return new SpeedOperator(departureTimes, onError);
        }

        public IEnumerable<TrainSchedule> CompileToSchedules(TrainInfo trainInfo, TimeSpan originTime)
        {
            if (trainInfo.Count == 0) yield break;

            TimeSpan time = TimeSpan.Zero;
            for (int i = 0; i < trainInfo.Count - 1; i++)
            {
                int j = trainInfo.Direction < 0 ? trainInfo.Count - 1 - i : i;
                TrainStopObject prev = (TrainStopObject)trainInfo[j];
                TrainStopObject next = (TrainStopObject)trainInfo[j + trainInfo.Direction];

                if (TimeSpan.Zero < prev.StopTime || i == 0)
                {
                    yield return new TrainSchedule(time, TimeSpan.Zero, prev.Location, 0, 0);
                    time += prev.StopTime;

                    if (DepartureTimes.TryGetValue(prev, out TimeSpan departureTime))
                    {
                        if (trainInfo.EnableLocation != 0 || trainInfo.EnableTime != TimeSpan.Zero)
                        {
                            OnError("Train[trainKey].StopUntil ステートメントは Train[trainKey].Enable ステートメントと併用できません。", null, 0, 0);
                        }
                        else
                        {
                            if (time < departureTime) time = departureTime - originTime;
                        }
                    }
                }

                double prevAccelerationInv = 0 < prev.Acceleration ? 1 / prev.Acceleration : 0;
                double nextDecelerationInv = 0 < next.Deceleration ? 1 / next.Deceleration : 0;

                double finishAccelerateLocation = prev.Location + prev.Speed * prev.Speed * prevAccelerationInv / 2 * trainInfo.Direction;
                double beginDecelerateLocation = next.Location - prev.Speed * prev.Speed * nextDecelerationInv / 2 * trainInfo.Direction;

                bool needCoasting = finishAccelerateLocation * trainInfo.Direction < beginDecelerateLocation * trainInfo.Direction;
                if (needCoasting)
                {
                    if (prevAccelerationInv != 0)
                    {
                        TimeSpan accelerateDuration = CompatibleTimeSpanFactory.FromSeconds(prev.Speed * prevAccelerationInv);
                        yield return new TrainSchedule(time, time, prev.Location, 0, prev.Acceleration * trainInfo.Direction);
                        time += accelerateDuration;
                    }

                    TimeSpan coastDuration = CompatibleTimeSpanFactory.FromSeconds((beginDecelerateLocation - finishAccelerateLocation) / prev.Speed * trainInfo.Direction);
                    yield return new TrainSchedule(time, time, finishAccelerateLocation, prev.Speed * trainInfo.Direction, 0);
                    time += coastDuration;

                    if (nextDecelerationInv != 0)
                    {
                        TimeSpan decelerateDuration = CompatibleTimeSpanFactory.FromSeconds(prev.Speed * nextDecelerationInv);
                        yield return new TrainSchedule(time, time + decelerateDuration, next.Location, 0, -next.Deceleration * trainInfo.Direction);
                        time += decelerateDuration;
                    }
                }
                else
                {
                    float maxSpeed = (float)Math.Sqrt(2 * trainInfo.Direction * (next.Location - prev.Location) / (prevAccelerationInv + nextDecelerationInv));

                    TimeSpan accelerateDuration = CompatibleTimeSpanFactory.FromSeconds(maxSpeed * prevAccelerationInv);
                    yield return new TrainSchedule(time, time, prev.Location, 0, prev.Acceleration * trainInfo.Direction);
                    time += accelerateDuration;

                    TimeSpan decelerateDuration = CompatibleTimeSpanFactory.FromSeconds(maxSpeed * nextDecelerationInv);
                    yield return new TrainSchedule(time, time + decelerateDuration, next.Location, 0, -next.Deceleration * trainInfo.Direction);
                    time += decelerateDuration;
                }
            }

            TrainStopObject lastStop = (TrainStopObject)trainInfo[trainInfo.Direction < 0 ? 0 : trainInfo.Count - 1];
            yield return new TrainSchedule(time, TimeSpan.Zero, lastStop.Location, 0, 0);
        }
    }
}
