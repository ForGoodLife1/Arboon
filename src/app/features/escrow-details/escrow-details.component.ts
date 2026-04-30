import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { EscrowService } from '../../core/services/escrow.service';
import { Escrow } from '../../core/models/escrow.interface';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-escrow-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './escrow-details.component.html'
})
export class EscrowDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private escrowService = inject(EscrowService);
  
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

  // 👈 الدالة اللي كانت ناقصة
  copyLink() {
    const currentEscrow = this.escrow();
    if (!currentEscrow) return;

    // تجميع الرابط الكامل
    const link = `https://arboon.app/pay/${currentEscrow.id}`;

    // النسخ إلى الحافظة (Clipboard)
    navigator.clipboard.writeText(link).then(() => {
      alert('تم نسخ رابط الدفع بنجاح! يمكنك إرساله للمشتري.');
    }).catch(err => {
      console.error('فشل في نسخ الرابط:', err);
    });
  }
}