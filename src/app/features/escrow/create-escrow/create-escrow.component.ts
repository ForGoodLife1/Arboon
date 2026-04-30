import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { EscrowService } from '../../../core/services/escrow.service';
import { CreateEscrowPayload } from '../../../core/models/escrow.interface';

@Component({
  selector: 'app-create-escrow',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './create-escrow.component.html',
  styleUrls: ['./create-escrow.component.css']
})
export class CreateEscrowComponent {
  private escrowService = inject(EscrowService);

  // ----------------------------------------
  // خصائص مرتبطة بالـ UI (ngModel)
  // ----------------------------------------
  title: string = '';
  amount: number | null = null;
  currency: string = 'SAR';
  description: string = ''; // الحقل الإجباري الجديد في الـ Interface
  conditions: string = '';    // حقل الشروط الإضافية (اختياري)

  // ----------------------------------------
  // حالة الشاشة (State)
  // ----------------------------------------
  isLoading: boolean = false;
  createdId: string | null = null;

  // ----------------------------------------
  // الدوال (Methods)
  // ----------------------------------------

  /**
   * حساب رسوم المنصة (1.5%) لعرضها لحظياً في الـ UI
   */
  calculateFee(): string {
    if (!this.amount) return '0.00';
    return (this.amount * 0.015).toFixed(2);
  }

  /**
   * إرسال البيانات للباك إند لإنشاء العُهدة وتوليد الرابط
   */
  createLink() {
    // التحقق من البيانات الأساسية المطلوبة برمجياً وبزنس
    if (!this.title || !this.amount || !this.description) {
      alert('يرجى إكمال البيانات الأساسية: العنوان، المبلغ، ووصف الخدمة.');
      return;
    }

    this.isLoading = true;

    // تجهيز الـ Payload بناءً على الـ Interface المحدث
    const payload: CreateEscrowPayload = {
      title: this.title,
      amount: Number(this.amount), // التأكد من إرساله كـ number
      currency: this.currency,
      description: this.description, // الحقل اللي كان عامل المشكلة
      conditions: this.conditions     // الشروط الإضافية
    };

    console.log('جاري إرسال الطلب...', payload);

    this.escrowService.createEscrow(payload).subscribe({
      next: (response) => {
        // تأخير بسيط (800ms) لضمان تجربة مستخدم سلسة (UX)
        setTimeout(() => {
          this.isLoading = false;
          if (response.success) {
            this.createdId = response.data.id;
            console.log('تم إنشاء العُهدة بنجاح، المعرف:', this.createdId);
          }
        }, 800);
      },
      error: (err) => {
        console.error('فشل في إنشاء الرابط:', err);
        this.isLoading = false;
        alert('حدث خطأ أثناء الاتصال بالسيرفر، يرجى المحاولة لاحقاً.');
      }
    });
  }

  /**
   * توليد الرابط النهائي المخصص للمشتري
   */
  getLink(): string {
    return this.createdId ? `https://arboon.app/pay/${this.createdId}` : '';
  }

  /**
   * نسخ الرابط للحافظة (Clipboard)
   */
  copyLink() {
    const link = this.getLink();
    if (!link) return;

    navigator.clipboard.writeText(link).then(() => {
      // إشعار نجاح النسخ
      alert('تم نسخ الرابط! أرسله الآن للمشتري لإتمام الدفع.');
    }).catch(err => {
      console.error('فشل النسخ:', err);
    });
  }

  /**
   * إعادة تعيين الفورم لإنشاء رابط جديد
   */
  resetForm() {
    this.createdId = null;
    this.title = '';
    this.amount = null;
    this.description = '';
    this.conditions = '';
  }
}