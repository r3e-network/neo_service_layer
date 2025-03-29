/**
 * Formatters for Secret Service
 * 
 * This module provides utility functions for formatting secret data
 * in the Neo Service Layer website.
 */

/**
 * Format a date string to a human-readable format
 * @param dateString ISO date string
 * @returns Formatted date string
 */
export function formatDate(dateString: string): string {
  if (!dateString) return 'N/A';
  
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date);
}

/**
 * Format a secret value for display (mask sensitive data)
 * @param value Secret value
 * @returns Masked value
 */
export function formatSecretValue(value: string): string {
  if (!value) return '';
  
  // Show only first and last 2 characters, mask the rest
  if (value.length <= 4) {
    return '••••';
  }
  
  return `${value.substring(0, 2)}${'•'.repeat(Math.min(10, value.length - 4))}${value.substring(value.length - 2)}`;
}

/**
 * Format a secret type for display
 * @param type Secret type
 * @returns Formatted type string
 */
export function formatSecretType(type: string): string {
  if (!type) return 'Unknown';
  
  // Convert snake_case or kebab-case to Title Case
  return type
    .replace(/[-_]/g, ' ')
    .replace(/\w\S*/g, (word) => word.charAt(0).toUpperCase() + word.substring(1).toLowerCase());
}

/**
 * Format a file size in bytes to a human-readable string
 * @param bytes File size in bytes
 * @returns Formatted file size string
 */
export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes';
  
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
}

/**
 * Truncate a string to a specified length
 * @param str String to truncate
 * @param maxLength Maximum length
 * @returns Truncated string
 */
export function truncateString(str: string, maxLength: number = 30): string {
  if (!str) return '';
  if (str.length <= maxLength) return str;
  
  return `${str.substring(0, maxLength - 3)}...`;
}
