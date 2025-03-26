package dashboard

import (
	"encoding/json"
	"fmt"
	"html/template"
	"net/http"
	"time"

	"github.com/sirupsen/logrus"
	"github.com/will/neo_service_layer/internal/services/metrics"
)

// Config holds configuration for the metrics dashboard
type Config struct {
	Port           int
	Host           string
	RefreshSeconds int
	Title          string
	EnableAuth     bool
	Username       string
	Password       string
}

// MetricsDashboard provides a web UI for metrics visualization
type MetricsDashboard struct {
	config         *Config
	metricsService *metrics.Service
	log            *logrus.Logger
	server         *http.Server
}

// dashboardTemplate is a simple HTML template for the metrics dashboard
const dashboardTemplate = `
<!DOCTYPE html>
<html>
<head>
    <title>{{.Title}} - Neo Service Layer Metrics</title>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f8f9fa;
            color: #333;
        }
        header {
            background-color: #343a40;
            color: white;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 20px;
        }
        h1 {
            margin: 0;
            font-size: 24px;
        }
        .dashboard {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
            gap: 20px;
        }
        .card {
            background: white;
            border-radius: 5px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
            padding: 15px;
        }
        .card h2 {
            margin-top: 0;
            padding-bottom: 10px;
            border-bottom: 1px solid #eee;
            font-size: 18px;
        }
        .metric {
            margin: 10px 0;
            display: flex;
            justify-content: space-between;
        }
        .metric-name {
            font-weight: 500;
        }
        .metric-value {
            font-family: monospace;
        }
        .updated {
            text-align: right;
            color: #6c757d;
            font-size: 12px;
            margin-top: 15px;
        }
        .tag {
            display: inline-block;
            background: #e9ecef;
            padding: 2px 6px;
            border-radius: 3px;
            font-size: 12px;
            margin-right: 5px;
        }
    </style>
</head>
<body>
    <header>
        <h1>{{.Title}} - Neo Service Layer Metrics</h1>
    </header>
    
    <div class="dashboard" id="metricsContainer">
        <!-- Metrics cards will be inserted here -->
    </div>
    
    <div class="updated">Last updated: <span id="lastUpdated"></span></div>
    
    <script>
        function fetchMetrics() {
            fetch('/api/metrics')
                .then(response => response.json())
                .then(data => {
                    const container = document.getElementById('metricsContainer');
                    container.innerHTML = '';
                    
                    // Update the last updated timestamp
                    document.getElementById('lastUpdated').textContent = new Date().toLocaleString();
                    
                    // Create cards for each service
                    Object.keys(data).forEach(serviceName => {
                        const serviceData = data[serviceName];
                        const card = document.createElement('div');
                        card.className = 'card';
                        
                        // Service header
                        const header = document.createElement('h2');
                        header.textContent = serviceName;
                        card.appendChild(header);
                        
                        // Tags
                        if (serviceData.tags && Object.keys(serviceData.tags).length > 0) {
                            const tagContainer = document.createElement('div');
                            tagContainer.className = 'tags';
                            
                            Object.keys(serviceData.tags).forEach(key => {
                                const tag = document.createElement('span');
                                tag.className = 'tag';
                                tag.textContent = key + ': ' + serviceData.tags[key];
                                tagContainer.appendChild(tag);
                            });
                            
                            card.appendChild(tagContainer);
                        }
                        
                        // Metrics
                        Object.keys(serviceData.metrics).forEach(metricName => {
                            const metric = document.createElement('div');
                            metric.className = 'metric';
                            
                            const name = document.createElement('div');
                            name.className = 'metric-name';
                            name.textContent = metricName;
                            
                            const value = document.createElement('div');
                            value.className = 'metric-value';
                            value.textContent = serviceData.metrics[metricName];
                            
                            metric.appendChild(name);
                            metric.appendChild(value);
                            card.appendChild(metric);
                        });
                        
                        // Timestamp
                        const timestamp = document.createElement('div');
                        timestamp.className = 'updated';
                        timestamp.textContent = 'Service data from: ' + new Date(serviceData.timestamp).toLocaleString();
                        card.appendChild(timestamp);
                        
                        container.appendChild(card);
                    });
                })
                .catch(error => {
                    console.error('Error fetching metrics:', error);
                });
        }
        
        // Initial fetch
        fetchMetrics();
        
        // Refresh metrics periodically
        setInterval(fetchMetrics, {{.RefreshSeconds}} * 1000);
    </script>
</body>
</html>
`

// NewDashboard creates a new metrics dashboard
func NewDashboard(config *Config, metricsService *metrics.Service, logger *logrus.Logger) (*MetricsDashboard, error) {
	if config == nil {
		config = &Config{
			Port:           8080,
			Host:           "0.0.0.0",
			RefreshSeconds: 10,
			Title:          "Neo Service Layer",
			EnableAuth:     false,
		}
	}

	if logger == nil {
		logger = logrus.New()
		logger.SetLevel(logrus.InfoLevel)
	}

	if metricsService == nil {
		return nil, fmt.Errorf("metrics service cannot be nil")
	}

	return &MetricsDashboard{
		config:         config,
		metricsService: metricsService,
		log:            logger,
	}, nil
}

