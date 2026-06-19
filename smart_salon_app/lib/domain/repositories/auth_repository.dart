import '../entities/user_entity.dart';

abstract class AuthRepository {
  Future<UserEntity> login(String phoneNumber, String password);
  Future<UserEntity> register(String phoneNumber, String password,
      String firstName, String lastName, String nationalCode);
  Future<UserEntity> getProfile();
  Future<void> logout();
  Future<bool> isLoggedIn();
  Future<String?> getToken();
}
