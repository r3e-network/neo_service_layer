package dashboard

import (
	"fmt"
	"html/template"
	"strings"
	"time"

	"github.com/will/neo_service_layer/internal/services/metrics"
)

// DashboardConfig represents a pre-defined metrics dashboard configuration
type DashboardConfig struct {
	Id          string
	Title       string
	Description string
	Category    string
	Version     string
	Created     time.Time
	Updated     time.Time
	Layout      string
	Panels      []Panel
	Variables   []Variable
}

// Panel represents a dashboard visualization panel
type Panel struct {
	Id          string
	Title       string
	Description string
	Type        string // "graph", "gauge", "table", etc.
	Width       int    // Width in grid units
	Height      int    // Height in grid units
	X           int    // X position in grid
	Y           int    // Y position in grid
	DataSource  string
	Query       string
	Format      string // "time_series", "table", etc.
	Options     map[string]interface{}
}

// Variable represents a dashboard template variable
type Variable struct {
	Name        string
	Label       string
	Description string
	Type        string // "query", "custom", "interval", etc.
	Query       string
	Options     []string
	Default     string
	Required    bool
}

// DashboardRegistry manages a collection of dashboards
type DashboardRegistry struct {
	dashboards map[string]*DashboardConfig
}

// NewDashboardRegistry creates a new dashboard registry
func NewDashboardRegistry() *DashboardRegistry {
	registry := &DashboardRegistry{
		dashboards: make(map[string]*DashboardConfig),
	}

	// Register default dashboards
	registry.registerDefaultDashboards()

	return registry
}

// GetDashboard retrieves a dashboard by ID
func (r *DashboardRegistry) GetDashboard(id string) (*DashboardConfig, error) {
	if dashboard, exists := r.dashboards[id]; exists {
		return dashboard, nil
	}
	return nil, fmt.Errorf("dashboard not found: %s", id)
}

// ListDashboards returns all registered dashboards
func (r *DashboardRegistry) ListDashboards() []*DashboardConfig {
	dashboards := make([]*DashboardConfig, 0, len(r.dashboards))
	for _, dashboard := range r.dashboards {
		dashboards = append(dashboards, dashboard)
	}
	return dashboards
}

// RegisterDashboard adds a dashboard to the registry
func (r *DashboardRegistry) RegisterDashboard(dashboard *DashboardConfig) error {
	if dashboard.Id == "" {
		return fmt.Errorf("dashboard Id cannot be empty")
	}
	if _, exists := r.dashboards[dashboard.Id]; exists {
		return fmt.Errorf("dashboard already exists: %s", dashboard.Id)
	}
	r.dashboards[dashboard.Id] = dashboard
	return nil
}

