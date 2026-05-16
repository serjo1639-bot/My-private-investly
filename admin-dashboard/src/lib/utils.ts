import { type ClassValue, clsx } from 'clsx';

export function cn(...inputs: ClassValue[]) {
  return inputs
    .flat()
    .filter(Boolean)
    .join(' ');
}

export function formatCurrency(amount: number, currency = 'LYD'): string {
  return new Intl.NumberFormat('ar-LY', {
    style: 'currency',
    currency,
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount);
}

export function formatNumber(num: number): string {
  if (num >= 1_000_000) return `${(num / 1_000_000).toFixed(1)}M`;
  if (num >= 1_000) return `${(num / 1_000).toFixed(1)}K`;
  return num.toString();
}

export function formatDate(dateStr: string | undefined): string {
  if (!dateStr) return '-';
  return new Date(dateStr).toLocaleDateString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  });
}

export function formatDateTime(dateStr: string | undefined): string {
  if (!dateStr) return '-';
  return new Date(dateStr).toLocaleString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function getRelativeTime(dateStr: string): string {
  const now = new Date();
  const date = new Date(dateStr);
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMins / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;
  return formatDate(dateStr);
}

export function getInitials(name: string): string {
  return name
    .split(' ')
    .map((n) => n[0])
    .slice(0, 2)
    .join('')
    .toUpperCase();
}

export function extractError(error: unknown): string {
  if (typeof error === 'string') return error;
  if (error && typeof error === 'object') {
    const e = error as Record<string, unknown>;
    // No response means the server is unreachable (not running, wrong URL, CORS preflight failed)
    if (!e.response && (e.message === 'Network Error' || e.code === 'ERR_NETWORK')) {
      return 'Unable to connect to the server. Please make sure the backend is running.';
    }
    if (e.code === 'ECONNABORTED') {
      return 'Request timed out. Please try again.';
    }
    const axiosData = (e.response as Record<string, unknown>)?.data as Record<string, unknown> | undefined;
    if (axiosData?.message && typeof axiosData.message === 'string') return axiosData.message;
    if (axiosData?.title && typeof axiosData.title === 'string') return axiosData.title;
    if (e.message && typeof e.message === 'string') return e.message;
  }
  return 'An unexpected error occurred';
}

export function getStatusColor(status: string): {
  bg: string;
  text: string;
  dot: string;
} {
  const map: Record<string, { bg: string; text: string; dot: string }> = {
    active: { bg: 'bg-green-50', text: 'text-green-700', dot: 'bg-green-500' },
    completed: { bg: 'bg-blue-50', text: 'text-blue-700', dot: 'bg-blue-500' },
    pending: { bg: 'bg-amber-50', text: 'text-amber-700', dot: 'bg-amber-500' },
    inactive: { bg: 'bg-gray-100', text: 'text-gray-600', dot: 'bg-gray-400' },
    rejected: { bg: 'bg-red-50', text: 'text-red-700', dot: 'bg-red-500' },
    suspended: { bg: 'bg-orange-50', text: 'text-orange-700', dot: 'bg-orange-500' },
    banned: { bg: 'bg-red-50', text: 'text-red-700', dot: 'bg-red-500' },
    failed: { bg: 'bg-red-50', text: 'text-red-700', dot: 'bg-red-500' },
    cancelled: { bg: 'bg-gray-100', text: 'text-gray-600', dot: 'bg-gray-400' },
    refunded: { bg: 'bg-purple-50', text: 'text-purple-700', dot: 'bg-purple-500' },
  };
  return map[status] ?? { bg: 'bg-gray-100', text: 'text-gray-600', dot: 'bg-gray-400' };
}

export function getCategoryLabel(category: string): string {
  const labels: Record<string, string> = {
    tech: 'Technology',
    energy: 'Renewable Energy',
    agri: 'Agriculture',
    health: 'Health',
    edu: 'Education',
    realestate: 'Real Estate',
  };
  return labels[category] ?? category;
}
