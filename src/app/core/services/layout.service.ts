import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LayoutService {
  // حالة المنيو على الموبايل (افتراضياً مقفول)
  isMobileMenuOpen = signal<boolean>(false);

  toggleMenu() {
    this.isMobileMenuOpen.update(isOpen => !isOpen);
  }

  closeMenu() {
    this.isMobileMenuOpen.set(false);
  }
}