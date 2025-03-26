package config

import "time"

// APIConfig represents API configuration
type APIConfig struct {
	Host             string        `yaml:"host"`
	Port             int           `yaml:"port"`
	Endpoint         string        `yaml:"endpoint"`
	Timeout          time.Duration `yaml:"timeout"`
	EnableCORS       bool          `yaml:"enableCors"`
	MaxRequestBodySize int64       `yaml:"maxRequestBodySize"`
}

// DefaultAPIConfig returns default API configuration
func DefaultAPIConfig() APIConfig {
	return APIConfig{
		Host:              "localhost",
		Port:              8080,
		Endpoint:          "http://localhost:10332",
		Timeout:           30 * time.Second,
		EnableCORS:        true,
		MaxRequestBodySize: 10 * 1024 * 1024, // 10MB
	}
}