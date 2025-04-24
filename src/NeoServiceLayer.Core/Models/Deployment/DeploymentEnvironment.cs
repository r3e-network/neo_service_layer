using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models.Deployment
{
    /// <summary>
    /// Represents a deployment environment in the system
    /// </summary>
    public class DeploymentEnvironment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the environment
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the environment
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the environment
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the account ID that owns this environment
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the user ID that created this environment
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the type of the environment
        /// </summary>
        public EnvironmentType Type { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the environment
        /// </summary>
        public EnvironmentConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the status of the environment
        /// </summary>
        public EnvironmentStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the environment
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the list of deployments in this environment
        /// </summary>
        public List<Guid> DeploymentIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the environment variables
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the secrets for the environment
        /// </summary>
        public List<EnvironmentSecret> Secrets { get; set; } = new List<EnvironmentSecret>();

        /// <summary>
        /// Gets or sets the network configuration
        /// </summary>
        public NetworkConfiguration Network { get; set; }

        /// <summary>
        /// Gets or sets the scaling configuration
        /// </summary>
        public ScalingConfiguration Scaling { get; set; }

        /// <summary>
        /// Gets or sets the security configuration
        /// </summary>
        public SecurityConfiguration Security { get; set; }
    }

    /// <summary>
    /// Represents a secret for an environment
    /// </summary>
    public class EnvironmentSecret
    {
        /// <summary>
        /// Gets or sets the name of the secret
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the secret reference
        /// </summary>
        public string SecretReference { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents the network configuration for an environment
    /// </summary>
    public class NetworkConfiguration
    {
        /// <summary>
        /// Gets or sets the VPC ID
        /// </summary>
        public string VpcId { get; set; }

        /// <summary>
        /// Gets or sets the subnet IDs
        /// </summary>
        public List<string> SubnetIds { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the security group IDs
        /// </summary>
        public List<string> SecurityGroupIds { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets whether public IP is enabled
        /// </summary>
        public bool PublicIpEnabled { get; set; }

        /// <summary>
        /// Gets or sets the load balancer configuration
        /// </summary>
        public LoadBalancerConfiguration LoadBalancer { get; set; }
    }

    /// <summary>
    /// Represents the load balancer configuration for an environment
    /// </summary>
    public class LoadBalancerConfiguration
    {
        /// <summary>
        /// Gets or sets whether the load balancer is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the load balancer type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the load balancer ARN
        /// </summary>
        public string Arn { get; set; }

        /// <summary>
        /// Gets or sets the target group ARN
        /// </summary>
        public string TargetGroupArn { get; set; }

        /// <summary>
        /// Gets or sets the listener ARN
        /// </summary>
        public string ListenerArn { get; set; }

        /// <summary>
        /// Gets or sets the DNS name
        /// </summary>
        public string DnsName { get; set; }

        /// <summary>
        /// Gets or sets the health check configuration
        /// </summary>
        public HealthCheckConfiguration HealthCheck { get; set; }
    }

    /// <summary>
    /// Represents the health check configuration for a load balancer
    /// </summary>
    public class HealthCheckConfiguration
    {
        /// <summary>
        /// Gets or sets the health check path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the health check port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the health check protocol
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// Gets or sets the health check interval in seconds
        /// </summary>
        public int IntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets the health check timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the healthy threshold
        /// </summary>
        public int HealthyThreshold { get; set; }

        /// <summary>
        /// Gets or sets the unhealthy threshold
        /// </summary>
        public int UnhealthyThreshold { get; set; }
    }

    /// <summary>
    /// Represents the scaling configuration for an environment
    /// </summary>
    public class ScalingConfiguration
    {
        /// <summary>
        /// Gets or sets the minimum number of instances
        /// </summary>
        public int MinInstances { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of instances
        /// </summary>
        public int MaxInstances { get; set; }

        /// <summary>
        /// Gets or sets the desired number of instances
        /// </summary>
        public int DesiredInstances { get; set; }

        /// <summary>
        /// Gets or sets the CPU utilization target percentage
        /// </summary>
        public int CpuUtilizationTargetPercentage { get; set; }

        /// <summary>
        /// Gets or sets the memory utilization target percentage
        /// </summary>
        public int MemoryUtilizationTargetPercentage { get; set; }

        /// <summary>
        /// Gets or sets the scaling cooldown period in seconds
        /// </summary>
        public int CooldownSeconds { get; set; }
    }

    /// <summary>
    /// Represents the security configuration for an environment
    /// </summary>
    public class SecurityConfiguration
    {
        /// <summary>
        /// Gets or sets whether IAM roles are enabled
        /// </summary>
        public bool IamRolesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the IAM role ARN
        /// </summary>
        public string IamRoleArn { get; set; }

        /// <summary>
        /// Gets or sets whether KMS encryption is enabled
        /// </summary>
        public bool KmsEncryptionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the KMS key ARN
        /// </summary>
        public string KmsKeyArn { get; set; }

        /// <summary>
        /// Gets or sets whether VPC endpoints are enabled
        /// </summary>
        public bool VpcEndpointsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the VPC endpoint IDs
        /// </summary>
        public List<string> VpcEndpointIds { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents the configuration for an environment
    /// </summary>
    public class EnvironmentConfiguration
    {
        /// <summary>
        /// Gets or sets the runtime for the environment
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// Gets or sets the memory size in MB
        /// </summary>
        public int MemorySizeMb { get; set; }

        /// <summary>
        /// Gets or sets the CPU size in vCPUs
        /// </summary>
        public double CpuSize { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the storage size in GB
        /// </summary>
        public int StorageSizeGb { get; set; }

        /// <summary>
        /// Gets or sets whether the environment is in a VPC
        /// </summary>
        public bool IsVpc { get; set; }

        /// <summary>
        /// Gets or sets whether the environment is in a TEE
        /// </summary>
        public bool IsTee { get; set; }

        /// <summary>
        /// Gets or sets the TEE type
        /// </summary>
        public string TeeType { get; set; }

        /// <summary>
        /// Gets or sets the TEE attestation document
        /// </summary>
        public string TeeAttestationDocument { get; set; }

        /// <summary>
        /// Gets or sets the environment-specific configuration
        /// </summary>
        public Dictionary<string, string> EnvironmentSpecificConfig { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents the type of an environment
    /// </summary>
    public enum EnvironmentType
    {
        /// <summary>
        /// Development environment
        /// </summary>
        Development,

        /// <summary>
        /// Testing environment
        /// </summary>
        Testing,

        /// <summary>
        /// Staging environment
        /// </summary>
        Staging,

        /// <summary>
        /// Production environment
        /// </summary>
        Production,

        /// <summary>
        /// Custom environment
        /// </summary>
        Custom
    }

    /// <summary>
    /// Represents the status of an environment
    /// </summary>
    public enum EnvironmentStatus
    {
        /// <summary>
        /// Creating status
        /// </summary>
        Creating,

        /// <summary>
        /// Active status
        /// </summary>
        Active,

        /// <summary>
        /// Updating status
        /// </summary>
        Updating,

        /// <summary>
        /// Deleting status
        /// </summary>
        Deleting,

        /// <summary>
        /// Failed status
        /// </summary>
        Failed,

        /// <summary>
        /// Stopped status
        /// </summary>
        Stopped
    }
}
