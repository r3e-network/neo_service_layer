package functions

import (
	"fmt"
	"regexp"
	"strings"
	"time"
)

// ValidationRuleLevel represents the severity level of a validation rule
type ValidationRuleLevel string

const (
	// ValidationLevelError indicates a rule that must not be violated
	ValidationLevelError ValidationRuleLevel = "error"

	// ValidationLevelWarning indicates a rule that should not be violated
	ValidationLevelWarning ValidationRuleLevel = "warning"

	// ValidationLevelInfo indicates a rule that is informational
	ValidationLevelInfo ValidationRuleLevel = "info"
)

// ValidationRule represents a rule for validating function code
type ValidationRule struct {
	ID          string                           // Unique identifier for the rule
	Name        string                           // Name of the rule
	Description string                           // Description of the rule
	Level       ValidationRuleLevel              // Severity level
	Pattern     *regexp.Regexp                   // Pattern to match in the code
	Validator   func(code string) (bool, string) // Custom validator function
}

// ValidationResult represents the result of validating function code
type ValidationResult struct {
	Valid        bool                   // Whether the code is valid
	ErrorCount   int                    // Number of errors
	WarningCount int                    // Number of warnings
	InfoCount    int                    // Number of informational messages
	Violations   []*ValidationViolation // List of rule violations
}

// ValidationViolation represents a violation of a validation rule
type ValidationViolation struct {
	RuleID      string              // ID of the violated rule
	Level       ValidationRuleLevel // Severity level
	Message     string              // Violation message
	LineNumber  int                 // Line number where the violation occurred
	ColumnStart int                 // Column where the violation starts
	ColumnEnd   int                 // Column where the violation ends
}

// FunctionValidator validates function code against security rules
type FunctionValidator struct {
	rules []*ValidationRule
}

// NewFunctionValidator creates a new function validator with default rules
func NewFunctionValidator() *FunctionValidator {
	validator := &FunctionValidator{
		rules: []*ValidationRule{},
	}

	// Add default rules
	validator.AddRule(&ValidationRule{
		ID:          "SEC001",
		Name:        "No eval usage",
		Description: "Prevents the use of eval() which can execute arbitrary code",
		Level:       ValidationLevelError,
		Pattern:     regexp.MustCompile(`\beval\s*\(`),
	})

	validator.AddRule(&ValidationRule{
		ID:          "SEC002",
		Name:        "No new Function usage",
		Description: "Prevents the use of new Function() which can execute arbitrary code",
		Level:       ValidationLevelError,
		Pattern:     regexp.MustCompile(`\bnew\s+Function\s*\(`),
	})

	validator.AddRule(&ValidationRule{
		ID:          "SEC003",
		Name:        "No process access",
		Description: "Prevents access to Node.js process object",
		Level:       ValidationLevelError,
		Pattern:     regexp.MustCompile(`\bprocess\b`),
	})

	validator.AddRule(&ValidationRule{
		ID:          "SEC004",
		Name:        "No require usage",
		Description: "Prevents the use of require() to load external modules",
		Level:       ValidationLevelError,
		Pattern:     regexp.MustCompile(`\brequire\s*\(`),
	})

	validator.AddRule(&ValidationRule{
		ID:          "SEC005",
		Name:        "No import statement",
		Description: "Prevents the use of import statements to load external modules",
		Level:       ValidationLevelError,
		Pattern:     regexp.MustCompile(`\bimport\s+.*\bfrom\b`),
	})

	validator.AddRule(&ValidationRule{
		ID:          "SEC006",
		Name:        "No global modifications",
		Description: "Prevents modifications to global objects",
		Level:       ValidationLevelError,
		Pattern:     regexp.MustCompile(`\b(global|window|globalThis)\s*\.\s*\w+\s*=`),
	})

	validator.AddRule(&ValidationRule{
		ID:          "SEC007",
		Name:        "No __proto__ access",
		Description: "Prevents access to __proto__ which can be used for prototype pollution",
		Level:       ValidationLevelError,
		Pattern:     regexp.MustCompile(`\.__proto__\b`),
	})

	validator.AddRule(&ValidationRule{
		ID:          "SEC008",
		Name:        "No infinite loops",
		Description: "Warns about potential infinite loops",
		Level:       ValidationLevelWarning,
		Pattern:     regexp.MustCompile(`(while\s*\(\s*true\s*\)|for\s*\(\s*;\s*;\s*\))`),
	})

	validator.AddRule(&ValidationRule{
		ID:          "SEC009",
		Name:        "Main function exists",
		Description: "Ensures that a main function exists",
		Level:       ValidationLevelError,
		Validator: func(code string) (bool, string) {
			if !regexp.MustCompile(`\bfunction\s+main\s*\(`).MatchString(code) {
				return false, "Function must have a 'main' function defined"
			}
			return true, ""
		},
	})

	validator.AddRule(&ValidationRule{
		ID:          "QUA001",
		Name:        "No console.log in production",
		Description: "Warns about use of console.log which should be removed in production",
		Level:       ValidationLevelWarning,
		Pattern:     regexp.MustCompile(`\bconsole\.log\s*\(`),
	})

	validator.AddRule(&ValidationRule{
		ID:          "QUA002",
		Name:        "No TODO comments",
		Description: "Warns about TODO comments that should be addressed",
		Level:       ValidationLevelInfo,
		Pattern:     regexp.MustCompile(`//\s*TODO\b`),
	})

	return validator
}

