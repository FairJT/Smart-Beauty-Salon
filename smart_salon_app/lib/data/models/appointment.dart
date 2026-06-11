class AppointmentItem {
  final int id;
  final DateTime startTime;
  final DateTime endTime;
  final int status;
  final double estimatedPrice;
  final double depositAmount;
  final bool isRated;
  final int rating;
  final String? comment;
  final String salonName;
  final String artistName;
  final String serviceName;

  AppointmentItem({
    required this.id,
    required this.startTime,
    required this.endTime,
    required this.status,
    this.estimatedPrice = 0,
    this.depositAmount = 0,
    this.isRated = false,
    this.rating = 0,
    this.comment,
    this.salonName = '',
    this.artistName = '',
    this.serviceName = '',
  });

  factory AppointmentItem.fromJson(Map<String, dynamic> json) {
    return AppointmentItem(
      id: json['id'] ?? 0,
      startTime: DateTime.parse(json['startTime']),
      endTime: DateTime.parse(json['endTime']),
      status: json['status'] ?? 0,
      estimatedPrice: (json['estimatedPrice'] ?? 0).toDouble(),
      depositAmount: (json['depositAmount'] ?? 0).toDouble(),
      isRated: json['isRated'] ?? false,
      rating: json['rating'] ?? 0,
      comment: json['comment'],
      salonName: json['salonName'] ?? '',
      artistName: json['artistName'] ?? '',
      serviceName: json['serviceName'] ?? '',
    );
  }

  String get statusText {
    switch (status) {
      case 1: return 'در انتظار';
      case 2: return 'تایید شده';
      case 3: return 'در حال انجام';
      case 4: return 'تمام شده';
      case 5: return 'لغو شده';
      case 6: return 'حضور نیافت';
      default: return 'نامشخص';
    }
  }
}

enum AppointmentStatus {
  pending(1, 'در انتظار'),
  confirmed(2, 'تایید شده'),
  inProgress(3, 'در حال انجام'),
  completed(4, 'تمام شده'),
  cancelled(5, 'لغو شده'),
  noShow(6, 'حضور نیافت');

  final int value;
  final String label;
  const AppointmentStatus(this.value, this.label);
}
