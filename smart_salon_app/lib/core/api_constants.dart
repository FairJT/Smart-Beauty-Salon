class ApiConstants {
  static const String baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://5.202.45.40:8080/api',
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
}
