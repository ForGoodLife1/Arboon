import { Component, inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { EscrowService } from '../../../core/services/escrow.service';
import { CreateEscrowPayload } from '../../../core/models/escrow.interface';
import { AlertService } from '../../../core/services/alert.service';

@Component({
  selector: 'app-create-escrow',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './create-escrow.component.html',
  styleUrls: ['./create-escrow.component.css']
})
export class CreateEscrowComponent {
  private escrowService = inject(EscrowService);
  private alertService = inject(AlertService);
  private router = inject(Router);
  private platformId = inject(PLATFORM_ID);

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
  createdPaymentUrl: string | null = null;

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
      this.alertService.error('يرجى إكمال البيانات الأساسية: العنوان، المبلغ، ووصف الخدمة.');
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
        this.isLoading = false;
        if (response.success) {
          this.createdId = response.data.id;
          this.createdPaymentUrl = response.data.payment_url || null;
          this.alertService.success('تم إنشاء رابط العُهدة بنجاح!');
          console.log('تم إنشاء العُهدة بنجاح، المعرف:', this.createdId);
          
          // تأخير بسيط للانتقال لضمان رؤية رسالة النجاح وتجربة سلسة
          setTimeout(() => {
            this.router.navigate(['/escrow-details', this.createdId]);
          }, 1500);

          // التمرير لأعلى بسلاسة لتجنب القفز المفاجئ (Scroll Jump)
          if (isPlatformBrowser(this.platformId)) {
            window.scrollTo({ top: 0, behavior: 'smooth' });
          }
        }
      },
      error: (err) => {
        console.error('فشل في إنشاء الرابط:', err);
        this.isLoading = false;
        // ملاحظة: الانترسيبتور هو اللي هيعرض رسالة الخطأ للمستخدم
      }
    });
  }

  /**
   * توليد الرابط النهائي المخصص للمشتري
   */
  getLink(): string {
    const origin = isPlatformBrowser(this.platformId) ? window.location.origin : '';
    return this.createdPaymentUrl || (this.createdId ? `${origin}/pay/${this.createdId}` : '');
  }

  /**
   * نسخ الرابط للحافظة (Clipboard)
   */
  copyLink() {
    const link = this.getLink();
    // النسخ إلى الحافظة (Clipboard)
    if (isPlatformBrowser(this.platformId) && navigator.clipboard) {
      navigator.clipboard.writeText(link).then(() => {
        // إشعار نجاح النسخ
        this.alertService.success('تم نسخ الرابط! أرسله الآن للمشتري لإتمام الدفع.');
      }).catch(err => {
        console.error('فشل النسخ:', err);
      });
    }
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