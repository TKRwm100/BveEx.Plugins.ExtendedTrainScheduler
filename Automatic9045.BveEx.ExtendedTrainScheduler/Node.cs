using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automatic9045.BveEx.ExtendedTrainScheduler
{
    internal class Node<T>
    {
        public double Location { get; }
        public T Value { get; }

        public Node(double location, T value)
        {
            Location = location;
            Value = value;
        }
    }
}
