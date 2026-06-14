class DashboardMoney {
  final int amount;
  final String currency;

  const DashboardMoney({required this.amount, this.currency = 'IRR'});

  factory DashboardMoney.fromJson(Map<String, dynamic> json) {
    return DashboardMoney(
      amount: json['amount'] ?? 0,
      currency: json['currency'] ?? 'IRR',
    );
  }
}

class SalonManagerDashboard {
  final int todayAppointments;
  final int upcomingAppointments;
  final DashboardMoney revenue;
  final List<ArtistUtilization> artistUtilization;
  final int activeServiceCount;
  final int activeArtistCount;
  final String? subscriptionStatus;

  const SalonManagerDashboard({
    required this.todayAppointments,
    required this.upcomingAppointments,
    required this.revenue,
    this.artistUtilization = const [],
    this.activeServiceCount = 0,
    this.activeArtistCount = 0,
    this.subscriptionStatus,
  });

  factory SalonManagerDashboard.fromJson(Map<String, dynamic> json) {
    return SalonManagerDashboard(
      todayAppointments: json['todayAppointments'] ?? 0,
      upcomingAppointments: json['upcomingAppointments'] ?? 0,
      revenue: DashboardMoney.fromJson(json['revenue'] ?? {}),
      artistUtilization: (json['artistUtilization'] as List<dynamic>?)
              ?.map((a) => ArtistUtilization.fromJson(a))
              .toList() ??
          [],
      activeServiceCount: json['activeServiceCount'] ?? 0,
      activeArtistCount: json['activeArtistCount'] ?? 0,
      subscriptionStatus: json['subscriptionStatus'],
    );
  }
}

class ArtistUtilization {
  final int artistId;
  final String artistName;
  final int todayAppointments;
  final int completedToday;
  final double utilizationPercent;

  const ArtistUtilization({
    required this.artistId,
    required this.artistName,
    this.todayAppointments = 0,
    this.completedToday = 0,
    this.utilizationPercent = 0,
  });

  factory ArtistUtilization.fromJson(Map<String, dynamic> json) {
    return ArtistUtilization(
      artistId: json['artistId'] ?? 0,
      artistName: json['artistName'] ?? '',
      todayAppointments: json['todayAppointments'] ?? 0,
      completedToday: json['completedToday'] ?? 0,
      utilizationPercent: (json['utilizationPercent'] ?? 0).toDouble(),
    );
  }
}

class ArtistDashboard {
  final int todayAppointments;
  final int upcomingAppointments;
  final ArtistNextAppointment? nextAppointment;
  final double ratingAvg;
  final int ratingCount;
  final int monthAppointments;
  final DashboardMoney? monthRevenue;

  const ArtistDashboard({
    this.todayAppointments = 0,
    this.upcomingAppointments = 0,
    this.nextAppointment,
    this.ratingAvg = 0,
    this.ratingCount = 0,
    this.monthAppointments = 0,
    this.monthRevenue,
  });

  factory ArtistDashboard.fromJson(Map<String, dynamic> json) {
    return ArtistDashboard(
      todayAppointments: json['todayAppointments'] ?? 0,
      upcomingAppointments: json['upcomingAppointments'] ?? 0,
      nextAppointment: json['nextAppointment'] != null
          ? ArtistNextAppointment.fromJson(json['nextAppointment'])
          : null,
      ratingAvg: (json['ratingAvg'] ?? 0).toDouble(),
      ratingCount: json['ratingCount'] ?? 0,
      monthAppointments: json['monthAppointments'] ?? 0,
      monthRevenue: json['monthRevenue'] != null
          ? DashboardMoney.fromJson(json['monthRevenue'])
          : null,
    );
  }
}

class ArtistNextAppointment {
  final int id;
  final DateTime startTime;
  final String clientName;
  final String serviceName;
  final int status;

  const ArtistNextAppointment({
    required this.id,
    required this.startTime,
    this.clientName = '',
    this.serviceName = '',
    this.status = 0,
  });

