import 'artist.dart';
import 'service_item.dart';

class Salon {
  final String id;
  final String name;
  final String? slug;
  final String? phone;
  final String? address;
  final String? description;
  final String? logoUrl;
  final String themeColor;
  final bool isVip;
  final double ratingAvg;
  final List<Artist> artists;
  final List<ServiceItem> services;

  Salon({
    required this.id,
    required this.name,
    this.slug,
    this.phone,
    this.address,
    this.description,
    this.logoUrl,
    this.themeColor = '#1B3A5C',
    this.isVip = false,
    this.ratingAvg = 0.0,
    this.artists = const [],
    this.services = const [],
  });

  factory Salon.fromJson(Map<String, dynamic> json) {
    return Salon(
      id: json['id']?.toString() ?? '',
      name: json['name'] ?? '',
      slug: json['slug'],
      phone: json['phone'],
      address: json['address'],
      description: json['description'],
      logoUrl: json['logoUrl'],
      themeColor: json['themeColor'] ?? '#1B3A5C',
      isVip: json['isVip'] ?? false,
      ratingAvg: (json['ratingAvg'] ?? 0).toDouble(),
      artists: (json['artists'] as List<dynamic>?)
              ?.map((a) => Artist.fromJson(a))
              .toList() ??
          [],
      services: (json['services'] as List<dynamic>?)
              ?.map((s) => ServiceItem.fromJson(s))
              .toList() ??
          [],
    );
  }

  String get initial => name.isNotEmpty ? name[0] : '؟';
}

class SalonListItem {
  final String id;
  final String name;
  final String? logoUrl;
  final double ratingAvg;
  final bool isVip;
  final String? address;
  final int serviceCount;
  final int artistCount;

  SalonListItem({
    required this.id,
    required this.name,
    this.logoUrl,
    this.ratingAvg = 0.0,
    this.isVip = false,
    this.address,
    this.serviceCount = 0,
    this.artistCount = 0,
  });

  factory SalonListItem.fromJson(Map<String, dynamic> json) {
    return SalonListItem(
      id: json['id']?.toString() ?? '',
      name: json['name'] ?? '',
      logoUrl: json['logoUrl'],
      ratingAvg: (json['ratingAvg'] ?? 0).toDouble(),
      isVip: json['isVip'] ?? false,
      address: json['address'],
      serviceCount: json['serviceCount'] ?? 0,
      artistCount: json['artistCount'] ?? 0,
    );
  }

  String get initial => name.isNotEmpty ? name[0] : '؟';
}
