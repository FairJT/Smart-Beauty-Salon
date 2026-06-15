class ServiceItem {
  final String id;
  final String name;
  final String category;
  final int baseDurationMinutes;
  final double basePrice;

  ServiceItem({
    required this.id,
    required this.name,
    required this.category,
    this.baseDurationMinutes = 30,
    this.basePrice = 0,
  });

  factory ServiceItem.fromJson(Map<String, dynamic> json) {
    return ServiceItem(
      id: json['id']?.toString() ?? '',
      name: json['name'] ?? '',
      category: json['category'] ?? '',
      baseDurationMinutes: json['baseDurationMinutes'] ?? 30,
      basePrice: (json['basePrice'] ?? 0).toDouble(),
    );
  }
}
