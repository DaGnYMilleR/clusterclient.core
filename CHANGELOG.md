## 0.1.45 (29-08-2022):

Added `TryGetQueryParameter` request extension.

## 0.1.44 (15-06-2022):

Fixed transformation of null `AsyncClusterProvider`.

## 0.1.43 (09-06-2022):

Added `IAsyncClusterProvider`.

## 0.1.42 (15-04-2022):

Added new logging options to ClusterClient. Now it can log all information about communication with the cluster in one line.

## 0.1.41 (01-03-2022):

Rewrote `WeighedReplicaOrdering`: now uses linear replica selection instead of logarithmic one.
It is approximately 30% faster.

## 0.1.40 (13-01-2022):

Reduced memory traffic and added several optimizations (some .Net 6 only) around weights handling.

## 0.1.39 (06-12-2021):

Added `net6.0` target.

## 0.1.37 (21.09.2021)

Updated `EnvironmentInfo.Application` from `vostok.commons.environment` module.

## 0.1.36 (02.09.2021)

`IContentProducer`: add new mechanic which enables user to produce content into request stream in a custom manner.

## 0.1.35 (17.08.2021)

`RelativeWeightsModifier`: fix bug when RelativeWeightSettings were cached in ClusterState.
`RelativeWeightsModifier`: fix bug when weights changes not logged.
`RelativeWeightsSettings`: add new settings.

## 0.1.34 (04.08.2021)

BREAKING CHANGE. Removed `Dont-Fork` header because another following mechanic was invented. It is expected that no one except drive used the header so it must be safe to remove it.
Added `Unreliable-Response` header name for parallel requests races prevention in `ForkingRequestStrategy` and `ParallelRequestStrategy`.

## 0.1.32 (20.07.2021):

Added validation for a request time budget. Now if budget is greater than int.MaxValue, the IncorrectArguments result will be returned instead of throwing an exception from Task.Delay.

## 0.1.31 (30.06.2021):

Added `VerdictBasedRetryPolicy` which earlier was duplicated in couple different solutions.
Added `Dont-Fork` header name, which in case of not accepted response verdict instructs `ForkingRequestStrategy` not to schedule fork if response has such header.

## 0.1.30 (24.06.2021):

Added `IReplicaFilter` interface and use given realizations from `ClusterClientConfiguration` in execution module for filter given replicas from cluster provider.

## 0.1.29 (04.06.2021):

RelativeWeightModifier - log only significant weights changes.

## 0.1.28 (25.05.2021):

Added `RelativeWeightsModifier`.

## 0.1.27 (11.05.2021):

Use uri kind Relative in case of non-windows and leading slash.

## 0.1.26 (01.03.2021):

Make TimeoutHeaderTransport public to easy use wherever I don't need to create full and high weight ClusterClient.

## 0.1.25 (01.03.2021):

Changed IIS detection in depended project commons.environment to avoid "iisexpress" process name except correct assembly name. Related commit - https://github.com/vostok/commons.environment/commit/5cc91a9ce09e44ce0048e06f37325ff4bb3291fe

## 0.1.24 (04.02.2021):

ClusterClient should not prohibit GET or HEAD requests with message body.

## 0.1.23 (28.01.2021):

Extended `IRequestContext` interface with `ClusterProvider`, `ReplicaOrdering`, and `ConnectionAttempts` properties to enable their customization in request module.

## 0.1.22 (23.11.2020):

Added `IRetryStrategyEx` - an extended interface similar to the old IRetryStrategy but with IRequestContext and last seen ClusterResult.

## 0.1.21 (21.10.2020):

Added AppendToHeaderWithQuality extension to RequestHeadersExtensions.

## 0.1.20 (10.05.2020):

Fixed https://github.com/vostok/clusterclient.core/issues/14

## 0.1.19 (20.04.2020):

Added `Dont-Accept` header and a corresponding rejecting response criterion (used by default).

## 0.1.18 (13.04.2020):

Methods and extensions that add query parameters have received an optional overload to support empty values.

## 0.1.17 (02.04.2020):

Added `SetupExternalUrl` to simplify configuration of clients to external APIs.

## 0.1.16 (21.02.2020)

Added `ConnectionAttemptsTransport` - a transport decorator responsible for retrying connection failures.

## 0.1.15 (27.01.2020)

Implemented https://github.com/vostok/clusterclient.core/issues/9

## 0.1.14 (27.01.2020)

Added configurable `ClusterClientDefaults.ClientApplicationName` property.

## 0.1.12 (18.01.2020)

Slight improvements in error logging.

## 0.1.11 (18.11.2019)

Added `ElapsedTimeMs` log property.

## 0.1.10 (05.10.2019)

ForkingRequestStrategy now adds a `Concurrency-Level` header with current parallelism value to detect forked retries on server side.

## 0.1.9 (15.08.2019)

Potential fix for https://github.com/vostok/clusterclient.core/issues/6

## 0.1.8 (17.07.2019)

WeighedReplicaOrdering now builds its internal segment tree in O(N) time instead of O(N * log(N)).

## 0.1.7 (29.04.2019):

Added support for asynchronous request transforms.

## 0.1.6 (23.04.2019):

Removed redundant trailing 's' from 'Request-Timeout' header value format.

## 0.1.5 (15-03-2019):

Fixed https://github.com/vostok/clusterclient.core/issues/3

## 0.1.4 (03-03-2019): 

Introduced support for request bodies consisting of multiple buffer segments.

## 0.1.3 (16-02-2019): 

Enchancements in auxiliary headers modules.

## 0.1.2 (16-02-2019): 

AddRequestModule() extension is now idempotent.

## 0.1.1 (16-02-2019): 

Minor changes: added context-related header names, changed default request priority to null.

## 0.1.0 (04-02-2019): 

Initial prerelease.
