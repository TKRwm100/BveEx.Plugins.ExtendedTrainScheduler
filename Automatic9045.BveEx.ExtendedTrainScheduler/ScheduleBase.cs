using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automatic9045.BveEx.ExtendedTrainScheduler
{
    internal abstract class ScheduleBase<T>
    {
        protected readonly IReadOnlyList<Node<T>> Nodes;

        private int OldIndex = 0;

        public T DefaultValue { get; set; } = default;

        protected ScheduleBase(IReadOnlyList<Node<T>> nodes)
        {
            Nodes = nodes;
        }

        public T GetValue(double location)
        {
            if (Nodes.Count == 0) return default;

            double oldLocation = Nodes[OldIndex].Location;

            if (location == oldLocation)
            {
                return Nodes[OldIndex].Value;
            }
            else if (location < oldLocation)
            {
                for (int i = OldIndex - 1; 0 <= i; i--)
                {
                    if (Nodes[i].Location <= location)
                    {
                        OldIndex = i;
                        return Nodes[i].Value;
                    }
                }

                OldIndex = 0;
                return DefaultValue;
            }
            else
            {
                for (int i = OldIndex + 1; i < Nodes.Count; i++)
                {
                    if (location < Nodes[i].Location)
                    {
                        OldIndex = i - 1;
                        return Nodes[i - 1].Value;
                    }
                }

                OldIndex = Nodes.Count - 1;
                return Nodes[Nodes.Count - 1].Value;
            }
        }
    }
}
