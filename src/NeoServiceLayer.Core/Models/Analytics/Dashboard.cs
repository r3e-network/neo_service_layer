using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models.Analytics
{
    /// <summary>
    /// Represents a dashboard in the analytics system
    /// </summary>
    public class Dashboard
    {
        /// <summary>
        /// Gets or sets the unique identifier for the dashboard
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the dashboard
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the dashboard
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the account ID that owns this dashboard
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the user ID that created this dashboard
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the widgets on the dashboard
        /// </summary>
        public List<DashboardWidget> Widgets { get; set; } = new List<DashboardWidget>();

        /// <summary>
        /// Gets or sets the layout of the dashboard
        /// </summary>
        public DashboardLayout Layout { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the dashboard
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets whether the dashboard is public
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the refresh interval in seconds
        /// </summary>
        public int RefreshIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets the time range for the dashboard
        /// </summary>
        public TimeRange TimeRange { get; set; }

        /// <summary>
        /// Gets or sets the variables for the dashboard
        /// </summary>
        public List<DashboardVariable> Variables { get; set; } = new List<DashboardVariable>();
    }

    /// <summary>
    /// Represents a widget on a dashboard
    /// </summary>
    public class DashboardWidget
    {
        /// <summary>
        /// Gets or sets the unique identifier for the widget
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the widget
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the widget
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the widget
        /// </summary>
        public WidgetType Type { get; set; }

        /// <summary>
        /// Gets or sets the data source for the widget
        /// </summary>
        public WidgetDataSource DataSource { get; set; }

        /// <summary>
        /// Gets or sets the visualization options for the widget
        /// </summary>
        public Dictionary<string, object> VisualizationOptions { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the position of the widget
        /// </summary>
        public WidgetPosition Position { get; set; }

        /// <summary>
        /// Gets or sets the size of the widget
        /// </summary>
        public WidgetSize Size { get; set; }

        /// <summary>
        /// Gets or sets the refresh interval in seconds
        /// </summary>
        public int RefreshIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets whether the widget has a border
        /// </summary>
        public bool HasBorder { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the widget has a title
        /// </summary>
        public bool ShowTitle { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the widget has a legend
        /// </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// Gets or sets the time range for the widget
        /// </summary>
        public TimeRange TimeRange { get; set; }
    }

    /// <summary>
    /// Represents the data source for a widget
    /// </summary>
    public class WidgetDataSource
    {
        /// <summary>
        /// Gets or sets the type of the data source
        /// </summary>
        public DataSourceType Type { get; set; }

        /// <summary>
        /// Gets or sets the metric IDs for the data source
        /// </summary>
        public List<Guid> MetricIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the event filter for the data source
        /// </summary>
        public EventFilter EventFilter { get; set; }

        /// <summary>
        /// Gets or sets the query for the data source
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the aggregation function for the data source
        /// </summary>
        public AggregationFunction AggregationFunction { get; set; }

        /// <summary>
        /// Gets or sets the aggregation period for the data source
        /// </summary>
        public AggregationPeriod AggregationPeriod { get; set; }

        /// <summary>
        /// Gets or sets the dimensions to group by
        /// </summary>
        public List<string> GroupBy { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the limit for the data source
        /// </summary>
        public int Limit { get; set; } = 100;

        /// <summary>
        /// Gets or sets the sort field for the data source
        /// </summary>
        public string SortField { get; set; }

        /// <summary>
        /// Gets or sets the sort direction for the data source
        /// </summary>
        public SortDirection SortDirection { get; set; }
    }

    /// <summary>
    /// Represents the position of a widget
    /// </summary>
    public class WidgetPosition
    {
        /// <summary>
        /// Gets or sets the row
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Gets or sets the column
        /// </summary>
        public int Column { get; set; }
    }

    /// <summary>
    /// Represents the size of a widget
    /// </summary>
    public class WidgetSize
    {
        /// <summary>
        /// Gets or sets the width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height
        /// </summary>
        public int Height { get; set; }
    }

    /// <summary>
    /// Represents the layout of a dashboard
    /// </summary>
    public class DashboardLayout
    {
        /// <summary>
        /// Gets or sets the number of columns
        /// </summary>
        public int Columns { get; set; } = 12;

        /// <summary>
        /// Gets or sets the row height
        /// </summary>
        public int RowHeight { get; set; } = 100;

        /// <summary>
        /// Gets or sets the padding
        /// </summary>
        public int Padding { get; set; } = 10;
    }

    /// <summary>
    /// Represents a variable for a dashboard
    /// </summary>
    public class DashboardVariable
    {
        /// <summary>
        /// Gets or sets the name of the variable
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name of the variable
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the type of the variable
        /// </summary>
        public VariableType Type { get; set; }

        /// <summary>
        /// Gets or sets the default value of the variable
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the possible values of the variable
        /// </summary>
        public List<string> Values { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the query for the variable
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets whether the variable allows multiple values
        /// </summary>
        public bool AllowMultiple { get; set; }

        /// <summary>
        /// Gets or sets whether the variable is required
        /// </summary>
        public bool IsRequired { get; set; }
    }

    /// <summary>
    /// Represents the time range for a dashboard or widget
    /// </summary>
    public class TimeRange
    {
        /// <summary>
        /// Gets or sets the start time
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        /// Gets or sets the end time
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        /// Gets or sets the relative time
        /// </summary>
        public string RelativeTime { get; set; }
    }

    /// <summary>
    /// Represents the type of a widget
    /// </summary>
    public enum WidgetType
    {
        /// <summary>
        /// Line chart
        /// </summary>
        LineChart,

        /// <summary>
        /// Bar chart
        /// </summary>
        BarChart,

        /// <summary>
        /// Pie chart
        /// </summary>
        PieChart,

        /// <summary>
        /// Area chart
        /// </summary>
        AreaChart,

        /// <summary>
        /// Scatter plot
        /// </summary>
        ScatterPlot,

        /// <summary>
        /// Table
        /// </summary>
        Table,

        /// <summary>
        /// Gauge
        /// </summary>
        Gauge,

        /// <summary>
        /// Stat
        /// </summary>
        Stat,

        /// <summary>
        /// Text
        /// </summary>
        Text,

        /// <summary>
        /// Heatmap
        /// </summary>
        Heatmap
    }

    /// <summary>
    /// Represents the type of a data source
    /// </summary>
    public enum DataSourceType
    {
        /// <summary>
        /// Metrics data source
        /// </summary>
        Metrics,

        /// <summary>
        /// Events data source
        /// </summary>
        Events,

        /// <summary>
        /// Query data source
        /// </summary>
        Query
    }

    /// <summary>
    /// Represents the type of a variable
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// Text variable
        /// </summary>
        Text,

        /// <summary>
        /// Number variable
        /// </summary>
        Number,

        /// <summary>
        /// Date variable
        /// </summary>
        Date,

        /// <summary>
        /// Boolean variable
        /// </summary>
        Boolean,

        /// <summary>
        /// Select variable
        /// </summary>
        Select,

        /// <summary>
        /// Query variable
        /// </summary>
        Query
    }
}
