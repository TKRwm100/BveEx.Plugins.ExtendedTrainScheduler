using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automatic9045.BveEx.ExtendedTrainScheduler.PreTrains
{
    internal class PreTrain
    {
        public string TrainKey { get; }
        public double Offset { get; }

        public PreTrain(string trainKey, double offset)
        {
            TrainKey = trainKey;
            Offset = offset;
        }
    }
}
