import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EscrowService } from '../../core/services/escrow.service';
import { Escrow } from '../../core/models/escrow.interface';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  private escrowService = inject(EscrowService);

  // Signals لإدارة حالة البيانات
  frozenTotal = signal<number>(0);
  completedCount = signal<number>(0);
  pendingCount = signal<number>(0);
  activeEscrows = signal<Escrow[]>([]);
  
  // حالة البحث والتصفية
  searchTerm = signal<string>('');
  statusFilter = signal<string>('ALL');

  // الـ Signal المحسوب للتصفية التلقائية
  filteredEscrows = computed(() => {
    const term = this.searchTerm().toLowerCase();
    const status = this.statusFilter();
    
    return this.activeEscrows().filter(escrow => {
      const matchesSearch = 
        escrow.title.toLowerCase().includes(term) || 
        escrow.id.toLowerCase().includes(term);
      
      const matchesStatus = status === 'ALL' || escrow.status === status;
      
      return matchesSearch && matchesStatus;
    });
  });

  // حالة الواجهة
  isLoading = signal<boolean>(true);
  showToast = signal<boolean>(true);


  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.escrowService.getDashboardData().subscribe({
      next: (res) => {
        if (res.success) {
          this.frozenTotal.set(res.data.stats.frozen_total);
          this.completedCount.set(res.data.stats.completed_count);
          this.pendingCount.set(res.data.stats.pending_count);
          this.activeEscrows.set(res.data.recent_escrows);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error:', err);
        this.isLoading.set(false);
      }
    });
  }

  // أيقونات الحالة
  getIcon(escrow: Escrow): string {
    switch (escrow.status) {
      case 'FROZEN': return 'lock_clock';
      case 'PENDING': return 'hourglass_empty';
      case 'RELEASED': return 'verified';
      default: return 'receipt_long';
    }
  }

  // تسميات الحالة بالعربي
  getStatusLabel(status: string): string {
    switch(status) {
      case 'PENDING': return 'بانتظار الدفع';
      case 'FROZEN': return 'أموال مؤمنة';
      case 'RELEASED': return 'تم التسليم';
      default: return 'ملغاة';
    }
  }

  closeToast() {
    this.showToast.set(false);
  }
}