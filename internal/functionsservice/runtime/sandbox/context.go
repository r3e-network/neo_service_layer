package sandbox

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"net/http"
	"reflect"
	"time"

	"github.com/dop251/goja"
	"go.uber.org/zap"
)

// setupExecutionEnvironment prepares the VM with necessary bindings and context
// Called from execution.go
func (s *Sandbox) setupExecutionEnvironment(ctx *FunctionContext, output *FunctionOutput) error {
	// Set up console for logging
	console := s.createConsoleObject(output)
	err := s.vm.Set("console", console)
	if err != nil {
		return fmt.Errorf("failed to set console object: %w", err)
	}

	// Set up context object
	ctxObj, err := s.createContextObject(ctx)
	if err != nil {
		return fmt.Errorf("failed to create context object: %w", err)
	}
	err = s.vm.Set("context", ctxObj)
	if err != nil {
		return fmt.Errorf("failed to set context object: %w", err)
	}

	// Set up service bindings if enabled
	if s.config.EnableInteroperability {
		err = s.setupServiceBindings(ctx)
		if err != nil {
			s.logger.Warn("Failed to set up service bindings", zap.Error(err))
			// Continue execution despite service binding failures?
		}
	}

	// Inject HTTP client if network access is allowed
	if s.config.AllowNetwork {
		httpClientObj, err := s.createHttpClientBinding()
		if err != nil {
			s.logger.Error("Failed to create HTTP client binding", zap.Error(err))
			// Decide if this is a fatal error for the sandbox setup
			return fmt.Errorf("failed to create HTTP client binding: %w", err)
		}
		err = s.vm.Set("httpClient", httpClientObj)
		if err != nil {
			return fmt.Errorf("failed to set httpClient object: %w", err)
		}
	} else {
		// Optionally set httpClient to null or undefined if network is disabled
		_ = s.vm.Set("httpClient", goja.Null())
	}

	return nil
}

// createHttpClientBinding creates a JavaScript object for making HTTP requests.
func (s *Sandbox) createHttpClientBinding() (map[string]interface{}, error) {
	// Create a shared HTTP client instance for the sandbox (consider timeouts, etc.)
	client := &http.Client{
		Timeout: 15 * time.Second, // Example timeout
	}

	httpObj := map[string]interface{}{}

	// Define a generic 'request' function
	httpObj["request"] = func(call goja.FunctionCall) goja.Value {
		// Check if network access is still allowed (could be changed dynamically?)
		if !s.config.AllowNetwork {
			panic(s.vm.NewGoError(errors.New("httpClient error: network access is disabled")))
		}

		// Expect a single argument: an options object
		if len(call.Arguments) != 1 {
			panic(s.vm.NewGoError(errors.New("httpClient.request: requires exactly one options object argument")))
		}
		optionsVal := call.Argument(0)
		if optionsVal == nil || goja.IsUndefined(optionsVal) || goja.IsNull(optionsVal) {
			panic(s.vm.NewGoError(errors.New("httpClient.request: options object cannot be null or undefined")))
		}
		options := optionsVal.Export().(map[string]interface{}) // Assuming it's an object

		// Extract options
		method, _ := options["method"].(string)
		url, _ := options["url"].(string)
		headers, _ := options["headers"].(map[string]interface{}) // JS map[string]string comes as map[string]interface{}
		bodyArg, bodyExists := options["body"]

		if url == "" {
			panic(s.vm.NewGoError(errors.New("httpClient.request: 'url' option is required")))
		}
		if method == "" {
			method = "GET" // Default to GET
		}

		// Prepare request body
		var bodyReader io.Reader
		if bodyExists {
			if bodyStr, ok := bodyArg.(string); ok {
				bodyReader = bytes.NewBufferString(bodyStr)
			} else {
				// Try to JSON marshal if it's not a string (e.g., a JS object)
				bodyBytes, err := json.Marshal(bodyArg)
				if err != nil {
					panic(s.vm.NewGoError(fmt.Errorf("httpClient.request: failed to marshal request body: %w", err)))
				}
				bodyReader = bytes.NewBuffer(bodyBytes)
				// Automatically set Content-Type if marshalling
				if headers == nil {
					headers = make(map[string]interface{})
				}
				if _, ctExists := headers["Content-Type"]; !ctExists {
					headers["Content-Type"] = "application/json"
				}
			}
		}

		// Create request
		req, err := http.NewRequestWithContext(context.Background(), method, url, bodyReader) // Use background context for now
		if err != nil {
			panic(s.vm.NewGoError(fmt.Errorf("httpClient.request: failed to create request: %w", err)))
		}

		// Add headers
		if headers != nil {
			for k, v := range headers {
				if vStr, ok := v.(string); ok {
					req.Header.Set(k, vStr)
				}
			}
		}

		// Make the request
		resp, err := client.Do(req)
		if err != nil {
			panic(s.vm.NewGoError(fmt.Errorf("httpClient.request: failed to execute request to %s: %w", url, err)))
		}
		defer resp.Body.Close()

		// Read response body
		respBodyBytes, err := io.ReadAll(resp.Body)
		if err != nil {
			panic(s.vm.NewGoError(fmt.Errorf("httpClient.request: failed to read response body from %s: %w", url, err)))
		}

		// Prepare response object for JS
		jsResp := map[string]interface{}{
			"status":     resp.StatusCode,
			"statusText": resp.Status,
			"headers":    map[string][]string(resp.Header),
			"body":       string(respBodyBytes), // Return body as string for simplicity
		}

		// Try to parse body as JSON if Content-Type suggests it
		contentType := resp.Header.Get("Content-Type")
		if len(respBodyBytes) > 0 && (contentType == "application/json" || contentType == "text/json") {
			var jsonData interface{}
			if json.Unmarshal(respBodyBytes, &jsonData) == nil {
				jsResp["json"] = jsonData // Add parsed JSON object
			}
		}

		return s.vm.ToValue(jsResp)
	}

	// Add convenience methods like get, post?
	// httpObj["get"] = func(...) { ... call request ... }
	// httpObj["post"] = func(...) { ... call request ... }

	return httpObj, nil
}

