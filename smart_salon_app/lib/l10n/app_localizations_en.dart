// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appTitle => 'Smart Salon';

  @override
  String get login => 'Login';

  @override
  String get register => 'Register';

  @override
  String get logout => 'Logout';

  @override
  String get mobileNumber => 'Mobile Number';

  @override
  String get password => 'Password';

  @override
  String get confirmPassword => 'Confirm Password';

  @override
  String get forgotPassword => 'Forgot Password?';

  @override
  String get otpCode => 'Verification Code';

  @override
  String get sendOtp => 'Send Code';

  @override
  String get verifyOtp => 'Verify Code';

  @override
  String get profile => 'Profile';

  @override
  String get editProfile => 'Edit Profile';

  @override
  String get salons => 'Salons';

  @override
  String get appointments => 'Appointments';

  @override
  String get notifications => 'Notifications';

  @override
  String get search => 'Search';

  @override
  String get booking => 'Book Appointment';

  @override
  String get cancelBooking => 'Cancel Appointment';

  @override
  String get rateBooking => 'Rate Appointment';

  @override
  String get artistSchedule => 'Artist Schedule';

  @override
  String get adminDashboard => 'Admin Dashboard';

  @override
  String get artistManagement => 'Artist Management';

  @override
  String get errorNetwork => 'Connection error. Please try again.';

  @override
  String get errorTimeout => 'Connection timed out. Please try again.';

  @override
  String get errorServer => 'Internal server error.';

  @override
  String get errorSessionExpired => 'Session expired.';

  @override
  String get errorInvalidCredentials => 'Invalid mobile number or password.';

  @override
  String get errorUnknown => 'An unexpected error occurred';

  @override
  String get successLogin => 'Logged in successfully.';

  @override
  String get successRegister => 'Registered successfully.';

  @override
  String get successLogout => 'Logged out successfully.';

  @override
  String get successBooking => 'Appointment booked successfully.';

  @override
  String get successCancel => 'Appointment cancelled successfully.';

  @override
  String get changePassword => 'Change Password';

  @override
  String get currentPassword => 'Current Password';

  @override
  String get newPassword => 'New Password';

  @override
  String get passwordChanged => 'Password changed successfully.';

  @override
  String get mobileAlreadyRegistered =>
      'This mobile number is already registered.';

  @override
  String get noAppointments => 'No appointments found.';

  @override
  String get noNotifications => 'No notifications.';

  @override
  String get retry => 'Retry';

  @override
  String get save => 'Save';

  @override
  String get cancel => 'Cancel';

  @override
  String get confirm => 'Confirm';

  @override
  String get loading => 'Loading...';

  @override
  String get welcome => 'Welcome';

  @override
  String get guestBooking => 'Guest Booking';

  @override
  String get searchSalon => 'Search salon...';

  @override
  String get rating => 'Rating';

  @override
  String get comment => 'Comment';

  @override
  String get duration => 'Duration';

  @override
  String get price => 'Price';

  @override
  String get deposit => 'Deposit';

  @override
  String get estimatedPrice => 'Estimated Price';

  @override
  String get finalPrice => 'Final Price';

  @override
  String get statusPending => 'Pending';

  @override
  String get statusConfirmed => 'Confirmed';

  @override
  String get statusInProgress => 'In Progress';

  @override
  String get statusCompleted => 'Completed';

  @override
  String get statusCancelled => 'Cancelled';

  @override
  String get statusNoShow => 'No Show';

  @override
  String get statusCancelledByArtist => 'Cancelled by Artist';

  @override
  String get statusUnknown => 'Unknown';

  @override
  String get mobileRequired => 'Mobile number is required';

  @override
  String get mobileInvalid => 'Invalid mobile number';

  @override
  String get passwordRequired => 'Password is required';

  @override
  String get passwordMinLength => 'Password must be at least 8 characters';

  @override
  String fieldRequired(Object fieldName) {
    return '$fieldName is required';
  }

  @override
  String get nationalCodeRequired => 'National code is required';

  @override
  String get nationalCodeInvalid => 'National code must be 10 digits';

  @override
  String get phoneInvalid => 'Invalid phone number';

  @override
  String get restartApp => 'Please restart the app';

  @override
  String get daySat => 'Saturday';

  @override
  String get daySun => 'Sunday';

  @override
  String get dayMon => 'Monday';

  @override
  String get dayTue => 'Tuesday';

  @override
  String get dayWed => 'Wednesday';

  @override
  String get dayThu => 'Thursday';

  @override
  String get dayFri => 'Friday';

  @override
  String get reminderSent => 'Reminder Sent';
}
