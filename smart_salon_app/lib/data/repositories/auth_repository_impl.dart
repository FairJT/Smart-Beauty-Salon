import 'package:shared_preferences/shared_preferences.dart';
import '../../domain/entities/user_entity.dart';
import '../../domain/repositories/auth_repository.dart';
import '../datasources/dio_client.dart';
import '../datasources/api_constants.dart';

class AuthRepositoryImpl implements AuthRepository {
  @override
  Future<UserEntity> login(String phoneNumber, String password) async {
    final response = await DioClient.instance.post(
      ApiConstants.login,
      data: {
        'phoneNumber': phoneNumber,
        'password': password,
      },
    );

    final data = response.data;
    await _saveToken(data['token']);

    return UserEntity(
      id: data['user']['id'],
      phoneNumber: data['user']['phoneNumber'],
      firstName: data['user']['firstName'],
      lastName: data['user']['lastName'],
      userType: data['user']['userType'] ?? 1,
    );
  }

  @override
  Future<UserEntity> register(String phoneNumber, String password, String firstName, String lastName) async {
    final response = await DioClient.instance.post(
      ApiConstants.register,
      data: {
        'phoneNumber': phoneNumber,
        'password': password,
        'firstName': firstName,
        'lastName': lastName,
      },
    );

    final data = response.data;
    await _saveToken(data['token']);

    return UserEntity(
      id: data['user']['id'],
      phoneNumber: data['user']['phoneNumber'],
      firstName: data['user']['firstName'],
      lastName: data['user']['lastName'],
      userType: data['user']['userType'] ?? 1,
    );
  }

  @override
  Future<UserEntity> getProfile() async {
    final response = await DioClient.instance.get(ApiConstants.profile);
    final data = response.data;

    return UserEntity(
      id: data['id'],
      phoneNumber: data['phoneNumber'],
      firstName: data['firstName'],
      lastName: data['lastName'],
      userType: data['userType'] ?? 1,
    );
  }

  @override
  Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('token');
    await prefs.remove('token_saved_at');
  }

  @override
  Future<bool> isLoggedIn() async {
    final token = await getToken();
    return token != null;
  }

  @override
  Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');
    if (token == null) return null;

    final savedAt = prefs.getString('token_saved_at');
    if (savedAt != null) {
      final savedDate = DateTime.parse(savedAt);
      if (DateTime.now().difference(savedDate).inDays >= 29) {
        await logout();
        return null;
      }
    }

    return token;
  }

  Future<void> _saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('token', token);
    await prefs.setString('token_saved_at', DateTime.now().toIso8601String());
  }
}