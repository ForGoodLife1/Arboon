import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LayoutService } from '../../../core/services/layout.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule], // RouterModule مهم جداً عشان الروابط تشتغل
  templateUrl: './sidebar.component.html'
})
export class SidebarComponent {
layoutService = inject(LayoutService);}