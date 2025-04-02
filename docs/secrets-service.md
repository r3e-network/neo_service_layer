# Secrets Service

## 1. Introduction

### 1.1 Purpose
The Secrets Service provides a secure mechanism for users to store and manage sensitive information (like API keys, private keys, passwords) within the Neo Service Layer. It ensures that secrets are encrypted at rest and securely accessed at runtime, primarily by user-defined Functions running within a Trusted Execution Environment (TEE).

### 1.2 Scope
- Secure storage of user-provided secrets.
- Management interface for users (create, read, update, delete - CRUD).
- Permission system to control access to secrets.
- Secure retrieval mechanism for authorized Functions within the TEE.

## 2. Architecture

### 2.1 Overview
Secrets submitted by users are encrypted using a master key (potentially managed by the TEE or a secure key management system) before being persisted in the database. When a Function requests access to a secret, the service verifies the Function's permissions and, if authorized, decrypts the secret within the TEE enclave for the Function's use. Direct access to decrypted secrets outside the TEE is prohibited.

```
+-----------------+      +----------------------+      +--------------------+
| User (via API)  |----->| Secrets Service (API)|----->| Permission Manager |
| (Signed Msg)    |      | (Outside TEE)        |      | (Checks ACLs)      |
+-----------------+      +----------------------+      +--------------------+
                             |          ^
                             | Encrypt  | Decrypt Request (via TEE Runtime)
                             v          |
+-----------------+      +----------------------+      +--------------------+
| Function        |<-----| Secrets Service (TEE)|<---->| Encrypted DB Store |
| (Running in TEE)|      | (Runtime Access)     |      | (Secrets at Rest)  |
+-----------------+      +----------------------+      +--------------------+
```

### 2.2 Key Components
1.  **API Handler:** Receives signed user requests for managing secrets (CRUD). Verifies signatures.
2.  **Permission Manager:** Manages and enforces access control rules (e.g., which Function ID can access which Secret ID owned by a specific User Address).
3.  **Encryption/Decryption Module (TEE):** Handles the cryptographic operations within the secure enclave.
4.  **Secure Storage:** Database where encrypted secrets and permission metadata are stored.
5.  **TEE Runtime Access:** Internal interface allowing Functions running in the TEE to request permitted secrets.

## 3. Features

### 3.1 Secret Management (User API)
Users interact via signed messages sent to the API service endpoint for secrets.

-   **Create Secret:**
    -   Input: `name` (string), `value` (string, sensitive data), `permissions` (list of Function IDs allowed access).
    -   Action: Encrypts `value`, stores it with the `name` and owner (derived from signature), and sets initial permissions.
    -   Output: `secret_id`.
-   **Get Secret (Metadata):**
    -   Input: `secret_id`.
    -   Action: Retrieves non-sensitive metadata (name, owner, permissions). The actual secret value is *not* returned.
    -   Output: Secret metadata.
-   **List Secrets:**
    -   Action: Returns metadata for all secrets owned by the user.
    -   Output: List of secret metadata.
-   **Update Secret (Value):**
    -   Input: `secret_id`, `new_value`.
    -   Action: Re-encrypts and replaces the stored secret value. Requires ownership verification.
-   **Update Secret (Permissions):**
    -   Input: `secret_id`, `new_permissions`.
    -   Action: Updates the access control list for the secret. Requires ownership verification.
-   **Delete Secret:**
    -   Input: `secret_id`.
    -   Action: Permanently removes the secret and its permissions. Requires ownership verification.

### 3.2 Secret Access (Function Runtime - TEE)
Functions running within the TEE can request secrets they need.

-   **Request Secret:**
    -   Input: `secret_id`.
    -   Action: The TEE runtime calls the Secrets Service (within TEE). The service checks if the requesting Function ID has permission for the given `secret_id` (associated with the user context the function runs under). If permitted, the service retrieves the encrypted value, decrypts it *within the TEE*, and returns the plaintext value directly to the Function.
    -   Output: Plaintext secret value (only within TEE).

### 3.3 Permissions Model
-   Permissions are tied to a specific `secret_id`.
-   Permissions grant access rights to specific `function_id`s.
-   Only the owner of a secret (identified by their Neo address) can manage the secret's value and permissions.

## 4. Security Considerations

-   **Encryption:** Secrets must be encrypted at rest using strong, standard algorithms (e.g., AES-GCM).
-   **Key Management:** The master encryption key must be securely managed, ideally protected by the TEE or a dedicated Hardware Security Module (HSM).
-   **TEE Guarantees:** Relies on the TEE's isolation properties to protect secrets during decryption and use.
-   **Access Control:** Strict enforcement of permissions is critical.
-   **Audit Logging:** Log all secret management operations and access requests (without logging the secret values themselves).
-   **Input Validation:** Sanitize all inputs, especially secret names and permission identifiers.

## 5. Future Enhancements
-   Secret rotation policies.
-   Integration with external secret managers (e.g., HashiCorp Vault).
-   More granular permissions (e.g., read-only access). 