using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AtsEx.Extensions.MapStatements;

namespace Automatic9045.AtsEx.ExtendedTrainScheduler.Tracks
{
    internal class TrackSchedule : ScheduleBase<string>
    {
        private TrackSchedule(IReadOnlyList<Node<string>> nodes) : base(nodes)
        {
        }

        public static TrackSchedule FromStatements(IEnumerable<Statement> statements)
        {
            List<Node<string>> nodes = statements
                .Select(statement =>
                {
                    double location = statement.Source.Location;
                    string trackKey = statement.Source.Clauses[5].Args[0].ToString().ToLowerInvariant();

                    return new Node<string>(location, trackKey);
                })
                .ToList();

            return new TrackSchedule(nodes);
        }
    }
}