// setupServiceBindings creates service client bindings for JavaScript
func (s *Sandbox) setupServiceBindings(ctx *FunctionContext) error {
	if ctx.ServiceClients == nil {
		return errors.New("service clients not available")
	}

	// Create services object for JavaScript
	servicesObj := map[string]interface{}{}

	// Add available service clients
	if ctx.ServiceClients.Wallet != nil {
		walletObj, err := s.createWalletBinding(ctx.ServiceClients.Wallet)
		if err != nil {
			s.logger.Warn("Failed to create wallet binding", zap.Error(err))
		} else {
			servicesObj["wallet"] = walletObj
		}
	}

	if ctx.ServiceClients.Storage != nil {
		storageObj, err := s.createStorageBinding(ctx.ServiceClients.Storage)
		if err != nil {
			s.logger.Warn("Failed to create storage binding", zap.Error(err))
		} else {
			servicesObj["storage"] = storageObj
		}
	}

	if ctx.ServiceClients.Oracle != nil {
		oracleObj, err := s.createOracleBinding(ctx.ServiceClients.Oracle)
		if err != nil {
			s.logger.Warn("Failed to create oracle binding", zap.Error(err))
		} else {
			servicesObj["oracle"] = oracleObj
		}
	}

	// Set services object in VM
	err := s.vm.Set("services", servicesObj)
	if err != nil {
		return fmt.Errorf("failed to set services object: %w", err)
	}

	return nil
}

// createWalletBinding creates JavaScript bindings for wallet service
func (s *Sandbox) createWalletBinding(client interface{}) (map[string]interface{}, error) {
	if client == nil {
		return nil, errors.New("wallet client is nil")
	}

	// Create wallet object with exported methods
	return s.exportServiceMethods(client, []string{
		"OpenWallet",
		"CreateWallet",
		"ListWallets",
		"GetWalletInfo",
		"CreateAccount",
		"ListAccounts",
		"GetAccountInfo",
		"SignMessage",
		"CloseWallet",
	})
}

// createStorageBinding creates JavaScript bindings for storage service
func (s *Sandbox) createStorageBinding(client interface{}) (map[string]interface{}, error) {
	if client == nil {
		return nil, errors.New("storage client is nil")
	}

	// Create storage object with exported methods
	return s.exportServiceMethods(client, []string{
		"Put",
		"Get",
		"Delete",
		"List",
	})
}

// createOracleBinding creates JavaScript bindings for oracle service
func (s *Sandbox) createOracleBinding(client interface{}) (map[string]interface{}, error) {
	if client == nil {
		return nil, errors.New("oracle client is nil")
	}

	// Create oracle object with exported methods
	return s.exportServiceMethods(client, []string{
		"GetData",
		"SubmitRequest",
		"GetRequestStatus",
	})
}

// exportServiceMethods creates JavaScript bindings for service methods
func (s *Sandbox) exportServiceMethods(service interface{}, methods []string) (map[string]interface{}, error) {
	if service == nil {
		return nil, errors.New("service is nil")
	}

	result := make(map[string]interface{})
	serviceValue := reflect.ValueOf(service)
	serviceType := serviceValue.Type()

	// Check if service is a pointer and get its element type
	if serviceType.Kind() != reflect.Ptr {
		return nil, fmt.Errorf("service must be a pointer, got %s", serviceType.Kind())
	}

	// Export each method
	for _, methodName := range methods {
		method := serviceValue.MethodByName(methodName)
		if !method.IsValid() {
			s.logger.Warn("Method not found on service", zap.String("method", methodName))
			continue
		}

		// Create JavaScript function that calls the Go method
		result[methodName] = s.createMethodWrapper(method)
	}

	return result, nil
}

