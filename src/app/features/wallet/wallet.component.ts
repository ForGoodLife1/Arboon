import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Transaction, WalletStats, WithdrawalPayload } from '../../core/models/escrow.interface';
import { EscrowService } from '../../core/services/escrow.service';
import { AlertService } from '../../core/services/alert.service';

@Component({
  selector: 'app-wallet',
  standalone: true,
  imports: [CommonModule, FormsModule], // FormsModule ضروري هنا
  templateUrl: './wallet.component.html'
})
export class WalletComponent implements OnInit {
  private escrowService = inject(EscrowService);
  private alertService = inject(AlertService);

  // حالة البيانات
  stats = signal<WalletStats | null>(null);
  transactions = signal<Transaction[]>([]);
  isLoading = signal<boolean>(true);

  // حالة نافذة السحب (Modal)
  isWithdrawModalOpen = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);

  // بيانات فورم السحب
  withdrawAmount: number | null = null;
  withdrawMethod: 'INSTAPAY' | 'BANK_TRANSFER' | 'WALLET' = 'INSTAPAY';
  accountDetails: string = '';

  ngOnInit() {
    this.loadWallet();
  }

  loadWallet() {
    this.isLoading.set(true);
    this.escrowService.getWalletData().subscribe({
      next: (res) => {
        if (res.success) {
          this.stats.set(res.data.stats);
          this.transactions.set(res.data.transactions);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading wallet:', err);
        this.isLoading.set(false);
      }
    });
  }

  openWithdrawModal() {
    this.isWithdrawModalOpen.set(true);
  }

  closeWithdrawModal() {
    this.isWithdrawModalOpen.set(false);
    this.resetForm();
  }

  submitWithdrawal() {
    if (!this.withdrawAmount || !this.accountDetails) {
      this.alertService.error('يرجى إدخال المبلغ وتفاصيل الحساب.');
      return;
    }

    const maxAvailable = this.stats()?.available || 0;
    if (this.withdrawAmount > maxAvailable) {
      this.alertService.error('المبلغ المطلوب يتجاوز الرصيد المتاح للسحب!');
      return;
    }

    this.isSubmitting.set(true);

    const payload: WithdrawalPayload = {
      amount: Number(this.withdrawAmount),
      method: this.withdrawMethod,
      account_details: this.accountDetails
    };

    this.escrowService.requestWithdrawal(payload).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        if (res.success) {
          this.alertService.success('تم تقديم طلب السحب بنجاح. سيتم مراجعته قريباً.');
          this.closeWithdrawModal();
          this.loadWallet(); // تحديث البيانات بعد السحب
        }
      },
      error: (err) => {
        console.error('Withdrawal error:', err);
        this.isSubmitting.set(false);
      }
    });
  }

  resetForm() {
    this.withdrawAmount = null;
    this.withdrawMethod = 'INSTAPAY';
    this.accountDetails = '';
  }
}