// registerDefaultDashboards creates and registers default system dashboards
func (r *DashboardRegistry) registerDefaultDashboards() {
	// System Overview Dashboard
	systemDashboard := &DashboardConfig{
		Id:          "system-overview",
		Title:       "System Overview",
		Description: "Overview of all services and system health",
		Category:    "System",
		Version:     "1.0.0",
		Created:     time.Now(),
		Updated:     time.Now(),
		Layout:      "grid",
		Panels: []Panel{
			{
				Id:          "service-health",
				Title:       "Service Health",
				Description: "Health status of all services",
				Type:        "gauge",
				Width:       6,
				Height:      4,
				X:           0,
				Y:           0,
				DataSource:  "metrics",
				Query:       "service_health",
				Format:      "gauge",
				Options: map[string]interface{}{
					"thresholds": []float64{0.7, 0.9},
					"colors":     []string{"#d44a3a", "#e5ac0e", "#299c46"},
				},
			},
			{
				Id:          "api-requests",
				Title:       "API Requests",
				Description: "Total API requests over time",
				Type:        "graph",
				Width:       6,
				Height:      4,
				X:           6,
				Y:           0,
				DataSource:  "metrics",
				Query:       "api_requests_total",
				Format:      "time_series",
				Options: map[string]interface{}{
					"legend":      true,
					"tooltips":    true,
					"grid":        true,
					"yAxisLabel":  "Requests",
					"xAxisLabel":  "Time",
					"lineColors":  []string{"#1f78c1", "#33a02c"},
					"fillOpacity": 0.1,
				},
			},
			{
				Id:          "memory-usage",
				Title:       "Memory Usage",
				Description: "Memory usage across services",
				Type:        "graph",
				Width:       12,
				Height:      5,
				X:           0,
				Y:           4,
				DataSource:  "metrics",
				Query:       "memory_usage_bytes",
				Format:      "time_series",
				Options: map[string]interface{}{
					"legend":     true,
					"tooltips":   true,
					"grid":       true,
					"yAxisLabel": "Memory (MB)",
					"xAxisLabel": "Time",
					"lineColors": []string{"#ff7f0e", "#9467bd", "#d62728"},
					"stack":      true,
				},
			},
		},
		Variables: []Variable{
			{
				Name:        "timeRange",
				Label:       "Time Range",
				Description: "Time period to display",
				Type:        "interval",
				Options:     []string{"1h", "6h", "12h", "24h", "7d", "30d"},
				Default:     "24h",
				Required:    true,
			},
			{
				Name:        "service",
				Label:       "Service",
				Description: "Filter by service",
				Type:        "custom",
				Options:     []string{"all", "api", "functions", "gasbank", "trigger", "pricefeed"},
				Default:     "all",
				Required:    false,
			},
		},
	}
	r.dashboards[systemDashboard.Id] = systemDashboard

	// API Service Dashboard
	apiDashboard := &DashboardConfig{
		Id:          "api-service",
		Title:       "API Service",
		Description: "Detailed metrics for the API Service",
		Category:    "Services",
		Version:     "1.0.0",
		Created:     time.Now(),
		Updated:     time.Now(),
		Layout:      "grid",
		Panels: []Panel{
			{
				Id:          "request-overview",
				Title:       "Request Overview",
				Description: "Overview of API requests",
				Type:        "stat",
				Width:       4,
				Height:      3,
				X:           0,
				Y:           0,
				DataSource:  "metrics",
				Query:       "api_requests_total",
				Format:      "stat",
				Options: map[string]interface{}{
					"colorMode":   "value",
					"graphMode":   "area",
					"justifyMode": "auto",
				},
			},
			{
				Id:          "request-rate",
				Title:       "Request Rate",
				Description: "Requests per second",
				Type:        "graph",
				Width:       8,
				Height:      3,
				X:           4,
				Y:           0,
				DataSource:  "metrics",
				Query:       "api_request_rate",
				Format:      "time_series",
				Options: map[string]interface{}{
					"legend":     true,
					"tooltips":   true,
					"yAxisLabel": "Requests/s",
					"xAxisLabel": "Time",
				},
			},
			{
				Id:          "response-time",
				Title:       "Response Time",
				Description: "API response time",
				Type:        "graph",
				Width:       6,
				Height:      4,
				X:           0,
				Y:           3,
				DataSource:  "metrics",
				Query:       "api_response_time",
				Format:      "time_series",
				Options: map[string]interface{}{
					"legend":     true,
					"tooltips":   true,
					"yAxisLabel": "Time (ms)",
					"xAxisLabel": "Time",
				},
			},
			{
				Id:          "error-rate",
				Title:       "Error Rate",
				Description: "API error percentage",
				Type:        "graph",
				Width:       6,
				Height:      4,
				X:           6,
				Y:           3,
				DataSource:  "metrics",
				Query:       "api_error_rate",
				Format:      "time_series",
				Options: map[string]interface{}{
					"legend":     true,
					"tooltips":   true,
					"yAxisLabel": "Error Rate (%)",
					"xAxisLabel": "Time",
					"threshold":  5,
				},
			},
		},
		Variables: []Variable{
			{
				Name:        "timeRange",
				Label:       "Time Range",
				Description: "Time period to display",
				Type:        "interval",
				Options:     []string{"1h", "6h", "12h", "24h", "7d", "30d"},
				Default:     "24h",
				Required:    true,
			},
			{
				Name:        "endpoint",
				Label:       "Endpoint",
				Description: "Filter by API endpoint",
				Type:        "query",
				Query:       "api_endpoints",
				Default:     "all",
				Required:    false,
			},
		},
	}
	r.dashboards[apiDashboard.Id] = apiDashboard

	// Gas Bank Dashboard
	gasBankDashboard := &DashboardConfig{
		Id:          "gas-bank",
		Title:       "Gas Bank Service",
		Description: "Detailed metrics for the Gas Bank Service",
		Category:    "Services",
		Version:     "1.0.0",
		Created:     time.Now(),
		Updated:     time.Now(),
		Layout:      "grid",
		Panels: []Panel{
			{
				Id:          "gas-overview",
				Title:       "Gas Overview",
				Description: "Overview of Gas Bank status",
				Type:        "stat",
				Width:       4,
				Height:      3,
				X:           0,
				Y:           0,
				DataSource:  "metrics",
				Query:       "gas_total",
				Format:      "stat",
				Options: map[string]interface{}{
					"colorMode":   "value",
					"graphMode":   "area",
					"justifyMode": "auto",
				},
			},
			{
				Id:          "gas-allocation",
				Title:       "Gas Allocation",
				Description: "Gas allocation over time",
				Type:        "graph",
				Width:       8,
				Height:      3,
				X:           4,
				Y:           0,
				DataSource:  "metrics",
				Query:       "gas_allocation_rate",
				Format:      "time_series",
				Options: map[string]interface{}{
					"legend":     true,
					"tooltips":   true,
					"yAxisLabel": "Gas/s",
					"xAxisLabel": "Time",
				},
			},
			{
				Id:          "gas-usage",
				Title:       "Gas Usage",
				Description: "Gas usage by service",
				Type:        "pie",
				Width:       6,
				Height:      4,
				X:           0,
				Y:           3,
				DataSource:  "metrics",
				Query:       "gas_usage_by_service",
				Format:      "time_series",
				Options: map[string]interface{}{
					"legend":      true,
					"tooltips":    true,
					"innerRadius": 0.3,
				},
			},
			{
				Id:          "gas-balance",
				Title:       "Gas Balance",
				Description: "Available gas over time",
				Type:        "graph",
				Width:       6,
				Height:      4,
				X:           6,
				Y:           3,
				DataSource:  "metrics",
				Query:       "gas_balance",
				Format:      "time_series",
				Options: map[string]interface{}{
					"legend":     true,
					"tooltips":   true,
					"yAxisLabel": "Gas",
					"xAxisLabel": "Time",
				},
			},
		},
		Variables: []Variable{
			{
				Name:        "timeRange",
				Label:       "Time Range",
				Description: "Time period to display",
				Type:        "interval",
				Options:     []string{"1h", "6h", "12h", "24h", "7d", "30d"},
				Default:     "24h",
				Required:    true,
			},
			{
				Name:        "user",
				Label:       "User",
				Description: "Filter by user",
				Type:        "query",
				Query:       "gas_users",
				Default:     "all",
				Required:    false,
			},
		},
	}
	r.dashboards[gasBankDashboard.Id] = gasBankDashboard
}

