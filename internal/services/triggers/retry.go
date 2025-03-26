package triggers

// RetryPolicy represents the retry policy for trigger execution
type RetryPolicy string

const (
	// RetryPolicyNone represents no retry policy
	RetryPolicyNone RetryPolicy = "none"
	// RetryPolicyLinear represents linear retry policy
	RetryPolicyLinear RetryPolicy = "linear"
	// RetryPolicyExponential represents exponential retry policy
	RetryPolicyExponential RetryPolicy = "exponential"
)

// ValidRetryPolicies returns a list of valid retry policies
func ValidRetryPolicies() []string {
	return []string{
		string(RetryPolicyNone),
		string(RetryPolicyLinear),
		string(RetryPolicyExponential),
	}
}