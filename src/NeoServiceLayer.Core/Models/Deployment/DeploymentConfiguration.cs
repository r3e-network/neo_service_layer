using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models.Deployment
{
    /// <summary>
    /// Represents the configuration for a deployment
    /// </summary>
    public class DeploymentConfiguration
    {
        /// <summary>
        /// Gets or sets the deployment strategy configuration
        /// </summary>
        public StrategyConfiguration Strategy { get; set; }

        /// <summary>
        /// Gets or sets the traffic routing configuration
        /// </summary>
        public TrafficRoutingConfiguration TrafficRouting { get; set; }

        /// <summary>
        /// Gets or sets the rollback configuration
        /// </summary>
        public RollbackConfiguration Rollback { get; set; }

        /// <summary>
        /// Gets or sets the pre-deployment hooks
        /// </summary>
        public List<Hook> PreDeploymentHooks { get; set; } = new List<Hook>();

        /// <summary>
        /// Gets or sets the post-deployment hooks
        /// </summary>
        public List<Hook> PostDeploymentHooks { get; set; } = new List<Hook>();

        /// <summary>
        /// Gets or sets the deployment timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 600;

        /// <summary>
        /// Gets or sets whether to enable auto-rollback
        /// </summary>
        public bool AutoRollbackEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the scaling configuration
        /// </summary>
        public ScalingConfiguration Scaling { get; set; } = new ScalingConfiguration();

        /// <summary>
        /// Gets or sets the deployment-specific configuration
        /// </summary>
        public Dictionary<string, string> DeploymentSpecificConfig { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents the strategy configuration for a deployment
    /// </summary>
    public class StrategyConfiguration
    {
        /// <summary>
        /// Gets or sets the type of the strategy
        /// </summary>
        public DeploymentStrategy Type { get; set; }

        /// <summary>
        /// Gets or sets the blue-green configuration
        /// </summary>
        public BlueGreenConfiguration BlueGreen { get; set; }

        /// <summary>
        /// Gets or sets the canary configuration
        /// </summary>
        public CanaryConfiguration Canary { get; set; }

        /// <summary>
        /// Gets or sets the rolling configuration
        /// </summary>
        public RollingConfiguration Rolling { get; set; }
    }

    /// <summary>
    /// Represents the blue-green configuration for a deployment
    /// </summary>
    public class BlueGreenConfiguration
    {
        /// <summary>
        /// Gets or sets the pre-traffic hook ARN
        /// </summary>
        public string PreTrafficHookArn { get; set; }

        /// <summary>
        /// Gets or sets the post-traffic hook ARN
        /// </summary>
        public string PostTrafficHookArn { get; set; }

        /// <summary>
        /// Gets or sets the traffic routing configuration
        /// </summary>
        public TrafficRoutingConfiguration TrafficRouting { get; set; }

        /// <summary>
        /// Gets or sets the deployment group name
        /// </summary>
        public string DeploymentGroupName { get; set; }

        /// <summary>
        /// Gets or sets the termination wait time in minutes
        /// </summary>
        public int TerminationWaitTimeInMinutes { get; set; } = 5;
    }

    /// <summary>
    /// Represents the canary configuration for a deployment
    /// </summary>
    public class CanaryConfiguration
    {
        /// <summary>
        /// Gets or sets the canary traffic percentage
        /// </summary>
        public int CanaryTrafficPercentage { get; set; } = 10;

        /// <summary>
        /// Gets or sets the canary interval in minutes
        /// </summary>
        public int CanaryIntervalInMinutes { get; set; } = 15;

        /// <summary>
        /// Gets or sets the traffic steps
        /// </summary>
        public List<TrafficStep> TrafficSteps { get; set; } = new List<TrafficStep>();

        /// <summary>
        /// Gets or sets the pre-traffic hook ARN
        /// </summary>
        public string PreTrafficHookArn { get; set; }

        /// <summary>
        /// Gets or sets the post-traffic hook ARN
        /// </summary>
        public string PostTrafficHookArn { get; set; }

        /// <summary>
        /// Gets or sets the alarm configuration
        /// </summary>
        public AlarmConfiguration Alarms { get; set; }
    }

    /// <summary>
    /// Represents a traffic step for a canary deployment
    /// </summary>
    public class TrafficStep
    {
        /// <summary>
        /// Gets or sets the traffic percentage
        /// </summary>
        public int TrafficPercentage { get; set; }

        /// <summary>
        /// Gets or sets the interval in minutes
        /// </summary>
        public int IntervalInMinutes { get; set; }
    }

    /// <summary>
    /// Represents the rolling configuration for a deployment
    /// </summary>
    public class RollingConfiguration
    {
        /// <summary>
        /// Gets or sets the maximum batch size
        /// </summary>
        public int MaxBatchSize { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum percentage
        /// </summary>
        public int MaxPercentage { get; set; } = 25;

        /// <summary>
        /// Gets or sets the minimum healthy percentage
        /// </summary>
        public int MinHealthyPercentage { get; set; } = 75;

        /// <summary>
        /// Gets or sets the batch interval in seconds
        /// </summary>
        public int BatchIntervalInSeconds { get; set; } = 60;
    }

    /// <summary>
    /// Represents the traffic routing configuration for a deployment
    /// </summary>
    public class TrafficRoutingConfiguration
    {
        /// <summary>
        /// Gets or sets the type of traffic routing
        /// </summary>
        public TrafficRoutingType Type { get; set; }

        /// <summary>
        /// Gets or sets the time-based configuration
        /// </summary>
        public TimeBasedConfiguration TimeBased { get; set; }

        /// <summary>
        /// Gets or sets the linear configuration
        /// </summary>
        public LinearConfiguration Linear { get; set; }

        /// <summary>
        /// Gets or sets the all-at-once configuration
        /// </summary>
        public AllAtOnceConfiguration AllAtOnce { get; set; }
    }

    /// <summary>
    /// Represents the time-based configuration for traffic routing
    /// </summary>
    public class TimeBasedConfiguration
    {
        /// <summary>
        /// Gets or sets the canary interval in minutes
        /// </summary>
        public int CanaryIntervalInMinutes { get; set; } = 15;

        /// <summary>
        /// Gets or sets the canary percentage
        /// </summary>
        public int CanaryPercentage { get; set; } = 10;

        /// <summary>
        /// Gets or sets the bake time in minutes
        /// </summary>
        public int BakeTimeInMinutes { get; set; } = 30;
    }

    /// <summary>
    /// Represents the linear configuration for traffic routing
    /// </summary>
    public class LinearConfiguration
    {
        /// <summary>
        /// Gets or sets the linear percentage
        /// </summary>
        public int LinearPercentage { get; set; } = 10;

        /// <summary>
        /// Gets or sets the linear interval in minutes
        /// </summary>
        public int LinearIntervalInMinutes { get; set; } = 10;
    }

    /// <summary>
    /// Represents the all-at-once configuration for traffic routing
    /// </summary>
    public class AllAtOnceConfiguration
    {
        /// <summary>
        /// Gets or sets the bake time in minutes
        /// </summary>
        public int BakeTimeInMinutes { get; set; } = 0;
    }

    /// <summary>
    /// Represents the rollback configuration for a deployment
    /// </summary>
    public class RollbackConfiguration
    {
        /// <summary>
        /// Gets or sets whether rollback is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the rollback events
        /// </summary>
        public List<RollbackEvent> Events { get; set; } = new List<RollbackEvent>();

        /// <summary>
        /// Gets or sets the alarm configuration
        /// </summary>
        public AlarmConfiguration Alarms { get; set; }
    }

    /// <summary>
    /// Represents a rollback event for a deployment
    /// </summary>
    public enum RollbackEvent
    {
        /// <summary>
        /// Deployment failure event
        /// </summary>
        DeploymentFailure,

        /// <summary>
        /// Deployment stop event
        /// </summary>
        DeploymentStop,

        /// <summary>
        /// Deployment timeout event
        /// </summary>
        DeploymentTimeout,

        /// <summary>
        /// Alarm threshold event
        /// </summary>
        AlarmThreshold
    }

    /// <summary>
    /// Represents the alarm configuration for a deployment
    /// </summary>
    public class AlarmConfiguration
    {
        /// <summary>
        /// Gets or sets whether alarms are enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to ignore poll alarm failure
        /// </summary>
        public bool IgnorePollAlarmFailure { get; set; } = false;

        /// <summary>
        /// Gets or sets the alarms
        /// </summary>
        public List<Alarm> Alarms { get; set; } = new List<Alarm>();
    }

    /// <summary>
    /// Represents an alarm for a deployment
    /// </summary>
    public class Alarm
    {
        /// <summary>
        /// Gets or sets the name of the alarm
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ARN of the alarm
        /// </summary>
        public string Arn { get; set; }
    }

    /// <summary>
    /// Represents a hook for a deployment
    /// </summary>
    public class Hook
    {
        /// <summary>
        /// Gets or sets the name of the hook
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the hook
        /// </summary>
        public HookType Type { get; set; }

        /// <summary>
        /// Gets or sets the ARN of the hook
        /// </summary>
        public string Arn { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the hook configuration
        /// </summary>
        public Dictionary<string, string> Configuration { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents the type of a hook
    /// </summary>
    public enum HookType
    {
        /// <summary>
        /// Lambda hook type
        /// </summary>
        Lambda,

        /// <summary>
        /// HTTP hook type
        /// </summary>
        Http,

        /// <summary>
        /// Script hook type
        /// </summary>
        Script
    }



    /// <summary>
    /// Represents the type of traffic routing
    /// </summary>
    public enum TrafficRoutingType
    {
        /// <summary>
        /// Time-based traffic routing
        /// </summary>
        TimeBased,

        /// <summary>
        /// Linear traffic routing
        /// </summary>
        Linear,

        /// <summary>
        /// All-at-once traffic routing
        /// </summary>
        AllAtOnce
    }

    /// <summary>
    /// Represents the deployment strategy
    /// </summary>
    public enum DeploymentStrategy
    {
        /// <summary>
        /// All-at-once deployment strategy
        /// </summary>
        AllAtOnce,

        /// <summary>
        /// Blue-green deployment strategy
        /// </summary>
        BlueGreen,

        /// <summary>
        /// Canary deployment strategy
        /// </summary>
        Canary,

        /// <summary>
        /// Rolling deployment strategy
        /// </summary>
        Rolling,

        /// <summary>
        /// Immutable deployment strategy
        /// </summary>
        Immutable,

        /// <summary>
        /// Traffic-splitting deployment strategy
        /// </summary>
        TrafficSplitting
    }
}
