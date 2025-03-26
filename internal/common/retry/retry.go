package retry

// Policy represents the retry policy for operations
type Policy string

const (
	// PolicyNone represents no retry policy
	PolicyNone Policy = "none"
	// PolicyLinear represents linear retry policy
	PolicyLinear Policy = "linear"
	// PolicyExponential represents exponential retry policy
	PolicyExponential Policy = "exponential"
)

// ValidPolicies returns a list of valid retry policies
func ValidPolicies() []string {
	return []string{
		string(PolicyNone),
		string(PolicyLinear),
		string(PolicyExponential),
	}
}