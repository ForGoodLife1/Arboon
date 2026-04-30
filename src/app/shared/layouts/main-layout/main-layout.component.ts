import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SidebarComponent } from '../../components/sidebar/sidebar.component';
import { LayoutService } from '../../../core/services/layout.service';
import { NavbarComponent } from "../../components/navbar/navbar.component";

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent, NavbarComponent],
  templateUrl: './main-layout.component.html'
})
export class MainLayoutComponent {
layoutService = inject(LayoutService);}