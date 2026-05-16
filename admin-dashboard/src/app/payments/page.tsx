'use client';

import React, { useEffect, useState, useCallback } from 'react';
import { DashboardLayout } from '@/components/layout/DashboardLayout';
import { ProtectedRoute } from '@/components/auth/ProtectedRoute';
import { Card } from '@/components/ui/Card';
import { Table, Pagination } from '@/components/ui/Table';
import { StatusBadge } from '@/components/ui/Badge';
import { SearchInput } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { adminApi } from '@/lib/api/admin';
import { Payment } from '@/types';
import { formatDate, formatCurrency } from '@/lib/utils';
import { RefreshCw, Download } from 'lucide-react';

const PAGE_SIZE = 15;

const STATUS_OPTIONS = [
  { value: '', label: 'All Status' },
  { value: 'completed', label: 'Completed' },
  { value: 'pending', label: 'Pending' },
  { value: 'failed', label: 'Failed' },
  { value: 'refunded', label: 'Refunded' },
];

// Mock data fallback
const MOCK_PAYMENTS: Payment[] = Array.from({ length: 60 }, (_, i) => ({
  id: `pay-${i + 1}`,
  amount: [500, 1000, 2500, 5000, 10000, 25000][i % 6],
  currency: 'LYD',
  method: ['wallet', 'credit_card', 'recharge_card'][i % 3],
  status: (['completed', 'completed', 'pending', 'completed', 'failed', 'refunded'] as Payment['status'][])[i % 6],
  userId: `user-${(i % 10) + 1}`,
  userName: ['Ahmad Al-Mansouri', 'Fatima Zahra', 'Khaled Hassan', 'Sara Ali', 'Omar Said'][i % 5],
  transactionId: `TXN-${String(100000 + i)}`,
  createdAt: new Date(Date.now() - i * 86400000).toISOString(),
}));

export default function PaymentsPage() {
  const [payments, setPayments] = useState<Payment[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const fetch = useCallback(async () => {
    setLoading(true);
    try {
      const res = await adminApi.getAllPayments({ page, pageSize: PAGE_SIZE, status: statusFilter || undefined });
      setPayments(res.data ?? []);
      setTotal(res.total ?? 0);
    } catch {
      let filtered = MOCK_PAYMENTS;
      if (search) filtered = filtered.filter((p) =>
        p.userName?.toLowerCase().includes(search.toLowerCase()) ||
        p.transactionId?.toLowerCase().includes(search.toLowerCase())
      );
      if (statusFilter) filtered = filtered.filter((p) => p.status === statusFilter);
      setTotal(filtered.length);
      setPayments(filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE));
    } finally {
      setLoading(false);
    }
  }, [page, search, statusFilter]);

  useEffect(() => {
    const t = setTimeout(fetch, search ? 400 : 0);
    return () => clearTimeout(t);
  }, [fetch, search]);

  const completedTotal = MOCK_PAYMENTS.filter(p => p.status === 'completed').reduce((s, p) => s + p.amount, 0);
  const failedCount = MOCK_PAYMENTS.filter(p => p.status === 'failed').length;
  const refundedTotal = MOCK_PAYMENTS.filter(p => p.status === 'refunded').reduce((s, p) => s + p.amount, 0);

  const columns = [
    {
      key: 'transactionId',
      header: 'Transaction ID',
      render: (p: Payment) => (
        <span className="font-mono text-xs text-text-secondary">{p.transactionId}</span>
      ),
    },
    {
      key: 'user',
      header: 'User',
      render: (p: Payment) => (
        <span className="text-sm font-medium text-text-primary">{p.userName}</span>
      ),
    },
    {
      key: 'amount',
      header: 'Amount',
      render: (p: Payment) => (
        <span className="text-sm font-semibold text-text-primary">{formatCurrency(p.amount, p.currency)}</span>
      ),
    },
    {
      key: 'method',
      header: 'Method',
      render: (p: Payment) => (
        <span className="text-xs capitalize text-text-muted">{(p.method ?? '').replace('_', ' ')}</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (p: Payment) => <StatusBadge status={p.status} />,
    },
    {
      key: 'createdAt',
      header: 'Date',
      render: (p: Payment) => (
        <span className="text-xs text-text-muted">{formatDate(p.createdAt)}</span>
      ),
    },
  ];

  return (
    <ProtectedRoute>
      <DashboardLayout title="Payments">
        <div className="mb-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-text-primary">Payment Transactions</h1>
              <p className="text-sm text-text-muted mt-1">{total} total transactions</p>
            </div>
            <Button variant="outline" size="sm" icon={<Download size={14} />}>Export</Button>
          </div>
        </div>

        {/* Summary */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-5">
          {[
            { label: 'Total Transactions', value: total.toString(), color: 'text-primary' },
            { label: 'Total Volume', value: formatCurrency(completedTotal), color: 'text-teal' },
            { label: 'Failed', value: failedCount.toString(), color: 'text-danger' },
            { label: 'Refunded', value: formatCurrency(refundedTotal), color: 'text-amber' },
          ].map((s) => (
            <Card key={s.label} padding="sm">
              <p className={`text-xl font-bold ${s.color}`}>{s.value}</p>
              <p className="text-xs text-text-muted mt-0.5">{s.label}</p>
            </Card>
          ))}
        </div>

        <Card padding="none">
          <div className="flex flex-wrap items-center gap-3 p-4 border-b border-border-light">
            <SearchInput value={search} onChange={setSearch} placeholder="Search by user or transaction ID..." className="w-80" />
            <Select options={STATUS_OPTIONS} value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} className="w-36" />
            <Button variant="ghost" size="sm" icon={<RefreshCw size={14} />} onClick={fetch}>Refresh</Button>
          </div>
          <Table columns={columns} data={payments} loading={loading} getRowKey={(p) => p.id} emptyMessage="No transactions found." />
          <Pagination page={page} totalPages={Math.ceil(total / PAGE_SIZE)} onPageChange={setPage} total={total} pageSize={PAGE_SIZE} />
        </Card>
      </DashboardLayout>
    </ProtectedRoute>
  );
}
