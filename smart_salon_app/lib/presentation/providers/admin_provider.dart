import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/datasources/dio_client.dart';
import '../../data/datasources/api_constants.dart';

class AdminStats {
  final int totalUsers;
  final int totalSalons;
  final int totalAppointments;
  final int activeSalons;
  final int totalArtists;
  final double totalRevenue;

  AdminStats({
    this.totalUsers = 0,
    this.totalSalons = 0,
    this.totalAppointments = 0,
    this.activeSalons = 0,
    this.totalArtists = 0,
    this.totalRevenue = 0,
  });

  factory AdminStats.fromJson(Map<String, dynamic> json) => AdminStats(
        totalUsers: json['totalUsers'] ?? 0,
        totalSalons: json['totalSalons'] ?? 0,
        totalAppointments: json['totalAppointments'] ?? 0,
        activeSalons: json['activeSalons'] ?? 0,
        totalArtists: json['totalArtists'] ?? 0,
        totalRevenue: (json['totalRevenue'] ?? 0).toDouble(),
      );
}

class AdminUser {
  final String id;
  final String phoneNumber;
  final String firstName;
  final String lastName;
  final String userType;
  final bool isActive;

  AdminUser({
    required this.id,
    required this.phoneNumber,
    required this.firstName,
    required this.lastName,
    required this.userType,
    required this.isActive,
  });

  factory AdminUser.fromJson(Map<String, dynamic> json) => AdminUser(
        id: json['id'] ?? '',
        phoneNumber: json['phoneNumber'] ?? '',
        firstName: json['firstName'] ?? '',
        lastName: json['lastName'] ?? '',
        userType: json['userType'] ?? 'Client',
        isActive: json['isActive'] ?? true,
      );

  String get fullName => '$firstName $lastName'.trim();
}

class AdminSalon {
  final String slug;
  final String name;
  final String? phone;
  final String? address;
  final bool isVip;
  final bool isActive;
  final String managerName;
  final int artistCount;
  final int serviceCount;

  AdminSalon({
    required this.slug,
    required this.name,
    this.phone,
    this.address,
    required this.isVip,
    required this.isActive,
    required this.managerName,
    required this.artistCount,
    required this.serviceCount,
  });

  factory AdminSalon.fromJson(Map<String, dynamic> json) => AdminSalon(
        slug: json['slug']?.toString() ?? '',
        name: json['name'] ?? '',
        phone: json['phone'],
        address: json['address'],
        isVip: json['isVip'] ?? false,
        isActive: json['isActive'] ?? true,
        managerName: json['managerName'] ?? '',
        artistCount: json['artistCount'] ?? 0,
        serviceCount: json['serviceCount'] ?? 0,
      );
}

class AdminState {
  final AdminStats? stats;
  final List<AdminUser> users;
  final List<AdminSalon> salons;
  final bool loading;
  final String? error;

  AdminState({
    this.stats,
    this.users = const [],
    this.salons = const [],
    this.loading = false,
    this.error,
  });

  AdminState copyWith({
    AdminStats? stats,
    List<AdminUser>? users,
    List<AdminSalon>? salons,
    bool? loading,
    String? error,
  }) =>
      AdminState(
        stats: stats ?? this.stats,
        users: users ?? this.users,
        salons: salons ?? this.salons,
        loading: loading ?? this.loading,
        error: error,
      );
}

class AdminNotifier extends StateNotifier<AdminState> {
  AdminNotifier() : super(AdminState());

  Future<void> loadStats() async {
    state = state.copyWith(loading: true);
    try {
      final response = await DioClient.instance.get(ApiConstants.adminStats);
      state = state.copyWith(
        stats: AdminStats.fromJson(response.data),
        loading: false,
      );
    } catch (e) {
      state = state.copyWith(error: e.toString(), loading: false);
    }
  }

  Future<void> loadUsers() async {
    try {
      final response = await DioClient.instance.get(ApiConstants.adminUsers);
      final data = response.data['data'] as List;
      state = state.copyWith(
        users: data.map((j) => AdminUser.fromJson(j)).toList(),
      );
    } catch (e) {
      state = state.copyWith(error: e.toString());
    }
  }

  Future<void> loadSalons() async {
    try {
      final response = await DioClient.instance.get(ApiConstants.adminSalons);
      final data = response.data['data'] as List;
      state = state.copyWith(
        salons: data.map((j) => AdminSalon.fromJson(j)).toList(),
      );
    } catch (e) {
      state = state.copyWith(error: e.toString());
    }
  }

  Future<void> toggleUserActive(String userId) async {
    try {
      await DioClient.instance
          .put('${ApiConstants.adminUsers}/$userId/toggle-active');
      await loadUsers();
    } catch (e) {
      state = state.copyWith(error: e.toString());
    }
  }

  Future<void> changeUserType(String userId, int type) async {
    try {
      await DioClient.instance.put(
        '${ApiConstants.adminUsers}/$userId/type',
        data: {'userType': type},
      );
      await loadUsers();
    } catch (e) {
      state = state.copyWith(error: e.toString());
    }
  }

  Future<void> toggleSalonActive(String slug) async {
    try {
      await DioClient.instance
          .put('${ApiConstants.adminSalons}/$slug/toggle-active');
      await loadSalons();
    } catch (e) {
      state = state.copyWith(error: e.toString());
    }
  }

  Future<void> toggleSalonVip(String slug) async {
    try {
      await DioClient.instance
          .put('${ApiConstants.adminSalons}/$slug/toggle-vip');
      await loadSalons();
    } catch (e) {
      state = state.copyWith(error: e.toString());
    }
  }
}

final adminProvider =
    StateNotifierProvider<AdminNotifier, AdminState>((ref) {
  return AdminNotifier();
});
