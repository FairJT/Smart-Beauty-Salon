import 'base_entity.dart';

class UserEntity extends BaseEntity {
  final String phoneNumber;
  final String? firstName;
  final String? lastName;
  final String? profileImageUrl;
  final int userType;
  final bool isActive;

  const UserEntity({
    required super.id,
    required this.phoneNumber,
    this.firstName,
    this.lastName,
    this.profileImageUrl,
    this.userType = 1,
    this.isActive = true,
    super.createdAt,
    super.updatedAt,
  });

  String get fullName => '${firstName ?? ''} ${lastName ?? ''}'.trim();
}