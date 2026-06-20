class ApiConstants {
  // PROD: pass --dart-define=API_BASE_URL=https://<your-domain> (behind nginx) at build time.
  // The localhost default below is for LOCAL dev only and bypasses nginx.
  static const String baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://localhost:5016',
  );

  static const String register = '$baseUrl/api/auth/register';
  static const String login = '$baseUrl/api/auth/login';
  static const String profile = '$baseUrl/api/auth/profile';
  static const String salons = '$baseUrl/api/salons';
  static const String appointments = '$baseUrl/api/appointments';
  static const String myAppointments = '$baseUrl/api/appointments/mine';
  static const String slots = '$baseUrl/api/appointments/slots';
  static const String services = '$baseUrl/api/services';
  static const String artists = '$baseUrl/api/artists';
  static const String notifications = '$baseUrl/api/notifications';
  static const String catalogServices = '$baseUrl/api/catalog/services';
  static const String inventory = '$baseUrl/api/inventory';
  static const String marketplace = '$baseUrl/api/marketplace';

  // Admin endpoints
  static const String adminUsers = '$baseUrl/api/admin/users';
  static const String adminSalons = '$baseUrl/api/admin/salons';
  static const String adminStats = '$baseUrl/api/admin/stats';
  static const String artistSchedule = '$baseUrl/api/artist-schedule/my';
  static const String artistScheduleStats =
      '$baseUrl/api/artist-schedule/my/stats';

  // Dashboard
  static const String dashboardManager = '$baseUrl/api/dashboard/manager';
  static const String dashboardArtist = '$baseUrl/api/dashboard/artist';
  static const String dashboardClient = '$baseUrl/api/dashboard/client';
  static const String dashboardPlatform = '$baseUrl/api/dashboard/platform';

  // Favorites
  static const String favorites = '$baseUrl/api/me/favorites';
}
