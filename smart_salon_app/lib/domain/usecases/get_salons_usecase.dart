import '../entities/salon_entity.dart';
import '../repositories/salon_repository.dart';

class GetSalonsUseCase {
  final SalonRepository _salonRepository;

  GetSalonsUseCase(this._salonRepository);

  Future<List<SalonEntity>> call({String? searchQuery}) async {
    return await _salonRepository.getSalons(searchQuery: searchQuery);
  }
}