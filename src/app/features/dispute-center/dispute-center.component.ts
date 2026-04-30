import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Dispute, DisputeMessage } from '../../core/models/dispute.interface';
import { DisputeService } from '../../core/services/dispute.service';
import { AlertService } from '../../core/services/alert.service';

@Component({
  selector: 'app-dispute-center',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dispute-center.component.html'
})
export class DisputeCenterComponent implements OnInit {
  private disputeService = inject(DisputeService);
  private alertService = inject(AlertService);

  disputes = signal<Dispute[]>([]);
  selectedDispute = signal<Dispute | null>(null);
  messages = signal<DisputeMessage[]>([]);

  isLoading = signal<boolean>(true);
  newMessage = signal<string>('');
  isSending = signal<boolean>(false);
  selectedFile = signal<File | null>(null);

  ngOnInit() {
    this.loadDisputes();
  }

  loadDisputes() {
    this.isLoading.set(true);
    this.disputeService.getSellerDisputes().subscribe({
      next: (res) => {
        if (res.success) {
          this.disputes.set(res.data);
          // اختيار أول نزاع تلقائياً لو موجود
          if (res.data.length > 0) {
            this.selectDispute(res.data[0]);
          }
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading disputes:', err);
        this.isLoading.set(false);
        // التنبيه بيظهر تلقائياً من الانترسيبتور لو فيه خطأ HTTP
      }
    });
  }


  selectDispute(dispute: Dispute) {
    this.selectedDispute.set(dispute);
    this.loadMessages(dispute.id);
  }

  loadMessages(disputeId: string) {
    this.disputeService.getDisputeMessages(disputeId).subscribe({
      next: (res) => {
        if (res.success) {
          this.messages.set(res.data);
        }
      }
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile.set(file);
    }
  }

  async sendMessage() {
    const currentDispute = this.selectedDispute();
    const msgText = this.newMessage().trim();

    if (!currentDispute) {
      this.alertService.error('يرجى اختيار نزاع أولاً.');
      return;
    }
    
    if (!msgText && !this.selectedFile()) return;

    this.isSending.set(true);

    let attachment: { url: string, name: string } | undefined;

    if (this.selectedFile()) {
      const uploadRes = await this.disputeService.uploadAttachment(this.selectedFile()!).toPromise();
      if (uploadRes?.success) {
        attachment = uploadRes.data;
      }
    }

    this.disputeService.sendMessage(currentDispute.id, msgText, attachment).subscribe({
      next: (res) => {
        this.isSending.set(false);
        if (res.success) {
          // تحديث الشات محلياً بدون ريفريش
          const newMsg: DisputeMessage = {
            id: Date.now().toString(),
            dispute_id: currentDispute.id,
            sender_type: 'Seller',
            message: msgText,
            attachment_url: attachment?.url,
            attachment_name: attachment?.name,
            created_at: new Date().toISOString()
          };
          this.messages.update(msgs => [...msgs, newMsg]);
          this.newMessage.set('');
          this.selectedFile.set(null);
        }
      },
      error: () => {
        this.isSending.set(false);
      }
    });
  }
}