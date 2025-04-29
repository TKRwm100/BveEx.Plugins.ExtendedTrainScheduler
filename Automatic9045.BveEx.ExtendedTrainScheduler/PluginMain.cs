using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using BveTypes.ClassWrappers;
using BveTypes.ClassWrappers.Extensions;
using FastMember;
using ObjectiveHarmonyPatch;
using SlimDX;
using TypeWrapping;

using BveEx.Extensions.MapStatements;
using BveEx.Extensions.PreTrainPatch;
using BveEx.PluginHost;
using BveEx.PluginHost.Plugins;

using Automatic9045.BveEx.ExtendedTrainScheduler.PreTrains;
using Automatic9045.BveEx.ExtendedTrainScheduler.Speed;
using Automatic9045.BveEx.ExtendedTrainScheduler.Tracks;

namespace Automatic9045.BveEx.ExtendedTrainScheduler
{
    [Plugin(PluginType.MapPlugin)]
    public class PluginMain : AssemblyPluginBase
    {
        private readonly HarmonyPatch CompileToSchedulesPatch;
        private readonly HarmonyPatch DrawCarsPatch;

        private bool AreOperatorsInitialized = false;
        private TrackOperator TrackOperator;
        private PreTrainOperator PreTrainOperator;
        private SpeedOperator SpeedOperator;
        private PreTrainPatch PreTrainPatch;

        public PluginMain(PluginBuilder builder) : base(builder)
        {
            ClassMemberSet trainMembers = BveHacker.BveTypes.GetClassInfoOf<Train>();

            FastMethod compileToSchedulesMethod = trainMembers.GetSourceMethodOf(nameof(Train.CompileToSchedules));
            CompileToSchedulesPatch = HarmonyPatch.Patch(nameof(ExtendedTrainScheduler), compileToSchedulesMethod.Source, PatchType.Prefix);
            CompileToSchedulesPatch.Invoked += (sender, e) =>
            {
                Train instance = Train.FromSource(e.Instance);

                if (!AreOperatorsInitialized)
                {
                    StatementSet statements = StatementSet.Load(Extensions.GetExtension<IStatementSet>());
                    WrappedSortedList<string, TrainInfo> trainInfos = instance.Map.TrainInfos;

                    TrackOperator = TrackOperator.Create(statements.SetTrack, trainInfos, ThrowError);
                    PreTrainOperator = PreTrainOperator.Create(statements.AttachToTrain, statements.Detach, trainInfos, ThrowError);
                    SpeedOperator = SpeedOperator.Create(statements.StopUntil,statements.StopAtUntil, statements.StopAt, statements.AccelerateToHereAt,BveHacker.MapLoader.Map.TrainInfos, ThrowError);
                    SpeedOverrider.Override(statements.AccelerateFromHere, statements.AccelerateToHere, BveHacker.MapLoader.Map.TrainInfos, ThrowError, ((Station)instance.Map.Stations.FirstOrDefault())?.DefaultTime??new TimeSpan());

                    AreOperatorsInitialized = true;


                    void ThrowError(string message, string senderName, int lineIndex, int charIndex)
                    {
                        BveHacker.LoadingProgressForm.ThrowError(message, senderName ?? Name, lineIndex, charIndex);
                        Application.DoEvents();
                    }
                }

                TimeSpan originTime = 0 < instance.Map.Stations.Count ? ((Station)instance.Map.Stations[0]).DefaultTime : TimeSpan.Zero;
                IEnumerable<TrainSchedule> stopSchedules = SpeedOperator.CompileToSchedules(instance.TrainInfo, originTime);
                foreach (TrainSchedule schedule in stopSchedules)
                {
                    instance.Schedules.Add(schedule);
                }

                return new PatchInvokationResult(SkipModes.SkipPatches | SkipModes.SkipOriginal);
            };

            FastMethod drawCarsMethod = trainMembers.GetSourceMethodOf(nameof(Train.DrawCars));
            DrawCarsPatch = HarmonyPatch.Patch(nameof(ExtendedTrainScheduler), drawCarsMethod.Source, PatchType.Prefix);
            DrawCarsPatch.Invoked += (sender, e) =>
            {
                Train instance = Train.FromSource(e.Instance);
                Direct3DProvider direct3DProvider = Direct3DProvider.FromSource(e.Args[0]);
                Matrix view = (Matrix)e.Args[1];

                bool overrode = TrackOperator.DrawCars(instance, view);
                return overrode ? new PatchInvokationResult(SkipModes.SkipOriginal) : PatchInvokationResult.DoNothing(e);
            };

            BveHacker.ScenarioCreated += OnScenarioCreated;
        }

        public override void Dispose()
        {
            CompileToSchedulesPatch.Dispose();
            DrawCarsPatch.Dispose();

            PreTrainPatch?.Dispose();

            BveHacker.ScenarioCreated -= OnScenarioCreated;
        }

        private void OnScenarioCreated(ScenarioCreatedEventArgs e)
        {
            PreTrainOperator.SectionManager = e.Scenario.SectionManager;
            PreTrainOperator.Trains = e.Scenario.Trains;

            PreTrainPatch = Extensions.GetExtension<IPreTrainPatchFactory>().Patch(nameof(ExtendedTrainScheduler), e.Scenario.SectionManager, PreTrainOperator);
        }

        public override void Tick(TimeSpan elapsed)
        {
        }
    }
}
