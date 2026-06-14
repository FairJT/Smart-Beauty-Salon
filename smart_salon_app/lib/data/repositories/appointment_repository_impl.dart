import '../../domain/entities/appointment_entity.dart';
import '../../domain/entities/slot_entity.dart';
import '../../domain/repositories/appointment_repository.dart';
import '../datasources/dio_client.dart';
import '../datasources/api_constants.dart';

class AppointmentRepositoryImpl implements AppointmentRepository {
  @override
  Future<List<AppointmentEntity>> getMyAppointments() async {
    final response = await DioClient.instance.get(ApiConstants.myAppointments);
    final data = response.data as List;

    return data.map((json) => AppointmentEntity(
      id: json['id'],
      salonId: json['salonId'],
      artistId: json['artistId'],
      serviceId: json['serviceId'],
      startTime: DateTime.parse(json['startTime']),
      endTime: DateTime.parse(json['endTime']),
      status: json['status'],
      estimatedPrice: (json['estimatedPrice'] ?? 0).toDouble(),
      depositAmount: (json['depositAmount'] ?? 0).toDouble(),
      isRated: json['isRated'] ?? false,
      rating: json['rating'] ?? 0,
      comment: json['comment'],
      salonName: json['salonName'],
      artistName: json['artistName'],
      serviceName: json['serviceName'],
    )).toList();
  }

  @override
  Future<AppointmentEntity> getAppointmentById(int id) async {
    final response = await DioClient.instance.get('${ApiConstants.appointments}/$id');
    final json = response.data;

    return AppointmentEntity(
      id: json['id'],
      salonId: json['salonId'],
      artistId: json['artistId'],
      serviceId: json['serviceId'],
      startTime: DateTime.parse(json['startTime']),
      endTime: DateTime.parse(json['endTime']),
      status: json['status'],
      estimatedPrice: (json['estimatedPrice'] ?? 0).toDouble(),
      depositAmount: (json['depositAmount'] ?? 0).toDouble(),
      isRated: json['isRated'] ?? false,
      rating: json['rating'] ?? 0,
      comment: json['comment'],
      salonName: json['salonName'],
      artistName: json['artistName'],
      serviceName: json['serviceName'],
    );
  }

  @override
  Future<AppointmentEntity> createAppointment(CreateAppointmentInput input) async {
    final response = await DioClient.instance.post(
      ApiConstants.appointments,
      data: {
        'salonId': input.salonId,
        'artistId': input.artistId,
        'serviceId': input.serviceId,
        'startTime': input.startTime.toIso8601String(),
        'endTime': input.endTime.toIso8601String(),
      },
    );

    final json = response.data;
    return AppointmentEntity(
      id: json['id'],
      salonId: json['salonId'],
      artistId: json['artistId'],
      serviceId: json['serviceId'],
      startTime: DateTime.parse(json['startTime']),
      endTime: DateTime.parse(json['endTime']),
      status: json['status'],
      estimatedPrice: (json['estimatedPrice'] ?? 0).toDouble(),
      depositAmount: (json['depositAmount'] ?? 0).toDouble(),
    );
  }

  @override
  Future<AppointmentEntity> cancelAppointment(int id) async {
    final response = await DioClient.instance.put(
      '${ApiConstants.appointments}/$id/cancel',
      data: {},
    );

    final json = response.data;
    return AppointmentEntity(
      id: json['id'],
      salonId: json['salonId'],
      artistId: json['artistId'],
      serviceId: json['serviceId'],
      startTime: DateTime.parse(json['startTime']),
      endTime: DateTime.parse(json['endTime']),
      status: json['status'],
      estimatedPrice: (json['estimatedPrice'] ?? 0).toDouble(),
      depositAmount: (json['depositAmount'] ?? 0).toDouble(),
    );
  }

  @override
  Future<List<SlotEntity>> getAvailableSlots(int artistId, int serviceId, DateTime date) async {
    final response = await DioClient.instance.get(
      ApiConstants.slots,
      queryParameters: {
        'artistId': artistId,
        'serviceId': serviceId,
        'date': date.toIso8601String().split('T')[0],
      },
    );

    final data = response.data as List;
    return data.map((json) => SlotEntity(
      id: json['id'],
      startTime: DateTime.parse(json['startTime']),
      endTime: DateTime.parse(json['endTime']),
      artistId: artistId,
      isAvailable: json['isAvailable'] ?? true,
    )).toList();
  }
}