using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    public class AccountRegistrationRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string NeoAddress { get; set; }
    }

    public class SecretCreateRequest
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public Guid AccountId { get; set; }
        public List<Guid> AllowedFunctionIds { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class SecretUpdateValueRequest
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
    }

    public class SecretRotateRequest
    {
        public Guid Id { get; set; }
        public string NewValue { get; set; }
    }
}
