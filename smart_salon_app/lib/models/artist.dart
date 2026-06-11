class Artist {
  final int id;
  final String firstName;
  final String lastName;
  final String? photoUrl;
  final String? bioShort;
  final double ratingAvg;
  final int ratingCount;
  final String contractType;

  Artist({
    required this.id,
    required this.firstName,
    required this.lastName,
    this.photoUrl,
    this.bioShort,
    this.ratingAvg = 0.0,
    this.ratingCount = 0,
    this.contractType = '',
  });

  factory Artist.fromJson(Map<String, dynamic> json) {
    return Artist(
      id: json['id'] ?? 0,
      firstName: json['firstName'] ?? '',
      lastName: json['lastName'] ?? '',
      photoUrl: json['photoUrl'],
      bioShort: json['bioShort'],
      ratingAvg: (json['ratingAvg'] ?? 0).toDouble(),
      ratingCount: json['ratingCount'] ?? 0,
      contractType: json['contractType'] ?? '',
    );
  }

  String get fullName => '$firstName $lastName';
  String get initial => firstName.isNotEmpty ? firstName[0] : '؟';
}
