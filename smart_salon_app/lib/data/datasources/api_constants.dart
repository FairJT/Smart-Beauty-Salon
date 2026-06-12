class ApiConstants {
  static const String baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://localhost:5015/api',
  );

  static const String register = '$baseUrl/auth/register';
  static const String login = '$baseUrl/auth/login';
  static const String profile = '$baseUrl/auth/profile';
  static const String salons = '$baseUrl/salons';
  static const String appointments = '$baseUrl/appointments';
  static const String myAppointments = '$baseUrl/appointments/mine';
  static const String slots = '$baseUrl/appointments/slots';
  static const String services = '$baseUrl/services';
  static const String artists = '$baseUrl/artists';
  static const String notifications = '$baseUrl/notifications';
  static const String catalogServices = '$baseUrl/catalog/services';
  static const String inventory = '$baseUrl/inventory';
  static const String marketplace = '$baseUrl/marketplace';

  // Admin endpoints
  static const String adminUsers = '$baseUrl/admin/users';
  static const String adminSalons = '$baseUrl/admin/salons';
  static const String adminStats = '$baseUrl/admin/stats';
  static const String artistSchedule = '$baseUrl/artist-schedule/my';
  static const String artistScheduleStats = '$baseUrl/artist-schedule/my/stats';
}