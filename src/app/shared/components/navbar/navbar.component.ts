import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LayoutService } from '../../../core/services/layout.service';
import { EscrowService } from '../../../core/services/escrow.service'; 

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html'
})
export class NavbarComponent implements OnInit {
  layoutService = inject(LayoutService);
  private escrowService = inject(EscrowService); // 👈 حقن السيرفيس

  isDarkMode = signal<boolean>(true);
  isLoggedIn = signal<boolean>(true);
  
  // المتغيرات الجديدة للرصيد والإشعارات 👈
  availableBalance = signal<number>(0);
  hasUnreadNotifications = signal<boolean>(true); 

  ngOnInit() {
    // لو مسجل دخول، هات رصيده من الباك إند
    if (this.isLoggedIn()) {
      this.fetchWalletBalance();
    }
  }

  fetchWalletBalance() {
    this.escrowService.getWalletData().subscribe({
      next: (res) => {
        if (res.success) {
          // تحديث الرصيد بالرقم الفعلي من المحفظة
          this.availableBalance.set(res.data.stats.available);
        }
      }
    });
  }

  toggleTheme() {
    this.isDarkMode.update(mode => !mode);
  }

  logoutForDemo() {
    alert('تم تسجيل الخروج مؤقتاً للتجربة!');
    this.isLoggedIn.set(false);
  }
  // حالة قائمة الإشعارات
  isNotificationsOpen = signal<boolean>(false);

  // داتا وهمية للإشعارات
  notifications = signal([
    { id: 1, text: 'تم تجميد أموال العُهدة ARB-8a2b9c', time: 'منذ 10 دقائق', read: false },
    { id: 2, text: 'رسالة جديدة من المشتري بخصوص مشروع التطبيق', time: 'منذ ساعة', read: true }
  ]);

  // دالة الفتح والقفل
  toggleNotifications() {
    this.isNotificationsOpen.update(v => !v);
  }
}