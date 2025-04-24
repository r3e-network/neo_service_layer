using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Enclave.Enclave.Models
{
    /// <summary>
    /// Request to create a secret
    /// </summary>
    public class CreateSecretRequest
    {
        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the secret name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the secret description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the secret value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the allowed function IDs
        /// </summary>
        public List<Guid> AllowedFunctionIds { get; set; }

        /// <summary>
        /// Gets or sets the rotation period in days
        /// </summary>
        public int? RotationPeriod { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the secret
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }
    }

    /// <summary>
    /// Request to get a secret value
    /// </summary>
    public class GetSecretValueRequest
    {
        /// <summary>
        /// Gets or sets the secret ID
        /// </summary>
        public Guid SecretId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid? FunctionId { get; set; }
    }

    /// <summary>
    /// Request to update a secret value
    /// </summary>
    public class UpdateSecretValueRequest
    {
        /// <summary>
        /// Gets or sets the secret ID
        /// </summary>
        public Guid SecretId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the secret value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the secret description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the allowed function IDs
        /// </summary>
        public List<Guid> AllowedFunctionIds { get; set; }

        /// <summary>
        /// Gets or sets the rotation period in days
        /// </summary>
        public int? RotationPeriod { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the secret
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }
    }

    /// <summary>
    /// Request to rotate a secret
    /// </summary>
    public class RotateSecretRequest
    {
        /// <summary>
        /// Gets or sets the secret ID
        /// </summary>
        public Guid SecretId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the new value (optional)
        /// </summary>
        public string NewValue { get; set; }
    }

    /// <summary>
    /// Request to delete a secret
    /// </summary>
    public class DeleteSecretRequest
    {
        /// <summary>
        /// Gets or sets the secret ID
        /// </summary>
        public Guid SecretId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }
    }
}
