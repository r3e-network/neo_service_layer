package functions

import (
	"bytes"
	"crypto/sha256"
	"encoding/hex"
	"errors"
	"regexp"
	"strings"
)

// CompilationOptions represents options for function compilation
type CompilationOptions struct {
	Minify            bool   // Whether to minify the code
	RemoveComments    bool   // Whether to remove comments
	StripConsoleLog   bool   // Whether to strip console.log statements
	InjectTracing     bool   // Whether to inject tracing code
	InjectGasTracking bool   // Whether to inject gas tracking code
	Environment       string // Environment (dev, test, prod)
}

// CompilationResult represents the result of function compilation
type CompilationResult struct {
	OriginalCode string   // Original source code
	CompiledCode string   // Compiled/optimized code
	SourceMap    string   // Source map (if available)
	CodeHash     string   // Hash of the compiled code
	WarningCount int      // Number of warnings during compilation
	ErrorCount   int      // Number of errors during compilation
	Warnings     []string // Compilation warnings
	Errors       []string // Compilation errors
}

// FunctionCompiler optimizes and prepares function code for execution
type FunctionCompiler struct {
	cache map[string]*CompilationResult // Cache of compilation results
}

// NewFunctionCompiler creates a new function compiler
func NewFunctionCompiler() *FunctionCompiler {
	return &FunctionCompiler{
		cache: make(map[string]*CompilationResult),
	}
}

// GetDefaultOptions returns default compilation options for the given environment
func (c *FunctionCompiler) GetDefaultOptions(environment string) *CompilationOptions {
	switch environment {
	case "prod", "production":
		return &CompilationOptions{
			Minify:            true,
			RemoveComments:    true,
			StripConsoleLog:   true,
			InjectTracing:     true,
			InjectGasTracking: true,
			Environment:       "prod",
		}
	case "test", "testing":
		return &CompilationOptions{
			Minify:            false,
			RemoveComments:    false,
			StripConsoleLog:   false,
			InjectTracing:     true,
			InjectGasTracking: true,
			Environment:       "test",
		}
	default: // dev, development
		return &CompilationOptions{
			Minify:            false,
			RemoveComments:    false,
			StripConsoleLog:   false,
			InjectTracing:     true,
			InjectGasTracking: true,
			Environment:       "dev",
		}
	}
}

// Compile compiles and optimizes function code
func (c *FunctionCompiler) Compile(code string, options *CompilationOptions) (*CompilationResult, error) {
	if code == "" {
		return nil, errors.New("empty code")
	}

	// Use default options if not provided
	if options == nil {
		options = c.GetDefaultOptions("dev")
	}

	// Check if we have a cached result
	codeHash := hashCode(code + optionsToString(options))
	if cachedResult, ok := c.cache[codeHash]; ok {
		return cachedResult, nil
	}

	// Create a new compilation result
	result := &CompilationResult{
		OriginalCode: code,
		CompiledCode: code,
		CodeHash:     codeHash,
		Warnings:     []string{},
		Errors:       []string{},
	}

	// Apply transformations
	var err error
	var transformed string

	// Remove comments if requested
	if options.RemoveComments {
		transformed, err = removeComments(result.CompiledCode)
		if err != nil {
			result.Errors = append(result.Errors, "Failed to remove comments: "+err.Error())
			result.ErrorCount++
		} else {
			result.CompiledCode = transformed
		}
	}

	// Strip console.log if requested
	if options.StripConsoleLog {
		transformed, err = stripConsoleLog(result.CompiledCode)
		if err != nil {
			result.Warnings = append(result.Warnings, "Failed to strip console.log: "+err.Error())
			result.WarningCount++
		} else {
			result.CompiledCode = transformed
		}
	}

	// Inject tracing if requested
	if options.InjectTracing {
		transformed, err = injectTracing(result.CompiledCode, options.Environment)
		if err != nil {
			result.Warnings = append(result.Warnings, "Failed to inject tracing: "+err.Error())
			result.WarningCount++
		} else {
			result.CompiledCode = transformed
		}
	}

	// Inject gas tracking if requested
	if options.InjectGasTracking {
		transformed, err = injectGasTracking(result.CompiledCode)
		if err != nil {
			result.Warnings = append(result.Warnings, "Failed to inject gas tracking: "+err.Error())
			result.WarningCount++
		} else {
			result.CompiledCode = transformed
		}
	}

	// Minify if requested
	if options.Minify {
		transformed, err = minifyCode(result.CompiledCode)
		if err != nil {
			result.Warnings = append(result.Warnings, "Failed to minify code: "+err.Error())
			result.WarningCount++
		} else {
			result.CompiledCode = transformed
		}
	}

	// Update result hash
	result.CodeHash = hashCode(result.CompiledCode)

	// Cache the result
	c.cache[codeHash] = result

	return result, nil
}

