import { Routes } from '@angular/router';

export const routes: Routes = [
  // 1. مسارات المشتري (مستقلة تماماً وبدون السايد بار)
  {
    path: 'pay/:id',
    loadComponent: () => import('./features/checkout/pay-escrow/pay-escrow.component').then(c => c.PayEscrowComponent)
  },
  {
    path: 'dispute/:id',
    loadComponent: () => import('./features/dispute-center/buyer-dispute.component').then(c => c.BuyerDisputeComponent)
  },

  // 2. المسارات المحمية اللي جواها السايد بار (للمستقل)
  {
    path: '',
    loadComponent: () => import('./shared/layouts/main-layout/main-layout.component').then(c => c.MainLayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(c => c.DashboardComponent)
      },
      {
        path: 'wallet',
        loadComponent: () => import('./features/wallet/wallet.component').then(c => c.WalletComponent)
      },
      {
        path: 'dispute-center',
        loadComponent: () => import('./features/dispute-center/dispute-center.component').then(c => c.DisputeCenterComponent)
      },
      {
        path: 'create',
        loadComponent: () => import('./features/escrow/create-escrow/create-escrow.component').then(c => c.CreateEscrowComponent)
      },
      {
        path: 'escrow-details/:id',
        loadComponent: () => import('./features/escrow-details/escrow-details.component').then(c => c.EscrowDetailsComponent)
      },

      // التوجيه الافتراضي لو دخل على الموقع مباشرة
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },

  // 3. مسار الخطأ (404)
  { path: '**', redirectTo: 'dashboard' }
];

