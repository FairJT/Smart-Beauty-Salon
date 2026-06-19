import 'package:shared_preferences/shared_preferences.dart';
import '../../domain/entities/user_entity.dart';
import '../../domain/repositories/auth_repository.dart';
import '../datasources/dio_client.dart';
import '../datasources/api_constants.dart';

class AuthRepositoryImpl implements AuthRepository {
  // Using SharedPreferences for token storage
  // Placeholder not needed; will retrieve instance as needed
  static const _tokenKey = 'auth_token';

  static int _parseUserType(dynamic value) {
    if (value is int) return value;
    if (value is String) {
      switch (value) {
        case 'SuperAdmin':
          return 1;
        case 'SalonManager':
          return 2;
        case 'Artist':
          return 3;
        case 'Client':
          return 4;
        default:
          final parsed = int.tryParse(value);
          return parsed ?? 4;
      }
    }
    return 4; // default to Client
  }

  @override
  Future<UserEntity> login(String phoneNumber, String password) async {
    final response = await DioClient.instance.post(
      ApiConstants.login,
      data: {
        'mobile': phoneNumber,
        'password': password,
      },
    );

    final data = response.data;
    await _saveToken(data['token']);

    return UserEntity(
      id: data['user']['id']?.toString() ?? '',
      phoneNumber: data['user']['mobile'],
      firstName: data['user']['firstName'],
      lastName: data['user']['lastName'],
      userType: _parseUserType(data['user']['userType']),
    );
  }

  @override
  Future<UserEntity> register(String phoneNumber, String password,
      String firstName, String lastName, String nationalCode) async {
    final response = await DioClient.instance.post(
      ApiConstants.register,
      data: {
        'mobile': phoneNumber,
        'password': password,
        'firstName': firstName,
        'lastName': lastName,
        'nationalCode': nationalCode,
      },
    );

    final data = response.data;
    await _saveToken(data['token']);

    return UserEntity(
      id: data['user']['id']?.toString() ?? '',
      phoneNumber: data['user']['mobile'],
      firstName: data['user']['firstName'],
      lastName: data['user']['lastName'],
      userType: _parseUserType(data['user']['userType']),
    );
  }

  @override
  Future<UserEntity> getProfile() async {
    final response = await DioClient.instance.get(ApiConstants.profile);
    final data = response.data;

    return UserEntity(
      id: data['id']?.toString() ?? '',
      phoneNumber: data['mobile'],
      firstName: data['firstName'],
      lastName: data['lastName'],
      userType: _parseUserType(data['userType']),
    );
  }

  @override
  Future<void> logout() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenKey);
  }

  @override
  Future<bool> isLoggedIn() async {
    final token = await getToken();
    return token != null;
  }

  @override
  Future<String?> getToken() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    return prefs.getString(_tokenKey);
  }

  Future<void> _saveToken(String token) async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    await prefs.setString(_tokenKey, token);
  }
}
