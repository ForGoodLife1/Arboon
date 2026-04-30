import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { DisputeService } from '../../core/services/dispute.service';
import { AlertService } from '../../core/services/alert.service';
import { Dispute, DisputeMessage } from '../../core/models/dispute.interface';

@Component({
  selector: 'app-buyer-dispute',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './buyer-dispute.component.html'
})
export class BuyerDisputeComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private disputeService = inject(DisputeService);
  private alertService = inject(AlertService);

  dispute = signal<Dispute | null>(null);
  messages = signal<DisputeMessage[]>([]);
  newMessage = signal('');
  isLoading = signal(true);
  isSending = signal(false);
  selectedFile = signal<File | null>(null);
  isUploading = signal(false);

  buyerToken = '';

  ngOnInit() {
    const disputeId = this.route.snapshot.paramMap.get('id');
    this.buyerToken = this.route.snapshot.queryParamMap.get('token') || '';

    if (disputeId) {
      this.loadDisputeData(disputeId);
    }
  }

  loadDisputeData(id: string) {
    this.isLoading.set(true);
    this.disputeService.getBuyerDispute(id, this.buyerToken).subscribe({
      next: (res) => {
        if (res.success) {
          this.dispute.set(res.data);
          this.loadMessages(id);
        }
      },
      error: () => this.isLoading.set(false)
    });
  }

  loadMessages(id: string) {
    this.disputeService.getDisputeMessages(id, this.buyerToken).subscribe(res => {
      this.messages.set(res.data);
      this.isLoading.set(false);
    });
  }


  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile.set(file);
    }
  }

  async sendMessage() {
    const currentDispute = this.dispute();
    if (!currentDispute || (!this.newMessage().trim() && !this.selectedFile())) return;

    this.isSending.set(true);

    let attachment: { url: string, name: string } | undefined;

    // رفع الملف أولاً لو موجود
    if (this.selectedFile()) {
      this.isUploading.set(true);
      const uploadRes = await this.disputeService.uploadAttachment(this.selectedFile()!).toPromise();
      if (uploadRes?.success) {
        attachment = uploadRes.data;
      }
      this.isUploading.set(false);
    }

    this.disputeService.sendMessage(currentDispute.id, this.newMessage(), attachment, this.buyerToken).subscribe({
      next: (res) => {
        if (res.success) {
          const newMsg: DisputeMessage = {
            id: Date.now().toString(),
            dispute_id: currentDispute.id,
            sender_type: 'Buyer',
            message: this.newMessage(),
            attachment_url: attachment?.url,
            attachment_name: attachment?.name,
            created_at: new Date().toISOString()
          };
          this.messages.update(msgs => [...msgs, newMsg]);
          this.newMessage.set('');
          this.selectedFile.set(null);
        }
        this.isSending.set(false);
      },
      error: () => this.isSending.set(false)
    });

  }
}