// RenderDashboard renders a dashboard to HTML
func RenderDashboard(dashboard *DashboardConfig, data map[string]interface{}) (string, error) {
	// If data map doesn't include metrics data, insert dummy placeholder
	if _, exists := data["metrics"]; !exists {
		// Create dummy metrics data as a placeholder
		dummyMetric := metrics.Metric{
			Name:    "example_metric",
			Type:    metrics.MetricTypeGauge,
			Value:   0.0,
			Service: metrics.ServiceAPI,
		}
		data["metrics"] = []metrics.Metric{dummyMetric}
	}

	// Basic dashboard template
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
        .dashboard-grid {
            display: grid;
            grid-template-columns: repeat(12, 1fr);
            grid-gap: 15px;
        }
        .panel {
            background: white;
            border-radius: 5px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
            padding: 15px;
        }
        .panel-header {
            margin-top: 0;
            padding-bottom: 10px;
            border-bottom: 1px solid #eee;
            font-size: 18px;
        }
        .panel-description {
            color: #6c757d;
            font-size: 14px;
            margin-bottom: 15px;
        }
        .variables {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            margin-bottom: 20px;
            padding: 15px;
            background: white;
            border-radius: 5px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
        }
        .variable {
            display: flex;
            flex-direction: column;
        }
        .variable label {
            font-weight: 500;
            margin-bottom: 5px;
        }
        .variable select {
            padding: 8px;
            border-radius: 3px;
            border: 1px solid #ced4da;
        }
    </style>
</head>
<body>
    <header>
        <h1>{{.Title}} - {{.Description}}</h1>
    </header>
    
    <div class="variables">
        {{range .Variables}}
        <div class="variable">
            <label for="{{.Name}}">{{.Label}}</label>
            <select id="{{.Name}}" name="{{.Name}}">
                {{range .Options}}
                <option value="{{.}}" {{if eq . (index $ (print .Name "_default"))}}selected{{end}}>{{.}}</option>
                {{end}}
            </select>
        </div>
        {{end}}
    </div>
    
    <div class="dashboard-grid">
        {{range .Panels}}
        <div class="panel" style="grid-column: span {{.Width}}; grid-row: span {{.Height}};">
            <h2 class="panel-header">{{.Title}}</h2>
            <div class="panel-description">{{.Description}}</div>
            <div class="panel-content" id="panel-{{.Id}}">
                <!-- Panel content will be inserted here by JavaScript -->
                <div class="loading">Loading...</div>
            </div>
        </div>
        {{end}}
    </div>
    
    <script>
        // Dashboard functionality would be implemented here
        // This would include fetching data and rendering charts
    </script>
</body>
</html>
`

	// Parse the template
	tmpl, err := template.New("dashboard").Parse(dashboardTemplate)
	if err != nil {
		return "", fmt.Errorf("failed to parse dashboard template: %w", err)
	}

	// Combine dashboard data with the provided data
	templateData := map[string]interface{}{
		"Id":          dashboard.Id,
		"Title":       dashboard.Title,
		"Description": dashboard.Description,
		"Version":     dashboard.Version,
		"Panels":      dashboard.Panels,
		"Variables":   dashboard.Variables,
		"Data":        data,
	}

	// For default values in variable dropdowns
	for _, variable := range dashboard.Variables {
		templateData[variable.Name+"_default"] = variable.Default
	}

	// Render the template to a string
	var sb strings.Builder
	err = tmpl.Execute(&sb, templateData)
	if err != nil {
		return "", fmt.Errorf("failed to render dashboard template: %w", err)
	}

	return sb.String(), nil
}