  factory ArtistNextAppointment.fromJson(Map<String, dynamic> json) {
    return ArtistNextAppointment(
      id: json['id'] ?? 0,
      startTime: DateTime.parse(json['startTime']),
      clientName: json['clientName'] ?? '',
      serviceName: json['serviceName'] ?? '',
      status: json['status'] ?? 0,
    );
  }
}

class ClientDashboard {
  final int upcomingBookings;
  final ClientNextBooking? nextBooking;
  final int loyaltyPoints;
  final int totalVisits;
  final int unreadNotifications;
  final List<FavoriteSalon> favoriteSalons;

  const ClientDashboard({
    this.upcomingBookings = 0,
    this.nextBooking,
    this.loyaltyPoints = 0,
    this.totalVisits = 0,
    this.unreadNotifications = 0,
    this.favoriteSalons = const [],
  });

  factory ClientDashboard.fromJson(Map<String, dynamic> json) {
    return ClientDashboard(
      upcomingBookings: json['upcomingBookings'] ?? 0,
      nextBooking: json['nextBooking'] != null
          ? ClientNextBooking.fromJson(json['nextBooking'])
          : null,
      loyaltyPoints: json['loyaltyPoints'] ?? 0,
      totalVisits: json['totalVisits'] ?? 0,
      unreadNotifications: json['unreadNotifications'] ?? 0,
      favoriteSalons: (json['favoriteSalons'] as List<dynamic>?)
              ?.map((f) => FavoriteSalon.fromJson(f))
              .toList() ??
          [],
    );
  }
}

class ClientNextBooking {
  final int id;
  final DateTime startTime;
  final String salonName;
  final String serviceName;
  final String artistName;
  final int status;

  const ClientNextBooking({
    required this.id,
    required this.startTime,
    this.salonName = '',
    this.serviceName = '',
    this.artistName = '',
    this.status = 0,
  });

  factory ClientNextBooking.fromJson(Map<String, dynamic> json) {
    return ClientNextBooking(
      id: json['id'] ?? 0,
      startTime: DateTime.parse(json['startTime'] ?? DateTime.now().toIso8601String()),
      salonName: json['salonName'] ?? '',
      serviceName: json['serviceName'] ?? '',
      artistName: json['artistName'] ?? '',
      status: json['status'] ?? 0,
    );
  }
}

class FavoriteSalon {
  final int salonId;
  final String salonName;
  final String? logoUrl;
  final double ratingAvg;
  final bool isVip;

  const FavoriteSalon({
    required this.salonId,
    required this.salonName,
    this.logoUrl,
    this.ratingAvg = 0,
    this.isVip = false,
  });

  factory FavoriteSalon.fromJson(Map<String, dynamic> json) {
    return FavoriteSalon(
      salonId: json['salonId'] ?? 0,
      salonName: json['salonName'] ?? json['name'] ?? '',
      logoUrl: json['logoUrl'],
      ratingAvg: (json['ratingAvg'] ?? 0).toDouble(),
      isVip: json['isVip'] ?? false,
    );
  }
}

class SuperAdminDashboard {
  final int totalTenants;
  final int totalSalons;
  final int activeSalons;
  final int totalArtists;
  final int activeSubscriptions;
  final DashboardMoney platformRevenue;

  const SuperAdminDashboard({
    this.totalTenants = 0,
    this.totalSalons = 0,
    this.activeSalons = 0,
    this.totalArtists = 0,
    this.activeSubscriptions = 0,
    required this.platformRevenue,
  });

  factory SuperAdminDashboard.fromJson(Map<String, dynamic> json) {
    return SuperAdminDashboard(
      totalTenants: json['totalTenants'] ?? 0,
      totalSalons: json['totalSalons'] ?? 0,
      activeSalons: json['activeSalons'] ?? 0,
      totalArtists: json['totalArtists'] ?? 0,
      activeSubscriptions: json['activeSubscriptions'] ?? 0,
      platformRevenue: DashboardMoney.fromJson(json['platformRevenue'] ?? {}),
    );
  }
}