// CompileFunction compiles a function's code
func (c *FunctionCompiler) CompileFunction(function *Function, options *CompilationOptions) (*CompilationResult, error) {
	if function == nil {
		return nil, errors.New("nil function")
	}

	// Determine environment based on function status
	if options == nil {
		env := "dev"
		if function.Status == FunctionStatusActive {
			env = "prod"
		}
		options = c.GetDefaultOptions(env)
	}

	return c.Compile(function.Code, options)
}

// ClearCache clears the compilation cache
func (c *FunctionCompiler) ClearCache() {
	c.cache = make(map[string]*CompilationResult)
}

// hashCode generates a hash for a code string
func hashCode(code string) string {
	hash := sha256.Sum256([]byte(code))
	return hex.EncodeToString(hash[:])
}

// optionsToString converts compilation options to a string for caching
func optionsToString(options *CompilationOptions) string {
	if options == nil {
		return ""
	}

	var b bytes.Buffer
	b.WriteString(options.Environment)
	if options.Minify {
		b.WriteString("|minify")
	}
	if options.RemoveComments {
		b.WriteString("|nocomments")
	}
	if options.StripConsoleLog {
		b.WriteString("|noconsole")
	}
	if options.InjectTracing {
		b.WriteString("|trace")
	}
	if options.InjectGasTracking {
		b.WriteString("|gas")
	}
	return b.String()
}

// removeComments removes comments from JavaScript code
func removeComments(code string) (string, error) {
	// Remove single-line comments
	re := regexp.MustCompile(`(?m)//.*$`)
	code = re.ReplaceAllString(code, "")

	// Remove multi-line comments (simplified approach)
	re = regexp.MustCompile(`/\*[\s\S]*?\*/`)
	code = re.ReplaceAllString(code, "")

	return code, nil
}

// stripConsoleLog removes console.log statements
func stripConsoleLog(code string) (string, error) {
	// Match console.log statements including the trailing semicolon
	re := regexp.MustCompile(`console\.log\s*\([^;]*\)\s*;?`)
	code = re.ReplaceAllString(code, "")
	return code, nil
}

