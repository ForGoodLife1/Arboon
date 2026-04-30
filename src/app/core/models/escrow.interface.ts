// src/app/core/models/escrow.interface.ts

export type EscrowStatus = 'PENDING' | 'FROZEN' | 'RELEASED' | 'CANCELLED';

export type PaymentMethod = 'CARD' | 'INSTAPAY' | 'WALLET' | 'PAYPAL';

export interface Escrow {
  id: string;
  title: string;
  description: string;
  seller_name?: string;
  amount: number;
  currency: string;
  conditions?: string;
  status: EscrowStatus;
  payment_url?: string;
  created_at: string;
}

export interface CreateEscrowPayload {
  title: string;
  description: string;
  amount: number;
  currency: string;
  conditions?: string;
}

export interface PaymentPayload {
  payment_method: PaymentMethod;
  buyer_email: string;
  card_token?: string;
  ipa_address?: string;
  wallet_number?: string;
  paypal_email?: string;
}

export interface DashboardStats {
  frozen_total: number;
  completed_count: number;
  pending_count: number;
}

export interface DashboardData {
  stats: DashboardStats;
  recent_escrows: Escrow[];
}

export interface WalletStats {
  available: number;
  pending: number;
  withdrawn: number;
}

export interface Transaction {
  id: string;
  type: 'WITHDRAWAL' | 'DEPOSIT';
  amount: number;
  method: string;
  date: string;
  status: 'PENDING' | 'COMPLETED' | 'FAILED';
}

export interface WalletData {
  stats: WalletStats;
  transactions: Transaction[];
}

export interface WithdrawalPayload {
  amount: number;
  method: 'INSTAPAY' | 'BANK_TRANSFER' | 'WALLET';
  account_details: string;
}