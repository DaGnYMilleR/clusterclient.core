﻿using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    /// <summary>
    /// <para>Represents a weight modifier that periodically calculates the weight of a replica, which characterizes its quality relative to others in the cluster.</para>
    /// <para>Replica weight is the probability that a replica will respond in a time less than or equal to the average response time across the cluster.</para>
    /// <para>For replicas whose responses are <see cref="ResponseVerdict.Reject"/> or <see cref="ResponseVerdict.DontKnow"/> a <see cref="RelativeWeightSettings.PenaltyMultiplier"/> will be applied.</para>
    /// </summary>
    [PublicAPI]
    public class RelativeWeightModifier : IReplicaWeightModifier
    {
        private readonly ILog log;
        private readonly RelativeWeightSettings settings;
        private readonly double minWeight;
        private readonly double maxWeight;
        private readonly string storageKey;

        public RelativeWeightModifier(
            RelativeWeightSettings settings,
            string service,
            string environment,
            double minWeight = ClusterClientDefaults.MinimumReplicaWeight,
            double maxWeight = ClusterClientDefaults.MaximumReplicaWeight,
            ILog log = null)
        {
            this.settings = settings;
            this.minWeight = minWeight;
            this.maxWeight = maxWeight;
            this.log = (log ?? new SilentLog()).ForContext<RelativeWeightModifier>();

            storageKey = CreateStorageKey(service, environment);
        }

        /// <inheritdoc />
        public void Modify(Uri replica, IList<Uri> allReplicas, IReplicaStorageProvider storageProvider, Request request, RequestParameters parameters, ref double weight)
        {
            var clusterState = storageProvider.ObtainGlobalValue(storageKey, CreateClusterState);
            
            ModifyWeightsIfNeed(clusterState);

            weight = ModifyAndApplyLimits(weight, clusterState.Weights.Get(replica));
        }

        /// <inheritdoc />
        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider) =>
            storageProvider.ObtainGlobalValue(storageKey, CreateClusterState).CurrentStatistic.Report(result);

        private void ModifyWeightsIfNeed(ClusterState clusterState)
        {
            var needUpdateWeights = NeedUpdateWeights(clusterState.TimeProvider.GetCurrentTime(), clusterState.LastUpdateTimestamp);
            var updatingFlagChanged = false;
            try
            {
                if (!needUpdateWeights || !(updatingFlagChanged = clusterState.IsUpdatingNow.TrySetTrue()))
                    return;

                var currentTime = clusterState.TimeProvider.GetCurrentTime();
                clusterState.LastUpdateTimestamp = currentTime;

                var aggregatedClusterStatistic = clusterState
                    .SwapToNewRawStatistic()
                    .GetPenalizedAndSmoothedStatistic(currentTime, clusterState.StatisticHistory.Get());
                
                ModifyWeights(aggregatedClusterStatistic, clusterState.RelativeWeightCalculator, clusterState.Weights, clusterState.LastUpdateTimestamp);

                clusterState.StatisticHistory.Update(aggregatedClusterStatistic);
            }
            finally
            {
                if (updatingFlagChanged)
                    clusterState.IsUpdatingNow.Value = false;
            }
        }

        private void ModifyWeights(
            AggregatedClusterStatistic aggregatedClusterStatistic,
            IRelativeWeightCalculator relativeWeightCalculator,
            IWeights weights,
            DateTime weightsLastUpdateTime)
        {
            var newWeights = new Dictionary<Uri, Weight>(aggregatedClusterStatistic.Replicas.Count);
            var relativeMaxWeight = 0d;
            foreach (var (replica, replicaStatistic) in aggregatedClusterStatistic.Replicas)
            {
                var previousWeight = weights.Get(replica) ?? new Weight(settings.InitialWeight, weightsLastUpdateTime - settings.WeightUpdatePeriod);
                var newReplicaWeight = relativeWeightCalculator.Calculate(aggregatedClusterStatistic.Cluster, replicaStatistic, previousWeight);
                newWeights.Add(replica, newReplicaWeight);

                if (newReplicaWeight.Value > relativeMaxWeight)
                    relativeMaxWeight = newReplicaWeight.Value;
            }
            
            weights.Update(newWeights);
            weights.Normalize();
            
            LogWeights(weights);
        }

        private double ModifyAndApplyLimits(double externalWeight, Weight? relativeWeight)
        {
            // ExternalWeight - [minWeight; maxWeight]
            // RelativeWeight - [0; 1]
            // ModifiedWeight - [0; maxWeight]
            var relative = relativeWeight?.Value ?? settings.InitialWeight;
            var modified = externalWeight * relative;

            return minWeight + modified / maxWeight * (maxWeight - minWeight);
        }

        private bool NeedUpdateWeights(in DateTime currentTimestamp, in DateTime previousTimestamp) =>
            currentTimestamp - previousTimestamp > settings.WeightUpdatePeriod;

        private string CreateStorageKey(string service, string environment) =>
            $"{nameof(RelativeWeightModifier)}_{environment}_{service}";

        private ClusterState CreateClusterState() =>
            new ClusterState(settings);

        private void LogWeights(IEnumerable<KeyValuePair<Uri, Weight>> weights)
        {
            if (!log.IsEnabledForDebug()) return;

            var newWeightsLog = new StringBuilder($"Weights:{Environment.NewLine}");
            foreach (var (replica, weight) in weights)
                newWeightsLog.AppendLine($"{replica}: {weight}");
            log.Debug(newWeightsLog.ToString());
        }
    }
}