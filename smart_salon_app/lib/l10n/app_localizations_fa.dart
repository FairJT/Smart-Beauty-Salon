// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Persian (`fa`).
class AppLocalizationsFa extends AppLocalizations {
  AppLocalizationsFa([String locale = 'fa']) : super(locale);

  @override
  String get appTitle => 'سالن هوشمند';

  @override
  String get login => 'ورود';

  @override
  String get register => 'ثبت‌نام';

  @override
  String get logout => 'خروج';

  @override
  String get mobileNumber => 'شماره موبایل';

  @override
  String get password => 'رمز عبور';

  @override
  String get confirmPassword => 'تکرار رمز عبور';

  @override
  String get forgotPassword => 'رمز عبور را فراموش کرده‌اید؟';

  @override
  String get otpCode => 'کد تأیید';

  @override
  String get sendOtp => 'ارسال کد';

  @override
  String get verifyOtp => 'تأیید کد';

  @override
  String get profile => 'پروفایل';

  @override
  String get editProfile => 'ویرایش پروفایل';

  @override
  String get salons => 'سالن‌ها';

  @override
  String get appointments => 'نوبت‌ها';

  @override
  String get notifications => 'اعلان‌ها';

  @override
  String get search => 'جستجو';

  @override
  String get booking => 'رزرو نوبت';

  @override
  String get cancelBooking => 'لغو نوبت';

  @override
  String get rateBooking => 'امتیازدهی';

  @override
  String get artistSchedule => 'برنامه کاری آرایشگر';

  @override
  String get adminDashboard => 'پنل مدیریت';

  @override
  String get artistManagement => 'مدیریت آرایشگران';

  @override
  String get errorNetwork => 'خطا در ارتباط با سرور';

  @override
  String get errorTimeout => 'زمان اتصال تمام شد. لطفاً دوباره تلاش کنید';

  @override
  String get errorServer => 'خطای داخلی سرور';

  @override
  String get errorSessionExpired => 'نشست منقضی شده است';

  @override
  String get errorInvalidCredentials => 'شماره موبایل یا رمز عبور اشتباه است';

  @override
  String get successLogin => 'با موفقیت وارد شدید';

  @override
  String get successRegister => 'ثبت‌نام با موفقیت انجام شد';

  @override
  String get successLogout => 'با موفقیت خارج شدید';

  @override
  String get successBooking => 'نوبت با موفقیت رزرو شد';

  @override
  String get successCancel => 'نوبت با موفقیت لغو شد';

  @override
  String get changePassword => 'تغییر رمز عبور';

  @override
  String get currentPassword => 'رمز عبور فعلی';

  @override
  String get newPassword => 'رمز عبور جدید';

  @override
  String get passwordChanged => 'رمز عبور با موفقیت تغییر کرد';

  @override
  String get mobileAlreadyRegistered =>
      'این شماره موبایل قبلاً ثبت‌نام شده است';

  @override
  String get noAppointments => 'نوبتی یافت نشد';

  @override
  String get noNotifications => 'اعلانی وجود ندارد';

  @override
  String get retry => 'تلاش مجدد';

  @override
  String get save => 'ذخیره';

  @override
  String get cancel => 'انصراف';

  @override
  String get confirm => 'تأیید';

  @override
  String get loading => 'در حال بارگذاری...';

  @override
  String get welcome => 'خوش آمدید';

  @override
  String get guestBooking => 'رزرو مهمان';

  @override
  String get searchSalon => 'جستجوی سالن...';

  @override
  String get rating => 'امتیاز';

  @override
  String get comment => 'نظر';

  @override
  String get duration => 'مدت زمان';

  @override
  String get price => 'قیمت';

  @override
  String get deposit => 'بیعانه';

  @override
  String get estimatedPrice => 'قیمت تخمینی';

  @override
  String get finalPrice => 'قیمت نهایی';

  @override
  String get statusPending => 'در انتظار تأیید';

  @override
  String get statusConfirmed => 'تأیید شده';

  @override
  String get statusInProgress => 'در حال انجام';

  @override
  String get statusCompleted => 'تکمیل شده';

  @override
  String get statusCancelled => 'لغو شده';

  @override
  String get statusNoShow => 'حاضر نشده';

  @override
  String get reminderSent => 'یادآوری ارسال شد';

  @override
  String get errorUnknown => 'خطای غیرمنتظره‌ای رخ داده است';

  @override
  String get statusCancelledByArtist => 'لغو توسط هنرمند';

  @override
  String get statusUnknown => 'نامشخص';

  @override
  String get mobileRequired => 'شماره موبایل الزامی است';

  @override
  String get mobileInvalid => 'شماره موبایل نامعتبر است';

  @override
  String get passwordRequired => 'رمز عبور الزامی است';

  @override
  String get passwordMinLength => 'رمز عبور حداقل ۸ کاراکتر باشد';

  @override
  String fieldRequired(String fieldName) => '$fieldName الزامی است';

  @override
  String get nationalCodeRequired => 'کد ملی الزامی است';

  @override
  String get nationalCodeInvalid => 'کد ملی باید ۱۰ رقم باشد';

  @override
  String get phoneInvalid => 'شماره تلفن نامعتبر است';

  @override
  String get restartApp => 'لطفاً برنامه را مجدداً راه‌اندازی کنید';

  @override
  String get daySat => 'شنبه';

  @override
  String get daySun => 'یکشنبه';

  @override
  String get dayMon => 'دوشنبه';

  @override
  String get dayTue => 'سه‌شنبه';

  @override
  String get dayWed => 'چهارشنبه';

  @override
  String get dayThu => 'پنج‌شنبه';

  @override
  String get dayFri => 'جمعه';
}
