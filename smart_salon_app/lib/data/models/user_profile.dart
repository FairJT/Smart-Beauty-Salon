class UserProfile {
  final String id;
  final String firstName;
  final String lastName;
  final String? mobile;
  final String userType;
  final int loyaltyPoints;
  final int totalVisits;
  final bool isActive;

  UserProfile({
    required this.id,
    required this.firstName,
    required this.lastName,
    this.mobile,
    this.userType = 'Client',
    this.loyaltyPoints = 0,
    this.totalVisits = 0,
    this.isActive = true,
  });

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      id: json['id'] ?? '',
      firstName: json['firstName'] ?? '',
      lastName: json['lastName'] ?? '',
      mobile: json['mobile'],
      userType: json['userType'] ?? 'Client',
      loyaltyPoints: json['loyaltyPoints'] ?? 0,
      totalVisits: json['totalVisits'] ?? 0,
      isActive: json['isActive'] ?? true,
    );
  }

  String get fullName => '$firstName $lastName';
}
