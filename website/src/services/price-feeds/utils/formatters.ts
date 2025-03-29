export function formatCurrency(
  value: number,
  options: Intl.NumberFormatOptions = {}
): string {
  const defaultOptions: Intl.NumberFormatOptions = {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
    ...options
  };

  return new Intl.NumberFormat('en-US', defaultOptions).format(value);
}

export function formatNumber(
  value: number,
  precision?: number,
  options: Intl.NumberFormatOptions = {}
): string {
  const defaultOptions: Intl.NumberFormatOptions = {
    minimumFractionDigits: precision ?? 0,
    maximumFractionDigits: precision ?? 2,
    ...options
  };

  return new Intl.NumberFormat('en-US', defaultOptions).format(value);
}

export function formatPercent(
  value: number,
  precision?: number,
  options: Intl.NumberFormatOptions = {}
): string {
  const defaultOptions: Intl.NumberFormatOptions = {
    style: 'percent',
    minimumFractionDigits: precision ?? 2,
    maximumFractionDigits: precision ?? 2,
    ...options
  };

  return new Intl.NumberFormat('en-US', defaultOptions).format(value);
}

export function formatDate(
  timestamp: number,
  options: Intl.DateTimeFormatOptions = {}
): string {
  const defaultOptions: Intl.DateTimeFormatOptions = {
    dateStyle: 'medium',
    timeStyle: 'medium',
    ...options
  };

  return new Intl.DateTimeFormat('en-US', defaultOptions).format(new Date(timestamp));
}

export function formatTimeAgo(timestamp: number): string {
  const now = Date.now();
  const diff = now - timestamp;
  const seconds = Math.floor(diff / 1000);
  const minutes = Math.floor(seconds / 60);
  const hours = Math.floor(minutes / 60);
  const days = Math.floor(hours / 24);

  if (days > 0) return `${days}d ago`;
  if (hours > 0) return `${hours}h ago`;
  if (minutes > 0) return `${minutes}m ago`;
  return `${seconds}s ago`;
}