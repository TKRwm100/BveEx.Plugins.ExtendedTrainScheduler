﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AtsEx.Extensions.MapStatements;

namespace Automatic9045.AtsEx.ExtendedTrainScheduler
{
    internal class StatementSet
    {
        private static readonly string UserName = "Automatic9045";

        private static readonly ClauseFilter Root = ClauseFilter.Element(nameof(ExtendedTrainScheduler), 0);
        private static readonly ClauseFilter Train = ClauseFilter.Element("Train", 1);
        private static readonly ClauseFilter Accelerate = ClauseFilter.Element("Accelerate", 0);
        private static readonly ClauseFilter PreTrain = ClauseFilter.Element("PreTrain", 0);

        public IEnumerable<Statement> SetTrack { get; }
        public IEnumerable<Statement> AccelerateFromHere { get; }
        public IEnumerable<Statement> AccelerateToHere { get; }
        public IEnumerable<Statement> StopUntil { get; }
        public IEnumerable<Statement> AttachToTrain { get; }
        public IEnumerable<Statement> Detach { get; }

        private StatementSet(IEnumerable<Statement> setTrack,
            IEnumerable<Statement> accelerateFromHere, IEnumerable<Statement> accelerateToHere, IEnumerable<Statement> stopUntil,
            IEnumerable<Statement> attachToTrain, IEnumerable<Statement> detach)
        {
            SetTrack = setTrack;
            AccelerateFromHere = accelerateFromHere;
            AccelerateToHere = accelerateToHere;
            StopUntil = stopUntil;
            AttachToTrain = attachToTrain;
            Detach = detach;
        }

        public static StatementSet Load(IStatementSet source)
        {
            IEnumerable<Statement> setTrack = source.FindUserStatements(UserName, Root, Train, ClauseFilter.Function("SetTrack", 1));
            IEnumerable<Statement> accelerateFromHere = source.FindUserStatements(UserName, Root, Train, Accelerate, ClauseFilter.Function("FromHere", 2));
            IEnumerable<Statement> accelerateToHere = source.FindUserStatements(UserName, Root, Train, Accelerate, ClauseFilter.Function("ToHere", 2));
            IEnumerable<Statement> stopUntil = source.FindUserStatements(UserName, Root, Train, ClauseFilter.Function("StopUntil", 4));
            IEnumerable<Statement> attachToTrain = source.FindUserStatements(UserName, Root, PreTrain, ClauseFilter.Function("AttachToTrain", 1));
            IEnumerable<Statement> detach = source.FindUserStatements(UserName, Root, PreTrain, ClauseFilter.Function("Detach", 0));

            return new StatementSet(setTrack, accelerateFromHere, accelerateToHere, stopUntil, attachToTrain, detach);
        }
    }
}
