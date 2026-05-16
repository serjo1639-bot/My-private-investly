import apiClient from './config';
import { User, PaginatedResponse } from '@/types';

export interface UsersQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  role?: string;
  status?: string;
}

export const usersApi = {
  getAllUsers: async (params?: UsersQueryParams): Promise<PaginatedResponse<User>> => {
    const response = await apiClient.get('/admin/users', { params });
    const data = response.data;
    if (Array.isArray(data)) {
      return { data, total: data.length, page: 1, pageSize: data.length, totalPages: 1 };
    }
    return data?.data ?? data;
  },

  getUserById: async (userId: string): Promise<User> => {
    const response = await apiClient.get(`/users/${userId}`);
    return response.data?.data ?? response.data;
  },

  updateUser: async (userId: string, data: Partial<User>): Promise<User> => {
    const response = await apiClient.put(`/users/${userId}`, data);
    return response.data?.data ?? response.data;
  },

  deleteUser: async (userId: string): Promise<void> => {
    await apiClient.delete(`/users/${userId}`);
  },

  banUser: async (userId: string): Promise<void> => {
    await apiClient.post(`/admin/users/${userId}/ban`);
  },

  unbanUser: async (userId: string): Promise<void> => {
    await apiClient.post(`/admin/users/${userId}/unban`);
  },

  suspendUser: async (userId: string, reason?: string): Promise<void> => {
    await apiClient.post(`/admin/users/${userId}/suspend`, { reason });
  },

  unsuspendUser: async (userId: string): Promise<void> => {
    await apiClient.post(`/admin/users/${userId}/unsuspend`);
  },

  getUserInvestments: async (userId: string) => {
    const response = await apiClient.get(`/users/${userId}/investments`);
    return response.data?.data ?? response.data;
  },

  getUserDocuments: async (userId: string) => {
    const response = await apiClient.get(`/users/${userId}/documents`);
    return response.data?.data ?? response.data;
  },

  submitKyc: async (userId: string, formData: FormData) => {
    const response = await apiClient.post(`/users/${userId}/kyc`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data?.data ?? response.data;
  },
};
