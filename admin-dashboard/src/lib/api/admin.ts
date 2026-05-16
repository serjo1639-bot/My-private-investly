import apiClient from './config';
import { DashboardStats } from '@/types';

export const adminApi = {
  getStats: async (): Promise<DashboardStats> => {
    const response = await apiClient.get('/admin/stats');
    return response.data?.data ?? response.data;
  },

  getAllPayments: async (params?: {
    page?: number;
    pageSize?: number;
    status?: string;
  }) => {
    const response = await apiClient.get('/admin/payments', { params });
    const data = response.data;
    if (Array.isArray(data)) {
      return { data, total: data.length, page: 1, pageSize: data.length, totalPages: 1 };
    }
    return data?.data ?? data;
  },

  sendNotification: async (payload: {
    targetUserId?: string;
    titleAr: string;
    titleEn: string;
    messageAr: string;
    messageEn: string;
    type: string;
  }) => {
    const response = await apiClient.post('/admin/notifications/send', payload);
    return response.data;
  },

  // Pass adminId to filter logs for a specific admin on the backend
  getActivityLogs: async (params?: { page?: number; pageSize?: number; adminId?: string }) => {
    const response = await apiClient.get('/admin/activity-logs', { params });
    return response.data?.data ?? response.data;
  },

  uploadMedia: async (file: File): Promise<{ url: string; mediaId: string }> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiClient.post('/media/upload', formData);
    return response.data?.data ?? response.data;
  },

  deleteMedia: async (mediaId: string): Promise<void> => {
    await apiClient.delete(`/media/${mediaId}`);
  },
};
