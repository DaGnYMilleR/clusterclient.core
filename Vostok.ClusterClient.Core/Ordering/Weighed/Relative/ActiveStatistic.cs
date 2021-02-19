﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class ActiveStatistic
    {
        private readonly TimeSpan smoothingConstant;
        private readonly int penaltyMultiplier;

        private readonly StatisticBucket clusterStatistic;
        private readonly ConcurrentDictionary<Uri, StatisticBucket> replicasStatistic;

        public ActiveStatistic(TimeSpan smoothingConstant, int penaltyMultiplier)
        {
            this.smoothingConstant = smoothingConstant;
            this.penaltyMultiplier = penaltyMultiplier;

            clusterStatistic = new StatisticBucket();
            replicasStatistic = new ConcurrentDictionary<Uri, StatisticBucket>();
        }

        public void Report(ReplicaResult result)
        { 
            clusterStatistic
                .Report(result);
            replicasStatistic
                .GetOrAdd(result.Replica, new StatisticBucket())
                .Report(result);
        }

        public double CalculatePenalty()
        {
            var globalStat = clusterStatistic.Observe(DateTime.UtcNow);
            return globalStat.Mean + globalStat.StdDev * penaltyMultiplier;
        }

        public Statistic ObserveCluster(DateTime currentTime, double penalty, Statistic? previous)
        {
            var smoothed = clusterStatistic
                .Penalize(penalty)
                .ObserveSmoothed(currentTime, smoothingConstant, previous);
            return smoothed;
        }

        public IEnumerable<(Uri Replica, Statistic Statistic)> ObserveReplicas(
            DateTime currentTime, double penalty, Func<Uri, Statistic?> statisticProvider)
        {
            return from replicaStatistic in replicasStatistic 
                   let smoothed = replicaStatistic.Value
                       .Penalize(penalty)
                       .ObserveSmoothed(currentTime, smoothingConstant, statisticProvider(replicaStatistic.Key)) 
                   select (replicaStatistic.Key, smoothed);
        }
    }
}