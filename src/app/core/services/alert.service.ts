import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';

@Injectable({
  providedIn: 'root'
})
export class AlertService {

  // 1. إعدادات الـ Toast (للتنبيهات السريعة اللي بتظهر وتختفي لوحدها)
  private toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true,
    background: '#12181A', // لون الخلفية الدارك بتاع المنصة
    color: '#fff',
    customClass: {
      popup: 'border border-white/5 rounded-xl shadow-2xl backdrop-blur-md'
    }
  });

  // دالة النجاح السريعة
  success(message: string) {
    this.toast.fire({
      icon: 'success',
      title: message,
      iconColor: '#10B981' // Emerald 500
    });
  }

  // دالة الخطأ السريعة
  error(message: string) {
    this.toast.fire({
      icon: 'error',
      title: message,
      iconColor: '#EF4444' // Red 500
    });
  }

  // 2. دالة رسائل التأكيد (Confirm) للعمليات الحساسة زي "تحرير الأموال"
  async confirm(title: string, text: string, confirmText: string = 'تأكيد'): Promise<boolean> {
    const result = await Swal.fire({
      title: title,
      text: text,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#10B981', // لون عُربون الأساسي
      cancelButtonColor: '#27272A',  // لون محايد للإلغاء
      confirmButtonText: confirmText,
      cancelButtonText: 'إلغاء',
      background: '#12181A',
      color: '#ffffff',
      customClass: {
        popup: 'border border-white/10 rounded-3xl',
        confirmButton: 'rounded-xl font-bold px-6 py-3',
        cancelButton: 'rounded-xl font-bold px-6 py-3'
      }
    });

    return result.isConfirmed;
  }

  // 3. دالة لإدخال نص (Prompt) زي سبب النزاع
  async input(title: string, placeholder: string, confirmText: string = 'إرسال'): Promise<string | null> {
    const { value: text } = await Swal.fire({
      title: title,
      input: 'textarea',
      inputPlaceholder: placeholder,
      showCancelButton: true,
      confirmButtonColor: '#10B981',
      cancelButtonColor: '#27272A',
      confirmButtonText: confirmText,
      cancelButtonText: 'إلغاء',
      background: '#12181A',
      color: '#ffffff',
      customClass: {
        popup: 'border border-white/10 rounded-3xl',
        input: 'bg-[#0B0F10] border-white/10 text-white rounded-xl focus:ring-emerald-500',
        confirmButton: 'rounded-xl font-bold px-6 py-3',
        cancelButton: 'rounded-xl font-bold px-6 py-3'
      },
      inputValidator: (value) => {
        if (!value) {
          return 'يرجى كتابة السبب للمتابعة';
        }
        return null;
      }
    });

    return text || null;
  }
}