// Start starts the dashboard HTTP server
func (d *MetricsDashboard) Start() error {
	mux := http.NewServeMux()

	// Dashboard HTML
	mux.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		if r.URL.Path != "/" {
			http.NotFound(w, r)
			return
		}

		if d.config.EnableAuth {
			username, password, ok := r.BasicAuth()
			if !ok || username != d.config.Username || password != d.config.Password {
				w.Header().Set("WWW-Authenticate", `Basic realm="Neo Service Layer Metrics"`)
				http.Error(w, "Unauthorized", http.StatusUnauthorized)
				return
			}
		}

		tmpl, err := template.New("dashboard").Parse(dashboardTemplate)
		if err != nil {
			http.Error(w, "Error rendering dashboard", http.StatusInternalServerError)
			d.log.WithError(err).Error("Failed to parse dashboard template")
			return
		}

		templateData := map[string]interface{}{
			"Title":          d.config.Title,
			"RefreshSeconds": d.config.RefreshSeconds,
		}

		w.Header().Set("Content-Type", "text/html")
		err = tmpl.Execute(w, templateData)
		if err != nil {
			http.Error(w, "Error rendering dashboard", http.StatusInternalServerError)
			d.log.WithError(err).Error("Failed to execute dashboard template")
			return
		}
	})

	// API endpoint for metrics data
	mux.HandleFunc("/api/metrics", func(w http.ResponseWriter, r *http.Request) {
		if d.config.EnableAuth {
			username, password, ok := r.BasicAuth()
			if !ok || username != d.config.Username || password != d.config.Password {
				http.Error(w, "Unauthorized", http.StatusUnauthorized)
				return
			}
		}

		metrics := d.metricsService.GetAllMetrics()
		if err := formatMetricsResponse(w, metrics); err != nil {
			http.Error(w, "Error retrieving metrics", http.StatusInternalServerError)
			d.log.WithError(err).Error("Failed to retrieve metrics")
			return
		}
	})

	// Specific service metrics endpoint
	mux.HandleFunc("/api/metrics/", func(w http.ResponseWriter, r *http.Request) {
		if d.config.EnableAuth {
			username, password, ok := r.BasicAuth()
			if !ok || username != d.config.Username || password != d.config.Password {
				http.Error(w, "Unauthorized", http.StatusUnauthorized)
				return
			}
		}

		serviceName := r.URL.Path[len("/api/metrics/"):]
		if serviceName == "" {
			http.Error(w, "Service name is required", http.StatusBadRequest)
			return
		}

		serviceType := metrics.ServiceType(serviceName)
		serviceMetrics := d.metricsService.GetMetricsForService(serviceType)
		if len(serviceMetrics) == 0 {
			http.Error(w, "Service metrics not found", http.StatusNotFound)
			return
		}

		if err := formatMetricsResponse(w, serviceMetrics); err != nil {
			http.Error(w, "Error retrieving metrics", http.StatusInternalServerError)
			d.log.WithError(err).Errorf("Failed to retrieve metrics for service %s", serviceName)
			return
		}
	})

	addr := fmt.Sprintf("%s:%d", d.config.Host, d.config.Port)
	d.server = &http.Server{
		Addr:         addr,
		Handler:      mux,
		ReadTimeout:  15 * time.Second,
		WriteTimeout: 15 * time.Second,
		IdleTimeout:  60 * time.Second,
	}

	d.log.Infof("Starting metrics dashboard at http://%s/", addr)
	return d.server.ListenAndServe()
}

// Stop stops the dashboard HTTP server
func (d *MetricsDashboard) Stop() error {
	if d.server != nil {
		d.log.Info("Stopping metrics dashboard")
		return d.server.Close()
	}
	return nil
}

// formatMetricsResponse formats metrics for JSON response
func formatMetricsResponse(w http.ResponseWriter, metrics []metrics.Metric) error {
	// Group metrics by service
	metricsByService := map[string]map[string]interface{}{}

	for _, metric := range metrics {
		serviceName := string(metric.Service)

		if _, exists := metricsByService[serviceName]; !exists {
			metricsByService[serviceName] = map[string]interface{}{
				"serviceName": serviceName,
				"timestamp":   time.Now().Format(time.RFC3339),
				"metrics":     map[string]interface{}{},
				"tags":        metric.Labels,
			}
		}

		metricsMap := metricsByService[serviceName]["metrics"].(map[string]interface{})
		metricsMap[metric.Name] = metric.Value
	}

	w.Header().Set("Content-Type", "application/json")
	return json.NewEncoder(w).Encode(metricsByService)
}
