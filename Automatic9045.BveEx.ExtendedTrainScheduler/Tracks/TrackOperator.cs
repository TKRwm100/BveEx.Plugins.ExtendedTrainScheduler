using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BveTypes.ClassWrappers;
using SlimDX;
using SlimDX.Direct3D9;

using BveEx.Extensions.MapStatements;

namespace Automatic9045.BveEx.ExtendedTrainScheduler.Tracks
{
    internal class TrackOperator
    {
        private static readonly TrainInfo DummyKey = new TrainInfo();

        private readonly IReadOnlyDictionary<TrainInfo, TrackSchedule> Schedules;

        public TrackOperator(IReadOnlyDictionary<TrainInfo, TrackSchedule> schedules)
        {
            Schedules = schedules;
        }

        public static TrackOperator Create(IEnumerable<Statement> source, IReadOnlyDictionary<string, TrainInfo> trainInfos, Action<string, string, int, int> onError)
        {
            Validator validator = new Validator(onError);

            Dictionary<TrainInfo, TrackSchedule> schedules = source
                .GroupBy(statement =>
                {
                    MapStatement statementSource = statement.Source;

                    validator.CheckClauseLength(statementSource, 6);
                    validator.CheckFirstArgIsNotNull(statementSource, 5);

                    TrainInfo trainInfo = validator.GetTrain(statementSource, trainInfos).Info ?? DummyKey;
                    return trainInfo;
                })
                .ToDictionary(x => x.Key, TrackSchedule.FromStatements);
            schedules.Remove(DummyKey);

            foreach (KeyValuePair<TrainInfo, TrackSchedule> item in schedules)
            {
                item.Value.DefaultValue = item.Key.TrackKey;
            }

            return new TrackOperator(schedules);
        }

        public bool DrawCars(Train train, Matrix view)
        {
            if (!Schedules.TryGetValue(train.TrainInfo, out TrackSchedule schedule)) return false;

            Direct3DProvider direct3DProvicer = Direct3DProvider.Instance;

            int distanceFromVehicle = (int)Math.Floor(train.Location - train.VehicleLocation.Location);
            foreach (Structure car in train.TrainInfo.Structures)
            {
                if (car.Model is null) continue;
                if (distanceFromVehicle + car.Location + car.Span < -train.DrawDistanceManager.BackDrawDistance - 25) continue;
                if (train.DrawDistanceManager.FrontDrawDistance + 25 <= distanceFromVehicle + car.Location) continue;

                Matrix trackMatrix;
                {
                    trackMatrix = car.Matrix;
                    trackMatrix.M41 = 0;
                    trackMatrix.M42 = 0;

                    int blockLocation = train.VehicleLocation.BlockIndex * 25;
                    double bogie1Location = train.Location + car.Location;
                    double bogie2Location = bogie1Location + Math.Max(car.Span, 1);

                    car.TrackKey = schedule.GetValue(bogie1Location);
                    Vector3 bogie1Position = train.Map.GetPosition(car, bogie1Location, blockLocation);

                    car.TrackKey = schedule.GetValue(bogie2Location);
                    Vector3 bogie2Position = train.Map.GetPosition(car, bogie2Location, blockLocation);

                    if (car.TiltsAlongCant && train.Map.OtherTracks.TryGetValue(car.TrackKey, out OtherTrack track))
                    {
                        track.Cants.GoTo(bogie1Location + car.Span / 2);
                        trackMatrix *= track.Cants.Rotation;
                    }
                    if (!car.TiltsAlongGradient) bogie2Position.Y = bogie1Position.Y;

                    trackMatrix *= Matrix.Invert(Matrix.LookAtLH(bogie1Position, bogie2Position, Vector3.UnitY));
                }

                direct3DProvicer.Device.SetTransform(TransformState.World, trackMatrix * view);
                car.Model.Draw(direct3DProvicer, false);
                car.Model.Draw(direct3DProvicer, true);
            }

            return true;
        }
    }
}
