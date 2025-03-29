export function isValidCronExpression(expression: string): boolean {
  const parts = expression.split(' ');
  
  // Basic cron expression should have 5 or 6 parts
  if (parts.length < 5 || parts.length > 6) {
    return false;
  }

  // Validate each part
  const validators = [
    isValidMinutes,   // Minutes: 0-59
    isValidHours,     // Hours: 0-23
    isValidDayOfMonth,// Day of Month: 1-31
    isValidMonth,     // Month: 1-12 or JAN-DEC
    isValidDayOfWeek, // Day of Week: 0-6 or SUN-SAT
    isValidYear       // Year: optional
  ];

  return parts.every((part, index) => {
    if (index >= validators.length) return true;
    return validators[index](part);
  });
}

function isValidMinutes(value: string): boolean {
  return isValidRange(value, 0, 59);
}

function isValidHours(value: string): boolean {
  return isValidRange(value, 0, 23);
}

function isValidDayOfMonth(value: string): boolean {
  return isValidRange(value, 1, 31);
}

function isValidMonth(value: string): boolean {
  const months = ['JAN', 'FEB', 'MAR', 'APR', 'MAY', 'JUN', 'JUL', 'AUG', 'SEP', 'OCT', 'NOV', 'DEC'];
  if (value.includes(',')) {
    return value.split(',').every(v => isValidMonth(v));
  }
  if (value.includes('-')) {
    const [start, end] = value.split('-');
    return isValidMonth(start) && isValidMonth(end);
  }
  if (value === '*') return true;
  
  const num = parseInt(value);
  if (!isNaN(num)) {
    return num >= 1 && num <= 12;
  }
  
  return months.includes(value.toUpperCase());
}

function isValidDayOfWeek(value: string): boolean {
  const days = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'];
  if (value.includes(',')) {
    return value.split(',').every(v => isValidDayOfWeek(v));
  }
  if (value.includes('-')) {
    const [start, end] = value.split('-');
    return isValidDayOfWeek(start) && isValidDayOfWeek(end);
  }
  if (value === '*') return true;
  
  const num = parseInt(value);
  if (!isNaN(num)) {
    return num >= 0 && num <= 6;
  }
  
  return days.includes(value.toUpperCase());
}

function isValidYear(value: string): boolean {
  if (!value) return true; // Year is optional
  return isValidRange(value, 1970, 2099);
}

function isValidRange(value: string, min: number, max: number): boolean {
  // Handle lists (e.g., "1,2,3")
  if (value.includes(',')) {
    return value.split(',').every(v => isValidRange(v, min, max));
  }

  // Handle ranges (e.g., "1-5")
  if (value.includes('-')) {
    const [start, end] = value.split('-').map(Number);
    return !isNaN(start) && !isNaN(end) &&
           start >= min && start <= max &&
           end >= min && end <= max &&
           start <= end;
  }

  // Handle step values (e.g., "*/5" or "1-30/5")
  if (value.includes('/')) {
    const [range, step] = value.split('/');
    const stepNum = parseInt(step);
    if (isNaN(stepNum) || stepNum < 1) return false;
    
    if (range === '*') return true;
    return isValidRange(range, min, max);
  }

  // Handle asterisk
  if (value === '*') return true;

  // Handle single numbers
  const num = parseInt(value);
  return !isNaN(num) && num >= min && num <= max;
}