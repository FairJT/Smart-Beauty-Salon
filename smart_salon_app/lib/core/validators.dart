class Validators {
  static String? mobile(String? value) {
    if (value == null || value.isEmpty) return 'شماره موبایل الزامی است';
    if (!RegExp(r'^09\d{9}$').hasMatch(value)) return 'شماره موبایل نامعتبر است';
    return null;
  }

  static String? password(String? value) {
    if (value == null || value.isEmpty) return 'رمز عبور الزامی است';
    if (value.length < 8) return 'رمز عبور حداقل ۸ کاراکتر باشد';
    return null;
  }

  static String? required(String? value, String fieldName) {
    if (value == null || value.trim().isEmpty) return '$fieldName الزامی است';
    return null;
  }

  static String? nationalCode(String? value) {
    if (value == null || value.isEmpty) return 'کد ملی الزامی است';
    if (!RegExp(r'^\d{10}$').hasMatch(value)) return 'کد ملی باید ۱۰ رقم باشد';
    return null;
  }

  static String? phone(String? value) {
    if (value == null || value.isEmpty) return null;
    if (!RegExp(r'^09\d{9}$').hasMatch(value)) return 'شماره تلفن نامعتبر است';
    return null;
  }
}