// AddRule adds a validation rule
func (v *FunctionValidator) AddRule(rule *ValidationRule) {
	v.rules = append(v.rules, rule)
}

// Validate validates function code against the rules
func (v *FunctionValidator) Validate(code string) *ValidationResult {
	result := &ValidationResult{
		Valid:      true,
		Violations: []*ValidationViolation{},
	}

	// Apply each rule
	for _, rule := range v.rules {
		// If the rule has a custom validator, use it
		if rule.Validator != nil {
			valid, message := rule.Validator(code)
			if !valid {
				violation := &ValidationViolation{
					RuleID:  rule.ID,
					Level:   rule.Level,
					Message: message,
				}

				result.Violations = append(result.Violations, violation)
				if rule.Level == ValidationLevelError {
					result.Valid = false
					result.ErrorCount++
				} else if rule.Level == ValidationLevelWarning {
					result.WarningCount++
				} else {
					result.InfoCount++
				}
			}
			continue
		}

		// If the rule has a pattern, apply it
		if rule.Pattern != nil {
			matches := rule.Pattern.FindAllStringIndex(code, -1)
			if len(matches) > 0 {
				// For each match, create a violation
				for _, match := range matches {
					// Calculate line and column numbers
					lineNumber, columnStart := getLineAndColumn(code, match[0])
					columnEnd := columnStart + (match[1] - match[0])

					violation := &ValidationViolation{
						RuleID:      rule.ID,
						Level:       rule.Level,
						Message:     rule.Description,
						LineNumber:  lineNumber,
						ColumnStart: columnStart,
						ColumnEnd:   columnEnd,
					}

					result.Violations = append(result.Violations, violation)
					if rule.Level == ValidationLevelError {
						result.Valid = false
						result.ErrorCount++
					} else if rule.Level == ValidationLevelWarning {
						result.WarningCount++
					} else {
						result.InfoCount++
					}
				}
			}
		}
	}

	return result
}

// ValidateFunction validates a function and returns error if invalid
func (v *FunctionValidator) ValidateFunction(function *Function) error {
	// Skip validation for non-JavaScript functions
	if function.Runtime != JavaScriptRuntime {
		return nil
	}

	result := v.Validate(function.Code)
	if !result.Valid {
		// Construct error message from violations
		errors := []string{}
		for _, violation := range result.Violations {
			if violation.Level == ValidationLevelError {
				if violation.LineNumber > 0 {
					errors = append(errors, fmt.Sprintf("%s: %s (line %d)",
						violation.RuleID, violation.Message, violation.LineNumber))
				} else {
					errors = append(errors, fmt.Sprintf("%s: %s",
						violation.RuleID, violation.Message))
				}
			}
		}

		return fmt.Errorf("function validation failed: %s", strings.Join(errors, "; "))
	}

	return nil
}

// getLineAndColumn calculates the line and column number for a position in the code
func getLineAndColumn(code string, position int) (int, int) {
	if position < 0 || position >= len(code) {
		return 1, 1
	}

	lineNumber := 1
	columnNumber := 1

	for i := 0; i < position; i++ {
		if code[i] == '\n' {
			lineNumber++
			columnNumber = 1
		} else {
			columnNumber++
		}
	}

	return lineNumber, columnNumber
}

// ValidateRuntimeConstraints validates runtime constraints for a function
func (v *FunctionValidator) ValidateRuntimeConstraints(function *Function, maxSize int, maxTimeout time.Duration) error {
	if len(function.Code) > maxSize {
		return fmt.Errorf("function code exceeds maximum size of %d bytes", maxSize)
	}

	// Additional runtime constraints could be checked here

	return nil
}
