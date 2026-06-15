import '../../types.dart';
import 'base_entity.dart';

class UserEntity extends BaseEntity {
  final String phoneNumber;
  final String? firstName;
  final String? lastName;
  final String? profileImageUrl;
  final int userType;
  final bool isActive;
  final int loyaltyPoints;
  final int totalVisits;

  const UserEntity({
    required super.id,
    required this.phoneNumber,
    this.firstName,
    this.lastName,
    this.profileImageUrl,
    this.userType = 1,
    this.isActive = true,
    this.loyaltyPoints = 0,
    this.totalVisits = 0,
    super.createdAt,
    super.updatedAt,
  });

  String get fullName => '${firstName ?? ''} ${lastName ?? ''}'.trim();

  bool get isSuperAdmin => userType == 1;
  bool get isSalonManager => userType == 2;
  bool get isArtist => userType == 3;
  bool get isClient => userType == 4;

  String get userTypeName {
    switch (userType) {
      case 1:
        return 'SuperAdmin';
      case 2:
        return 'SalonManager';
      case 3:
        return 'Artist';
      case 4:
        return 'Client';
      default:
        return 'Client';
    }
  }
}
