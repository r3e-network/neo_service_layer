package automation

import (
	"context"
	"fmt"
	"strconv"
	"strings"
	"sync"
	"time"
)

// CronExpression represents a parsed cron expression
type CronExpression struct {
	Minutes     []int
	Hours       []int
	DaysOfMonth []int
	Months      []int
	DaysOfWeek  []int
}

// ParseCronExpression parses a cron expression string
func ParseCronExpression(expr string) (*CronExpression, error) {
	fields := strings.Fields(expr)
	if len(fields) != 5 {
		return nil, fmt.Errorf("invalid cron expression: expected 5 fields, got %d", len(fields))
	}

	c := &CronExpression{}

	var err error
	c.Minutes, err = parseCronField(fields[0], 0, 59)
	if err != nil {
		return nil, fmt.Errorf("invalid minutes field: %w", err)
	}

	c.Hours, err = parseCronField(fields[1], 0, 23)
	if err != nil {
		return nil, fmt.Errorf("invalid hours field: %w", err)
	}

	c.DaysOfMonth, err = parseCronField(fields[2], 1, 31)
	if err != nil {
		return nil, fmt.Errorf("invalid days of month field: %w", err)
	}

	c.Months, err = parseCronField(fields[3], 1, 12)
	if err != nil {
		return nil, fmt.Errorf("invalid months field: %w", err)
	}

	c.DaysOfWeek, err = parseCronField(fields[4], 0, 6)
	if err != nil {
		return nil, fmt.Errorf("invalid days of week field: %w", err)
	}

	return c, nil
}

// parseCronField parses a single field of a cron expression
func parseCronField(field string, min, max int) ([]int, error) {
	if field == "*" {
		result := make([]int, max-min+1)
		for i := min; i <= max; i++ {
			result[i-min] = i
		}
		return result, nil
	}

	result := make([]int, 0)
	parts := strings.Split(field, ",")

	for _, part := range parts {
		if strings.Contains(part, "-") {
			rangeParts := strings.Split(part, "-")
			if len(rangeParts) != 2 {
				return nil, fmt.Errorf("invalid range: %s", part)
			}

			start, err := strconv.Atoi(rangeParts[0])
			if err != nil {
				return nil, fmt.Errorf("invalid range start: %s", rangeParts[0])
			}

			end, err := strconv.Atoi(rangeParts[1])
			if err != nil {
				return nil, fmt.Errorf("invalid range end: %s", rangeParts[1])
			}

			if start < min || end > max || start > end {
				return nil, fmt.Errorf("range out of bounds: %s", part)
			}

			for i := start; i <= end; i++ {
				result = append(result, i)
			}
		} else if strings.Contains(part, "/") {
			// Handle step values
			stepParts := strings.Split(part, "/")
			if len(stepParts) != 2 {
				return nil, fmt.Errorf("invalid step: %s", part)
			}

			var start, end int
			if stepParts[0] == "*" {
				start = min
				end = max
			} else if strings.Contains(stepParts[0], "-") {
				rangeParts := strings.Split(stepParts[0], "-")
				var err error
				start, err = strconv.Atoi(rangeParts[0])
				if err != nil {
					return nil, fmt.Errorf("invalid step start: %s", rangeParts[0])
				}

				end, err = strconv.Atoi(rangeParts[1])
				if err != nil {
					return nil, fmt.Errorf("invalid step end: %s", rangeParts[1])
				}
			} else {
				var err error
				start, err = strconv.Atoi(stepParts[0])
				if err != nil {
					return nil, fmt.Errorf("invalid step start: %s", stepParts[0])
				}
				end = max
			}

			step, err := strconv.Atoi(stepParts[1])
			if err != nil {
				return nil, fmt.Errorf("invalid step value: %s", stepParts[1])
			}

			if step <= 0 {
				return nil, fmt.Errorf("step must be positive: %d", step)
			}

			for i := start; i <= end; i += step {
				result = append(result, i)
			}
		} else {
			// Single value
			value, err := strconv.Atoi(part)
			if err != nil {
				return nil, fmt.Errorf("invalid value: %s", part)
			}

			if value < min || value > max {
				return nil, fmt.Errorf("value out of bounds: %d", value)
			}

			result = append(result, value)
		}
	}

	return result, nil
}

// Next calculates the next occurrence after the given time
func (c *CronExpression) Next(after time.Time) time.Time {
	// Start with the minute after the given time
	candidate := after.Add(time.Minute).Truncate(time.Minute)

	for {
		// Check if the candidate time matches the cron expression
		if c.matches(candidate) {
			return candidate
		}

		// Try the next minute
		candidate = candidate.Add(time.Minute)
	}
}

