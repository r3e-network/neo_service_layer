/**
 * Utility functions for formatting gas-related data
 */

/**
 * Formats a gas amount with appropriate units
 * @param amount The gas amount to format
 * @returns Formatted gas amount string
 */
export function formatGas(amount: number): string {
  if (amount >= 1_000_000) {
    return `${(amount / 1_000_000).toFixed(2)} MGas`;
  } else if (amount >= 1_000) {
    return `${(amount / 1_000).toFixed(2)} kGas`;
  }
  return `${amount.toFixed(2)} Gas`;
}

/**
 * Formats a date for display
 * @param date The date to format
 * @returns Formatted date string
 */
export function formatDate(date: Date): string {
  return date.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
}

/**
 * Formats a duration in milliseconds to a human-readable string
 * @param milliseconds Duration in milliseconds
 * @returns Formatted duration string
 */
export function formatDuration(milliseconds: number): string {
  const seconds = Math.floor(milliseconds / 1000);
  const minutes = Math.floor(seconds / 60);
  const hours = Math.floor(minutes / 60);
  const days = Math.floor(hours / 24);

  if (days > 0) {
    return `${days}d ${hours % 24}h`;
  } else if (hours > 0) {
    return `${hours}h ${minutes % 60}m`;
  } else if (minutes > 0) {
    return `${minutes}m ${seconds % 60}s`;
  }
  return `${seconds}s`;
}

/**
 * Calculates and returns a human-readable time remaining string
 * @param expiryTimestamp The expiry timestamp in milliseconds
 * @returns Time remaining string
 */
export function getTimeRemaining(expiryTimestamp: number): string {
  const now = Date.now();
  const remaining = expiryTimestamp - now;
  
  if (remaining <= 0) {
    return 'Expired';
  }
  
  return formatDuration(remaining);
}