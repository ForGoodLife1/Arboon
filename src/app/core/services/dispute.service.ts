import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { environment } from '../../../environments/environment.development';
import { Dispute, DisputeMessage, OpenDisputePayload } from '../models/dispute.interface';

@Injectable({
  providedIn: 'root'
})
export class DisputeService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/disputes`;

  // جلب كل نزاعات المستقل
  getSellerDisputes(): Observable<{ success: boolean; data: Dispute[] }> {
    if (!environment.useMocks) {
      return this.http.get<{ success: boolean; data: Dispute[] }>(this.baseUrl);
    }

    const mockDisputes: Dispute[] = [
      {
        id: 'disp_101',
        escrow_id: '8a2b9c',
        escrow_title: 'تصميم متجر إلكتروني',
        reason: 'تأخر العميل في استلام العمل النهائي',
        opened_by: 'Seller',
        status: 'OPEN',
        created_at: new Date(Date.now() - 86400000).toISOString()
      },
      {
        id: 'disp_102',
        escrow_id: '9k2m1n',
        escrow_title: 'حملة تسويقية',
        reason: 'خلاف على جودة التصاميم الملحقة',
        opened_by: 'Buyer',
        status: 'RESOLVED',
        created_at: new Date(Date.now() - 172800000).toISOString()
      }
    ];


    return of({ success: true, data: mockDisputes }).pipe(delay(800));
  }

  // فتح نزاع جديد (للعُهد المجمدة فقط)
  openDispute(payload: OpenDisputePayload): Observable<{ success: boolean; data: any }> {
    if (!environment.useMocks) {
      return this.http.post<{ success: boolean; data: any }>(this.baseUrl, payload);
    }
    return of({ success: true, data: { id: 'disp_' + Math.random() } }).pipe(delay(1000));
  }

  // جلب رسائل نزاع معين
  getDisputeMessages(disputeId: string): Observable<{ success: boolean; data: DisputeMessage[] }> {
    if (!environment.useMocks) {
      return this.http.get<{ success: boolean; data: DisputeMessage[] }>(`${this.baseUrl}/${disputeId}/messages`);
    }

    const mockMessages: DisputeMessage[] = [
      { id: '1', dispute_id: disputeId, sender_type: 'Seller', message: 'لقد قمت بتسليم العمل المطلوب منذ يومين ولم يتم الرد.', created_at: new Date(Date.now() - 80000000).toISOString() },
      { id: '2', dispute_id: disputeId, sender_type: 'Buyer', message: 'أحتاج لبعض التعديلات على الخطوط والألوان.', created_at: new Date(Date.now() - 70000000).toISOString() },
      { id: '3', dispute_id: disputeId, sender_type: 'Admin', message: 'يرجى من الطرفين إرفاق الملفات النهائية للفحص.', created_at: new Date(Date.now() - 60000000).toISOString() }
    ];

    return of({ success: true, data: mockMessages }).pipe(delay(500));
  }

  // إرسال رسالة في الشات
  sendMessage(disputeId: string, message: string, attachment?: { url: string, name: string }): Observable<{ success: boolean; data: any }> {
    if (!environment.useMocks) {
      return this.http.post<{ success: boolean; data: any }>(`${this.baseUrl}/${disputeId}/messages`, { 
        message,
        attachment_url: attachment?.url,
        attachment_name: attachment?.name
      });
    }
    return of({ success: true, data: null }).pipe(delay(500));
  }

  // رفع ملف (Mock)
  uploadAttachment(file: File): Observable<{ success: boolean; data: { url: string, name: string } }> {
    // في الحقيقة ده بيرفع للسيرفر أو S3
    // هنا هنعمل محاكاة
    const mockData = {
      url: URL.createObjectURL(file), // رابط مؤقت للمعاينة
      name: file.name
    };
    return of({ success: true, data: mockData }).pipe(delay(1000));
  }
}