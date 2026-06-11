import 'base_entity.dart';

class AppointmentEntity extends BaseEntity {
  final int salonId;
  final int artistId;
  final int serviceId;
  final DateTime startTime;
  final DateTime endTime;
  final int status;
  final double estimatedPrice;
  final double depositAmount;
  final bool isRated;
  final int rating;
  final String? comment;
  final String? salonName;
  final String? artistName;
  final String? serviceName;

  const AppointmentEntity({
    required super.id,
    required this.salonId,
    required this.artistId,
    required this.serviceId,
    required this.startTime,
    required this.endTime,
    required this.status,
    this.estimatedPrice = 0,
    this.depositAmount = 0,
    this.isRated = false,
    this.rating = 0,
    this.comment,
    this.salonName,
    this.artistName,
    this.serviceName,
    super.createdAt,
    super.updatedAt,
  });

  String get statusText {
    switch (status) {
      case 1:
        return 'در انتظار';
      case 2:
        return 'تایید شده';
      case 3:
        return 'در حال انجام';
      case 4:
        return 'تمام شده';
      case 5:
        return 'لغو شده';
      case 6:
        return 'حضور نیافت';
      default:
        return 'نامشخص';
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