// matches checks if a time matches the cron expression
func (c *CronExpression) matches(t time.Time) bool {
	// Check each field
	minute := t.Minute()
	hour := t.Hour()
	dayOfMonth := t.Day()
	month := int(t.Month())
	dayOfWeek := int(t.Weekday())

	// Check if the time matches all fields
	if !contains(c.Minutes, minute) {
		return false
	}

	if !contains(c.Hours, hour) {
		return false
	}

	if !contains(c.Months, month) {
		return false
	}

	// If both day of month and day of week are specified, either can match
	dayOfMonthMatches := contains(c.DaysOfMonth, dayOfMonth)
	dayOfWeekMatches := contains(c.DaysOfWeek, dayOfWeek)

	// If both fields are restricted (not "*"), then either can match
	if len(c.DaysOfMonth) < 31 && len(c.DaysOfWeek) < 7 {
		return dayOfMonthMatches || dayOfWeekMatches
	}

	// If only one is restricted, it must match
	if len(c.DaysOfMonth) < 31 {
		return dayOfMonthMatches
	}

	if len(c.DaysOfWeek) < 7 {
		return dayOfWeekMatches
	}

	// If neither is restricted, both match
	return true
}

// contains checks if a slice contains a value
func contains(slice []int, value int) bool {
	for _, item := range slice {
		if item == value {
			return true
		}
	}
	return false
}

// ScheduledTask represents a task that is scheduled to run periodically
type ScheduledTask struct {
	ID         string
	Schedule   string
	Expression *CronExpression
	Action     func()
	NextRun    time.Time
}

// Scheduler manages scheduled tasks
type Scheduler struct {
	interval  time.Duration
	tasks     map[string]*ScheduledTask
	mutex     sync.RWMutex
	executing map[string]bool
}

// NewScheduler creates a new scheduler with the given check interval
func NewScheduler(interval time.Duration) *Scheduler {
	return &Scheduler{
		interval:  interval,
		tasks:     make(map[string]*ScheduledTask),
		executing: make(map[string]bool),
	}
}

// Start begins the scheduler's main loop
func (s *Scheduler) Start(ctx context.Context) {
	ticker := time.NewTicker(s.interval)
	defer ticker.Stop()

	for {
		select {
		case <-ticker.C:
			s.checkSchedules()
		case <-ctx.Done():
			return
		}
	}
}

// AddTask adds a new task to the scheduler
func (s *Scheduler) AddTask(id string, schedule string) error {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	// Parse the cron expression
	expr, err := ParseCronExpression(schedule)
	if err != nil {
		return fmt.Errorf("invalid schedule: %w", err)
	}

	// Calculate the next run time
	now := time.Now()
	nextRun := expr.Next(now)

	// Create and store the task
	task := &ScheduledTask{
		ID:         id,
		Schedule:   schedule,
		Expression: expr,
		NextRun:    nextRun,
	}

	s.tasks[id] = task

	return nil
}

// RemoveTask removes a task from the scheduler
func (s *Scheduler) RemoveTask(id string) error {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	if _, exists := s.tasks[id]; !exists {
		return fmt.Errorf("task with ID %s does not exist", id)
	}

	delete(s.tasks, id)

	return nil
}

// UpdateTaskSchedule updates a task's schedule
func (s *Scheduler) UpdateTaskSchedule(id string, schedule string) error {
	expr, err := ParseCronExpression(schedule)
	if err != nil {
		return fmt.Errorf("invalid cron expression '%s': %w", schedule, err)
	}

	s.mutex.Lock()
	defer s.mutex.Unlock()

	task, exists := s.tasks[id]
	if !exists {
		return fmt.Errorf("task %s not found", id)
	}

	task.Schedule = schedule
	task.Expression = expr
	task.NextRun = expr.Next(time.Now())

	return nil
}

// CalculateNextRun calculates the next run time for a schedule
func (s *Scheduler) CalculateNextRun(schedule string) (time.Time, error) {
	expr, err := ParseCronExpression(schedule)
	if err != nil {
		return time.Time{}, fmt.Errorf("invalid cron expression '%s': %w", schedule, err)
	}

	return expr.Next(time.Now()), nil
}

// GetNextRunTime gets the next run time for a task
func (s *Scheduler) GetNextRunTime(id string) (time.Time, error) {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	task, exists := s.tasks[id]
	if !exists {
		return time.Time{}, fmt.Errorf("task %s not found", id)
	}

	return task.NextRun, nil
}

// checkSchedules checks for tasks that are due to run
func (s *Scheduler) checkSchedules() {
	tasksToRun := make([]*ScheduledTask, 0)

	// First, identify tasks that need to run
	s.mutex.RLock()
	now := time.Now()
	for _, task := range s.tasks {
		if !task.NextRun.After(now) && !s.isExecuting(task.ID) {
			tasksToRun = append(tasksToRun, task)
		}
	}
	s.mutex.RUnlock()

	// Then, execute them and update their next run time
	for _, task := range tasksToRun {
		// Mark task as executing
		s.setExecuting(task.ID, true)

		// Execute task in a goroutine
		go func(t *ScheduledTask) {
			defer s.setExecuting(t.ID, false)

			// Execute the task
			t.Action()

			// Update next run time
			s.mutex.Lock()
			t.NextRun = t.Expression.Next(time.Now())
			s.mutex.Unlock()
		}(task)
	}
}

// isExecuting checks if a task is currently executing
func (s *Scheduler) isExecuting(id string) bool {
	s.mutex.RLock()
	defer s.mutex.RUnlock()
	return s.executing[id]
}

// setExecuting marks a task as executing or not
func (s *Scheduler) setExecuting(id string, executing bool) {
	s.mutex.Lock()
	defer s.mutex.Unlock()
	if executing {
		s.executing[id] = true
	} else {
		delete(s.executing, id)
	}
}
