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
using TypeWrapping;

using AtsEx.Extensions.MapStatements;
using AtsEx.Extensions.PreTrainPatch;
using AtsEx.PluginHost;
using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost.Plugins.Extensions;

using Automatic9045.AtsEx.ExtendedTrainScheduler.PreTrains;
using Automatic9045.AtsEx.ExtendedTrainScheduler.Speed;
using Automatic9045.AtsEx.ExtendedTrainScheduler.Tracks;

namespace Automatic9045.AtsEx.ExtendedTrainScheduler
{
    [Plugin(PluginType.Extension)]
    public class PluginMain : AssemblyPluginBase, IExtension
    {
        private readonly HarmonyPatch HarmonyPatch;

        private bool AreOperatorsInitialized = false;
        private TrackOperator TrackOperator;
        private PreTrainOperator PreTrainOperator;
        private SpeedOperator SpeedOperator;
        private PreTrainPatch PreTrainPatch;

        public PluginMain(PluginBuilder builder) : base(builder)
        {
            ClassMemberSet trainMembers = BveHacker.BveTypes.GetClassInfoOf<Train>();
            FastMethod compileToSchedulesMethod = trainMembers.GetSourceMethodOf(nameof(Train.CompileToSchedules));
            HarmonyPatch = HarmonyPatch.Patch(nameof(ExtendedTrainScheduler), compileToSchedulesMethod.Source, PatchType.Prefix);
            HarmonyPatch.Invoked += (sender, e) =>
            {
                Train instance = Train.FromSource(e.Instance);

                if (!AreOperatorsInitialized)
                {
                    StatementSet statements = StatementSet.Load(Extensions.GetExtension<IStatementSet>());
                    WrappedSortedList<string, TrainInfo> trainInfos = instance.Route.TrainInfos;

                    TrackOperator = TrackOperator.Create(statements.SetTrack, trainInfos, ThrowError);
                    PreTrainOperator = PreTrainOperator.Create(statements.AttachToTrain, statements.Detach, trainInfos, ThrowError);
                    SpeedOperator = SpeedOperator.Create(statements.StopUntil, BveHacker.MapLoader.Route.TrainInfos, ThrowError);
                    SpeedOverrider.Override(statements.AccelerateFromHere, statements.AccelerateToHere, BveHacker.MapLoader.Route.TrainInfos, ThrowError);

                    AreOperatorsInitialized = true;


                    void ThrowError(string message, string senderName, int lineIndex, int charIndex)
                    {
                        BveHacker.LoadErrorManager.Throw(message, senderName ?? Name, lineIndex, charIndex);
                        Application.DoEvents();
                    }
                }

                IEnumerable<TrainSchedule> stopSchedules = SpeedOperator.CompileToSchedules(instance.TrainInfo, ((Station)instance.Route.Stations[0]).DefaultTime);
                foreach (TrainSchedule schedule in stopSchedules)
                {
                    instance.Schedules.Add(schedule);
                }

                return new PatchInvokationResult(SkipModes.SkipPatches | SkipModes.SkipOriginal);
            };

            BveHacker.ScenarioCreated += OnScenarioCreated;
            BveHacker.ScenarioClosed += OnScenarioClosed;
        }

        public override void Dispose()
        {
            HarmonyPatch.Dispose();
            PreTrainPatch?.Dispose();
            BveHacker.ScenarioCreated -= OnScenarioCreated;
            BveHacker.ScenarioClosed -= OnScenarioClosed;
        }

        private void OnScenarioCreated(ScenarioCreatedEventArgs e)
        {
            if (!AreOperatorsInitialized) return;

            PreTrainOperator.SectionManager = e.Scenario.SectionManager;
            PreTrainOperator.Trains = e.Scenario.Trains;

            PreTrainPatch = Extensions.GetExtension<IPreTrainPatchFactory>().Patch(nameof(ExtendedTrainScheduler), e.Scenario.SectionManager, PreTrainOperator);
        }

        private void OnScenarioClosed(EventArgs e)
        {
            AreOperatorsInitialized = false;

            TrackOperator = null;
            PreTrainOperator = null;
        }

        public override TickResult Tick(TimeSpan elapsed)
        {
            if (!AreOperatorsInitialized) return new ExtensionTickResult();

            WrappedSortedList<string, Train> trains = BveHacker.Scenario.Trains;
            TrackOperator.Tick(trains);

            return new ExtensionTickResult();
        }
    }
}
