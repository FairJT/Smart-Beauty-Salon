import '../../domain/entities/appointment_entity.dart';
import '../../domain/entities/slot_entity.dart';
import '../../domain/repositories/appointment_repository.dart';
import '../datasources/dio_client.dart';
import '../datasources/api_constants.dart';
import '../../types.dart';

class AppointmentRepositoryImpl implements AppointmentRepository {
  @override
  Future<List<AppointmentEntity>> getMyAppointments() async {
    final response = await DioClient.instance.get(ApiConstants.myAppointments);
    final data = response.data as List;

    return data.map((json) => _parseAppointment(json)).toList();
  }

  @override
  Future<AppointmentEntity> getAppointmentById(AppointmentId id) async {
    final response =
        await DioClient.instance.get('${ApiConstants.appointments}/$id');
    return _parseAppointment(response.data);
  }

  @override
  Future<AppointmentEntity> createAppointment(
      CreateAppointmentInput input) async {
    final response = await DioClient.instance.post(
      '${ApiConstants.appointments}/simple',
      data: {
        'slug': input.slug,
        'artistId': input.artistId,
        'serviceId': input.serviceId,
        'startTime': input.startTime.toIso8601String(),
        'endTime': input.endTime.toIso8601String(),
      },
    );

    final json = response.data;
    return await getAppointmentById(json['id'].toString());
  }

  @override
  Future<AppointmentEntity> cancelAppointment(AppointmentId id) async {
    await DioClient.instance.put(
      '${ApiConstants.appointments}/$id/cancel',
      data: {},
    );

    return await getAppointmentById(id);
  }

  @override
  Future<List<SlotEntity>> getAvailableSlots(
      ArtistId artistId, ServiceId serviceId, DateTime date) async {
    final response = await DioClient.instance.get(
      ApiConstants.slots,
      queryParameters: {
        'artistId': artistId,
        'date': date.toIso8601String().split('T')[0],
        'durationMinutes': 30,
      },
    );

    final data = response.data as List;
    return data
        .map((json) => SlotEntity(
              id: json['id']?.toString() ?? '',
              startTime: DateTime.parse(json['startTime']),
              endTime: DateTime.parse(json['endTime']),
              isAvailable: json['isAvailable'] ?? true,
            ))
        .toList();
  }

  AppointmentEntity _parseAppointment(Map<String, dynamic> json) {
    return AppointmentEntity(
      id: json['id']?.toString() ?? '',
      startTime: DateTime.parse(json['startsAt']),
      endTime: DateTime.parse(json['endsAt']),
      status: json['status'] ?? 1,
      estimatedPrice: (json['estimatedPriceAmount'] ?? 0).toDouble(),
      depositAmount: (json['depositAmountValue'] ?? 0).toDouble(),
      isRated: json['isRated'] ?? false,
      rating: json['rating'] ?? 0,
      comment: json['comment'],
      salonName: json['salonName'],
      artistName: json['artistName'],
      serviceName: json['serviceName'],
    );
  }
}
