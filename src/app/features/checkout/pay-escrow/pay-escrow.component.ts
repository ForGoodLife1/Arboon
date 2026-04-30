import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { EscrowService } from '../../../core/services/escrow.service';
import { Escrow, PaymentPayload, PaymentMethod } from '../../../core/models/escrow.interface';

@Component({
  selector: 'app-pay-escrow',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pay-escrow.component.html'
})
export class PayEscrowComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private escrowService = inject(EscrowService);

  // استخدام Signals عشان الـ HTML بتاعك
  escrow = signal<Escrow | null>(null);
  isLoading = signal<boolean>(true);
  isProcessing = signal<boolean>(false);

  // متغيرات الدفع
  selectedMethod: string = 'card';
  buyerEmail: string = '';
  cardName: string = '';
  cardNumber: string = '';
  expiryDate: string = '';
  cvv: string = '';
  ipaAddress: string = '';
  walletNumber: string = '';
  paypalEmail: string = '';

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.escrowService.getEscrow(id).subscribe({
        next: (res) => {
          if (res.success) {
            this.escrow.set(res.data);
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      });
    } else {
      this.isLoading.set(false);
    }
  }

  selectMethod(method: string) {
    this.selectedMethod = method;
  }

  simulatePayment() {
    const currentEscrow = this.escrow();
    if (!currentEscrow || !this.buyerEmail) return;

    this.isProcessing.set(true);

    const payload: PaymentPayload = {
      // بنحول 'card' لـ 'CARD' عشان تطابق الـ Interface
      payment_method: this.selectedMethod.toUpperCase() as PaymentMethod, 
      buyer_email: this.buyerEmail
    };

    this.escrowService.payEscrow(currentEscrow.id, payload).subscribe({
      next: (res) => {
        this.isProcessing.set(false);
        if (res.success) {
          // تحديث الـ Signal
          this.escrow.update(e => e ? { ...e, status: 'FROZEN' } : null);
        }
      },
      error: (err) => {
        this.isProcessing.set(false);
      }
    });
  }
  // دالة تحرير الأموال (بيتم استدعاؤها من شاشة التجميد)
  release() {
    const currentEscrow = this.escrow();
    if (!currentEscrow) return;

    // رسالة التأكيد للمشتري
    const confirmed = window.confirm('هل أنت متأكد من استلام العمل وتحرير الأموال للمستقل؟ لا يمكن التراجع عن هذا الإجراء.');
    
    if (confirmed) {
      this.isProcessing.set(true); // تشغيل السبينر

      this.escrowService.releaseEscrow(currentEscrow.id).subscribe({
        next: (res) => {
          this.isProcessing.set(false); // إيقاف السبينر
          if (res.success) {
            // تحديث الـ Signal لتغيير الشاشة فوراً إلى RELEASED (شاشة النجاح)
            this.escrow.update(e => e ? { ...e, status: 'RELEASED' } : null);
          }
        },
        error: (err) => {
          console.error('Release Error:', err);
          this.isProcessing.set(false);
          alert('حدث خطأ أثناء تحرير الأموال، يرجى المحاولة مرة أخرى.');
        }
      });
    }
  }
}