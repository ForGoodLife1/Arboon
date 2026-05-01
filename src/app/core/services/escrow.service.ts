import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../../../environments/environment.development';
import { 
  Escrow, 
  CreateEscrowPayload, 
  PaymentPayload, 
  DashboardData, 
  WalletData,
  WithdrawalPayload
} from '../models/escrow.interface';

@Injectable({
  providedIn: 'root'
})
export class EscrowService {
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);
  private baseUrl = `${environment.apiUrl}/escrows`;

  createEscrow(data: CreateEscrowPayload): Observable<{ success: boolean; data: Escrow }> {
    if (!environment.useMocks) {
      return this.http.post<{ success: boolean; data: Escrow }>(this.baseUrl, data);
    }

    const mockEscrow: Escrow = {
      id: 'esc_' + Math.random().toString(36).substring(2, 9),
      title: data.title,
      description: data.description,
      amount: data.amount,
      currency: data.currency,
      conditions: data.conditions || '',
      status: 'PENDING',
      payment_url: isPlatformBrowser(this.platformId) ? `${window.location.origin}/pay/esc_mock_123` : '',
      created_at: new Date().toISOString(),
      seller_name: 'أنت (البائع)' 
    };

    return of({ success: true, data: mockEscrow }).pipe(delay(500)); 
  }

  getEscrow(id: string): Observable<{ success: boolean; data: Escrow }> {
    if (!environment.useMocks) {
      return this.http.get<{ success: boolean; data: Escrow }>(`${this.baseUrl}/${id}`);
    }

    const mockData: Escrow = {
      id: id,
      seller_name: 'أحمد محمود',
      seller_avatar: 'https://ui-avatars.com/api/?name=Ahmed+Mahmoud&background=10B981&color=fff',
      seller_rating: 4.8,
      title: 'تصميم هوية بصرية لشركة ألفا',
      description: 'هذا المشروع يشمل تصميم الشعار، اختيار الألوان، وتصميم مطبوعات الشركة الأساسية (Business Cards, Letterheads).',
      amount: 1500,
      currency: 'SAR',
      status: 'PENDING',
      created_at: '2024-05-15',
      conditions: 'يتم التسليم بعد 3 أيام من تجميد المبلغ، ويحق للمشتري طلب تعديلين بحد أقصى.'
    };

    return of({ success: true, data: mockData }).pipe(delay(500));
  }


  payEscrow(id: string, payload: PaymentPayload): Observable<any> {
    if (!environment.useMocks) {
      return this.http.post<any>(`${this.baseUrl}/${id}/pay`, payload, { withCredentials: true });
    }
    return of({ 
      success: true, 
      message: "تم الدفع وتأمين المبلغ بنجاح.", 
      data: { status: 'FROZEN' } 
    }).pipe(delay(1000));
  }

  releaseEscrow(id: string, buyerToken?: string): Observable<any> {
    if (!environment.useMocks) {
      const body = buyerToken ? { buyer_token: buyerToken } : {};
      return this.http.post(`${this.baseUrl}/${id}/release`, body, { withCredentials: true });
    }
    return of({ success: true, message: 'تم تحرير الأموال بنجاح.' }).pipe(delay(1000));
  }

  getDashboardData(): Observable<{ success: boolean; data: DashboardData }> {
    if (!environment.useMocks) {
      return this.http.get<{ success: boolean; data: DashboardData }>(`${this.baseUrl}/dashboard`);
    }
    
    const mockData: DashboardData = {
      stats: { frozen_total: 12500.50, completed_count: 45, pending_count: 3 },
      recent_escrows: [
        { id: '8a2b9c', title: 'تصميم متجر إلكتروني', description: 'برمجة وتصميم', amount: 5000, currency: 'SAR', status: 'FROZEN', created_at: '2024-05-12' },
        { id: '1f4d3e', title: 'برمجة تطبيق توصيل', description: 'تطوير تطبيق أندرويد', amount: 7500, currency: 'SAR', status: 'PENDING', created_at: '2024-05-10' },
        { id: '9k2m1n', title: 'حملة تسويقية', description: 'إدارة حملة', amount: 3200, currency: 'SAR', status: 'RELEASED', created_at: '2024-05-08' }
      ]
    };
    return of({ success: true, data: mockData }).pipe(delay(800));
  }

  getWalletData(): Observable<{ success: boolean; data: WalletData }> {
    if (!environment.useMocks) {
      return this.http.get<{ success: boolean; data: WalletData }>(`${this.baseUrl}/wallet`);
    }

    const mockWallet: WalletData = {
      stats: { available: 12450.00, pending: 3200.50, withdrawn: 8500.00 },
      transactions: [
        { id: 'TX-9921', type: 'WITHDRAWAL', amount: 1500, method: 'Instapay', date: '2024-05-20', status: 'COMPLETED' },
        { id: 'TX-8812', type: 'DEPOSIT', amount: 5000, method: 'عُربون', date: '2024-05-18', status: 'COMPLETED' }
      ]
    };
    return of({ success: true, data: mockWallet }).pipe(delay(600));
  }

  requestWithdrawal(payload: WithdrawalPayload): Observable<any> {
    if (!environment.useMocks) {
      return this.http.post(`${this.baseUrl}/wallet/withdraw`, payload);
    }
    return of({ success: true, message: 'تم استلام طلب السحب بنجاح.' }).pipe(delay(1000));
  }
}