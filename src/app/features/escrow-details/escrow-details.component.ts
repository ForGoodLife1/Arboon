import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { EscrowService } from '../../core/services/escrow.service';
import { Escrow } from '../../core/models/escrow.interface';
import { CommonModule } from '@angular/common';
import { AlertService } from '../../core/services/alert.service';
import { DisputeService } from '../../core/services/dispute.service';

@Component({
  selector: 'app-escrow-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './escrow-details.component.html'
})
export class EscrowDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private escrowService = inject(EscrowService);
  private disputeService = inject(DisputeService);
  private alertService = inject(AlertService);
  
  escrow = signal<Escrow | null>(null);
  isLoading = signal(true);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.escrowService.getEscrow(id).subscribe({
        next: (res) => {
          this.escrow.set(res.data);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Error fetching escrow details:', err);
          this.isLoading.set(false);
        }
      });
    }
  }

  // 👈 دالة نسخ الرابط
  copyLink() {
    const currentEscrow = this.escrow();
    if (!currentEscrow) return;

    // تجميع الرابط الكامل
    const link = `https://arboon.app/pay/${currentEscrow.id}`;

    // النسخ إلى الحافظة (Clipboard)
    navigator.clipboard.writeText(link).then(() => {
      this.alertService.success('تم نسخ رابط الدفع بنجاح! يمكنك إرساله للمشتري.');
    }).catch(err => {
      console.error('فشل في نسخ الرابط:', err);
    });
  }

  // 👈 دالة فتح نزاع جديد
  async openDispute() {
    const currentEscrow = this.escrow();
    if (!currentEscrow) return;

    const reason = await this.alertService.input(
      'فتح نزاع جديد',
      'يرجى كتابة سبب النزاع بالتفصيل ليتمكن فريق الإدارة من مراجعته...',
      'تأكيد فتح النزاع'
    );

    if (reason) {
      this.isLoading.set(true); // تشغيل لودينج بسيط
      
      this.disputeService.openDispute({
        escrow_id: currentEscrow.id,
        reason: reason
      }).subscribe({
        next: (res) => {
          if (res.success) {
            this.alertService.success('تم فتح النزاع بنجاح. سيتم توجيهك لمركز النزاعات.');
            this.router.navigate(['/dispute-center']);
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
          console.error('Dispute Error:', err);
        }
      });
    }
  }
}