// createMethodWrapper creates a JavaScript function that calls a Go method
func (s *Sandbox) createMethodWrapper(method reflect.Value) func(call goja.FunctionCall) goja.Value {
	return func(call goja.FunctionCall) goja.Value {
		// Convert JavaScript arguments to Go values
		methodType := method.Type()
		numArgs := methodType.NumIn()
		args := make([]reflect.Value, numArgs)

		// Check if we have enough arguments
		if len(call.Arguments) < numArgs {
			s.vm.Interrupt(fmt.Sprintf("not enough arguments, expected %d, got %d", numArgs, len(call.Arguments)))
			return goja.Undefined()
		}

		// Convert arguments
		for i := 0; i < numArgs; i++ {
			// Get the expected type for this argument
			expectedType := methodType.In(i)

			// Convert JavaScript value to Go value
			goArg, err := s.jsValueToGoValue(call.Arguments[i], expectedType)
			if err != nil {
				s.vm.Interrupt(fmt.Sprintf("failed to convert argument %d: %v", i, err))
				return goja.Undefined()
			}

			args[i] = goArg
		}

		// Call the Go method
		results := method.Call(args)

		// Convert results back to JavaScript values
		if len(results) == 0 {
			return goja.Undefined()
		}

		// Handle the common pattern of (result, error) return values
		if len(results) == 2 && !results[1].IsNil() && results[1].Type().Implements(reflect.TypeOf((*error)(nil)).Elem()) {
			// If the second return value is a non-nil error, throw a JavaScript exception
			err := results[1].Interface().(error)
			s.vm.Interrupt(err.Error())
			return goja.Undefined()
		}

		// Convert the first result to a JavaScript value
		jsResult := s.vm.ToValue(results[0].Interface())

		return jsResult
	}
}

// jsValueToGoValue converts a JavaScript value to a Go value of the expected type
func (s *Sandbox) jsValueToGoValue(jsVal goja.Value, expectedType reflect.Type) (reflect.Value, error) {
	if jsVal == nil || goja.IsUndefined(jsVal) || goja.IsNull(jsVal) {
		// Return zero value for expected type
		return reflect.Zero(expectedType), nil
	}

	// Export to Go value
	goVal := jsVal.Export()
	if goVal == nil {
		return reflect.Zero(expectedType), nil
	}

	goValType := reflect.TypeOf(goVal)

	// If types match or can be assigned directly
	if goValType.AssignableTo(expectedType) {
		return reflect.ValueOf(goVal), nil
	}

	// Handle type conversions
	switch expectedType.Kind() {
	case reflect.String:
		return reflect.ValueOf(fmt.Sprintf("%v", goVal)), nil
	case reflect.Int, reflect.Int8, reflect.Int16, reflect.Int32, reflect.Int64:
		// Try to convert number to int
		if f, ok := goVal.(float64); ok {
			return reflect.ValueOf(int(f)).Convert(expectedType), nil
		}
	case reflect.Uint, reflect.Uint8, reflect.Uint16, reflect.Uint32, reflect.Uint64:
		// Try to convert number to uint
		if f, ok := goVal.(float64); ok {
			return reflect.ValueOf(uint(f)).Convert(expectedType), nil
		}
	case reflect.Float32, reflect.Float64:
		// Try to convert number to float
		if f, ok := goVal.(float64); ok {
			return reflect.ValueOf(f).Convert(expectedType), nil
		}
	case reflect.Bool:
		// Convert to boolean
		return reflect.ValueOf(goVal != nil && goVal != false && goVal != 0), nil
	case reflect.Slice:
		// Convert JavaScript array to Go slice
		if arr, ok := goVal.([]interface{}); ok {
			elemType := expectedType.Elem()
			sliceVal := reflect.MakeSlice(expectedType, len(arr), len(arr))

			for i, item := range arr {
				elemVal, err := s.jsValueToGoValue(s.vm.ToValue(item), elemType)
				if err != nil {
					return reflect.Value{}, err
				}
				sliceVal.Index(i).Set(elemVal)
			}

			return sliceVal, nil
		}
	case reflect.Map:
		// Convert JavaScript object to Go map
		if obj, ok := goVal.(map[string]interface{}); ok {
			keyType := expectedType.Key()
			elemType := expectedType.Elem()
			mapVal := reflect.MakeMap(expectedType)

			for k, v := range obj {
				keyValue := s.vm.ToValue(k)
				keyVal, err := s.jsValueToGoValue(keyValue, keyType)
				if err != nil {
					return reflect.Value{}, err
				}

				elemValue := s.vm.ToValue(v)
				elemVal, err := s.jsValueToGoValue(elemValue, elemType)
				if err != nil {
					return reflect.Value{}, err
				}

				mapVal.SetMapIndex(keyVal, elemVal)
			}

			return mapVal, nil
		}
	}

	// If we get here, we couldn't convert the value
	return reflect.Value{}, fmt.Errorf("cannot convert %v (type %T) to %v", goVal, goVal, expectedType)
}
