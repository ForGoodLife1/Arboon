import { Routes } from '@angular/router';

// استدعاء الشاشات (تأكد من مسار الملفات حسب مجلداتك)
import { CreateEscrowComponent } from './features/escrow/create-escrow/create-escrow.component';
import { PayEscrowComponent } from './features/checkout/pay-escrow/pay-escrow.component';
import { DashboardComponent } from './features/dashboard/dashboard.component'; // 👈 ضفنا الداشبورد اللي لسه عاملينه
import { EscrowDetailsComponent } from './features/escrow-details/escrow-details.component';
import { MainLayoutComponent } from './shared/layouts/main-layout/main-layout.component';
import { WalletComponent } from './features/wallet/wallet.component';
import { DisputeCenterComponent } from './features/dispute-center/dispute-center.component';

import { BuyerDisputeComponent } from './features/dispute-center/buyer-dispute.component';

export const routes: Routes = [
  // 1. مسارات المشتري (مستقلة تماماً وبدون السايد بار)
  { path: 'pay/:id', component: PayEscrowComponent },
  { path: 'dispute/:id', component: BuyerDisputeComponent },


  // 2. المسارات المحمية اللي جواها السايد بار (للمستقل)
  {
    path: '',
    component: MainLayoutComponent, // القالب الرئيسي هو الأب
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'wallet', component: WalletComponent },
      { path: 'dispute-center', component: DisputeCenterComponent },
      { path: 'create', component: CreateEscrowComponent },
      { path: 'escrow-details/:id', component: EscrowDetailsComponent },

      // التوجيه الافتراضي لو دخل على الموقع مباشرة
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },

  // 3. مسار الخطأ (404)
  { path: '**', redirectTo: 'dashboard' }
];