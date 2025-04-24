using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models.Analytics
{
    /// <summary>
    /// Represents an event in the analytics system
    /// </summary>
    public class AnalyticsEvent
    {
        /// <summary>
        /// Gets or sets the unique identifier for the event
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the event
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the category of the event
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the source of the event
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the account ID associated with the event
        /// </summary>
        public Guid? AccountId { get; set; }

        /// <summary>
        /// Gets or sets the user ID associated with the event
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets the session ID associated with the event
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the event
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the properties of the event
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the tags associated with the event
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the IP address associated with the event
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the user agent associated with the event
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Gets or sets the device information associated with the event
        /// </summary>
        public DeviceInfo Device { get; set; }

        /// <summary>
        /// Gets or sets the location information associated with the event
        /// </summary>
        public LocationInfo Location { get; set; }
    }

    /// <summary>
    /// Represents device information for an event
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// Gets or sets the device type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the operating system
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the operating system version
        /// </summary>
        public string OperatingSystemVersion { get; set; }

        /// <summary>
        /// Gets or sets the browser
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// Gets or sets the browser version
        /// </summary>
        public string BrowserVersion { get; set; }
    }

    /// <summary>
    /// Represents location information for an event
    /// </summary>
    public class LocationInfo
    {
        /// <summary>
        /// Gets or sets the country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the region
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the latitude
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude
        /// </summary>
        public double? Longitude { get; set; }
    }

    /// <summary>
    /// Represents an event filter for querying events
    /// </summary>
    public class EventFilter
    {
        /// <summary>
        /// Gets or sets the event names to filter by
        /// </summary>
        public List<string> EventNames { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the event categories to filter by
        /// </summary>
        public List<string> Categories { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the sources to filter by
        /// </summary>
        public List<string> Sources { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the account IDs to filter by
        /// </summary>
        public List<Guid> AccountIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the user IDs to filter by
        /// </summary>
        public List<Guid> UserIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the start timestamp
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end timestamp
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the property filters
        /// </summary>
        public Dictionary<string, object> PropertyFilters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the tag filters
        /// </summary>
        public Dictionary<string, string> TagFilters { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the IP address filter
        /// </summary>
        public string IpAddressFilter { get; set; }

        /// <summary>
        /// Gets or sets the country filter
        /// </summary>
        public string CountryFilter { get; set; }

        /// <summary>
        /// Gets or sets the limit
        /// </summary>
        public int Limit { get; set; } = 100;

        /// <summary>
        /// Gets or sets the offset
        /// </summary>
        public int Offset { get; set; } = 0;

        /// <summary>
        /// Gets or sets the sort field
        /// </summary>
        public string SortField { get; set; } = "Timestamp";

        /// <summary>
        /// Gets or sets the sort direction
        /// </summary>
        public SortDirection SortDirection { get; set; } = SortDirection.Descending;
    }

    /// <summary>
    /// Represents the sort direction for queries
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// Ascending sort
        /// </summary>
        Ascending,

        /// <summary>
        /// Descending sort
        /// </summary>
        Descending
    }
}
