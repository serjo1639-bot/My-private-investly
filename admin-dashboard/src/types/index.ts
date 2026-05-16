// ─── User Types ────────────────────────────────────────────────────────────────

export type UserRole = 'investor' | 'owner' | 'admin' | 'guest';
export type UserType = 'individual' | 'organization';
export type UserStatus = 'active' | 'suspended' | 'banned' | 'pending';

export interface User {
  id: string;
  name: string;
  phone: string;
  email: string;
  role: UserRole;
  type: UserType;
  status?: UserStatus;
  memberId?: string;
  walletBalance?: number;
  totalTopups?: number;
  contributionTotal?: number;
  contributionsCount?: number;
  projectsCount?: number;
  companyName?: string | null;
  bio?: string | null;
  token?: string | null;
  createdAt?: string;
  updatedAt?: string;
  isVerified?: boolean;
  kycStatus?: 'pending' | 'approved' | 'rejected' | 'none';
  avatar?: string | null;
}

// ─── Project Types ──────────────────────────────────────────────────────────────

export type ProjectStatus = 'active' | 'pending' | 'completed' | 'inactive' | 'rejected';
export type ProjectCategory = 'tech' | 'energy' | 'agri' | 'health' | 'edu' | 'realestate';

export interface Project {
  id: string;
  titleAr: string;
  titleEn: string;
  descriptionAr?: string;
  descriptionEn?: string;
  category: ProjectCategory;
  categoryAr?: string;
  categoryEn?: string;
  cityAr?: string;
  cityEn?: string;
  image?: string | null;
  goal: number;
  raised: number;
  minInvestment: number;
  maxInvestment?: number;
  currencyCode?: string;
  status: ProjectStatus;
  reference?: string;
  ownerId?: string;
  ownerName?: string;
  ownerCompanyName?: string | null;
  progress?: number;
  duration?: number;
  startDate?: string;
  endDate?: string;
  teamSize?: number;
  website?: string | null;
  founderName?: string;
  founderEmail?: string;
  founderPhone?: string;
  investorsCount?: number;
  viewsCount?: number;
  createdAt?: string;
  updatedAt?: string;
}

// ─── Investment Types ───────────────────────────────────────────────────────────

export type InvestmentStatus = 'pending' | 'completed' | 'failed' | 'cancelled';
export type PaymentMethod = 'wallet' | 'credit_card' | 'recharge_card';

export interface Investment {
  id: string;
  projectId: string;
  projectTitle?: string;
  reference?: string;
  amount: number;
  currency?: string;
  paymentMethod?: PaymentMethod;
  status: InvestmentStatus;
  investorId?: string;
  investorName?: string;
  createdAt?: string;
  updatedAt?: string;
}

// ─── Payment Types ──────────────────────────────────────────────────────────────

export type PaymentStatus = 'pending' | 'completed' | 'failed' | 'refunded';

export interface Payment {
  id: string;
  amount: number;
  currency?: string;
  method?: string;
  status: PaymentStatus;
  userId?: string;
  userName?: string;
  transactionId?: string;
  createdAt?: string;
  updatedAt?: string;
}

// ─── Notification Types ─────────────────────────────────────────────────────────

export type NotificationType = 'investment' | 'project' | 'system' | 'user';

export interface Notification {
  id: string;
  type: NotificationType;
  titleAr: string;
  titleEn: string;
  messageAr: string;
  messageEn: string;
  isRead: boolean;
  createdAt: string;
  targetUserId?: string;
}

// ─── Dashboard Stats ────────────────────────────────────────────────────────────

export interface DashboardStats {
  totalUsers: number;
  totalProjects: number;
  totalInvestments: number;
  totalRevenue: number;
  activeProjects: number;
  pendingProjects: number;
  newUsersToday: number;
  newUsersThisWeek: number;
  totalTransactions: number;
  successRate: number;
}

// ─── API Types ──────────────────────────────────────────────────────────────────

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  message: string;
  error?: string;
  statusCode?: number;
}

// ─── Auth Types ─────────────────────────────────────────────────────────────────

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface AuthTokens {
  token: string;
  refreshToken: string;
}

export interface AuthSession extends AuthTokens {
  user: User;
}

// ─── Chart Data Types ───────────────────────────────────────────────────────────

export interface ChartDataPoint {
  label: string;
  value: number;
  [key: string]: string | number;
}

export interface UserGrowthData {
  month: string;
  investors: number;
  owners: number;
}

export interface RevenueData {
  month: string;
  amount: number;
}