// injectTracing adds tracing code to the function
func injectTracing(code string, environment string) (string, error) {
	// Find the main function
	re := regexp.MustCompile(`(function\s+main\s*\()([^)]*)?(\))`)
	matches := re.FindStringSubmatch(code)
	if len(matches) < 4 {
		return code, errors.New("main function not found")
	}

	// Prepare tracing injection based on environment
	var tracingCode string
	if environment == "prod" {
		// Minimal tracing in production
		tracingCode = `
    // Production tracing
    const __startTime = Date.now();
    try {`
	} else {
		// More verbose tracing in development and testing
		tracingCode = `
    // Development tracing
    const __startTime = Date.now();
    const __functionId = (args && args._functionId) || "unknown";
    console.info("Function execution started: " + __functionId);
    try {`
	}

	// Find where to inject the code
	mainFuncPos := strings.Index(code, "function main")
	if mainFuncPos == -1 {
		return code, errors.New("could not find main function")
	}

	openBraceIndex := strings.Index(code[mainFuncPos:], "{")
	if openBraceIndex == -1 {
		return code, errors.New("could not find main function body")
	}

	// Adjust position to be relative to the full code string
	openBraceIndex += mainFuncPos

	// Insert the code just after the opening brace
	injectedCode := code[:openBraceIndex+1] + tracingCode + code[openBraceIndex+1:]

	// Now find where to add the 'finally' block
	// Look for the last closing brace
	lastBrace := strings.LastIndex(injectedCode, "}")
	if lastBrace == -1 {
		return injectedCode, errors.New("could not find closing brace")
	}

	var finallyBlock string
	if environment == "prod" {
		finallyBlock = `
    } finally {
      const __duration = Date.now() - __startTime;
      return { result, executionTime: __duration };
    }
`
	} else {
		finallyBlock = `
    } finally {
      const __duration = Date.now() - __startTime;
      console.info("Function execution completed in " + __duration + "ms");
      return { result, executionTime: __duration };
    }
`
	}

	// Insert the finally block before the last closing brace
	injectedCode = injectedCode[:lastBrace] + finallyBlock + injectedCode[lastBrace:]

	return injectedCode, nil
}

// injectGasTracking adds gas tracking code to the function
func injectGasTracking(code string) (string, error) {
	// Find the main function
	re := regexp.MustCompile(`(function\s+main\s*\()([^)]*)?(\))`)
	matches := re.FindStringSubmatch(code)
	if len(matches) < 4 {
		return code, errors.New("main function not found")
	}

	// Gas tracking code
	gasCode := `
    // Gas tracking
    let __gasUsed = 1000; // Base gas
    const __trackGas = (amount) => {
      __gasUsed += amount;
      // In a real implementation, we would check if gas limit is exceeded
    };
`

	// Find where to inject the code
	mainFuncPos := strings.Index(code, "function main")
	if mainFuncPos == -1 {
		return code, errors.New("could not find main function")
	}

	openBraceIndex := strings.Index(code[mainFuncPos:], "{")
	if openBraceIndex == -1 {
		return code, errors.New("could not find main function body")
	}

	// Adjust position to be relative to the full code string
	openBraceIndex += mainFuncPos

	// Insert the code just after the opening brace
	return code[:openBraceIndex+1] + gasCode + code[openBraceIndex+1:], nil
}

// minifyCode performs basic JavaScript minification
func minifyCode(code string) (string, error) {
	// We'll do some simple minification here
	// In a production environment, you might want to use a proper minifier library

	// Remove extra whitespace
	re := regexp.MustCompile(`\s+`)
	code = re.ReplaceAllString(code, " ")

	// Remove whitespace around operators
	re = regexp.MustCompile(`\s*([=+\-*/%&|^!<>]+)\s*`)
	code = re.ReplaceAllString(code, "$1")

	// Remove whitespace around parentheses
	re = regexp.MustCompile(`\s*\(\s*`)
	code = re.ReplaceAllString(code, "(")
	re = regexp.MustCompile(`\s*\)\s*`)
	code = re.ReplaceAllString(code, ")")

	// Remove whitespace around braces
	re = regexp.MustCompile(`\s*{\s*`)
	code = re.ReplaceAllString(code, "{")
	re = regexp.MustCompile(`\s*}\s*`)
	code = re.ReplaceAllString(code, "}")

	// Remove whitespace around brackets
	re = regexp.MustCompile(`\s*\[\s*`)
	code = re.ReplaceAllString(code, "[")
	re = regexp.MustCompile(`\s*\]\s*`)
	code = re.ReplaceAllString(code, "]")

	// Remove whitespace around commas and semicolons
	re = regexp.MustCompile(`\s*([,;])\s*`)
	code = re.ReplaceAllString(code, "$1")

	return code, nil
}
