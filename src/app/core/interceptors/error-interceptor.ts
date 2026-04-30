import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AlertService } from '../services/alert.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  // حقن الخدمات اللي هنحتاجها
  const alertService = inject(AlertService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'حدث خطأ غير متوقع، يرجى المحاولة لاحقاً.';

      // 1. أخطاء من جهة العميل (Client-side) زي مشكلة في المتصفح أو قطع النت
      if (error.error instanceof ErrorEvent) {
        errorMessage = `خطأ في النظام: ${error.error.message}`;
      }
      // 2. أخطاء من جهة السيرفر (Server-side)
      else {
        switch (error.status) {
          case 0:
            errorMessage = 'لا يوجد اتصال بالإنترنت، أو السيرفر لا يستجيب.';
            break;
          case 400:
            errorMessage = error.error?.message || 'البيانات المرسلة غير صحيحة.';
            break;
          case 401:
            // 401 تعني أن المستخدم غير مسجل دخول أو التوكن انتهى
            errorMessage = 'انتهت صلاحية الجلسة، يرجى إعادة الدخول.';
            localStorage.removeItem('token'); // مسح التوكن القديم
            router.navigate(['/dashboard']); // توجيه لصفحة الرئيسية (الداشبورد حالياً)
            break;
          case 403:
            errorMessage = 'عفواً، لا تملك الصلاحيات اللازمة لإتمام هذه العملية.';
            break;
          case 404:
            errorMessage = 'الرابط أو المورد المطلوب غير موجود (404).';
            break;
          case 500:
            errorMessage = 'حدث عطل داخلي في الخادم، فريقنا يعمل على حله.';
            break;
          default:
            // لو الباك إند باعت رسالة مخصصة نعرضها، غير كده نعرض الكود
            errorMessage = error.error?.message || `حدث خطأ برمز: ${error.status}`;
            break;
        }
      }

      // عرض رسالة الخطأ باستخدام الـ SweetAlert Service المركزية
      alertService.error(errorMessage);

      // تمرير الخطأ عشان لو الكومبوننت حابب يعمل حاجة إضافية (زي إيقاف الـ Loading)
      return throwError(() => error);
    })
  );
};