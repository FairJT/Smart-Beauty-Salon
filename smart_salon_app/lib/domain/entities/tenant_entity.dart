import '../../types.dart';
import 'base_entity.dart';

abstract class TenantEntity extends BaseEntity {
  final String tenantId;

  const TenantEntity({
    required super.id,
    required this.tenantId,
    super.createdAt,
    super.updatedAt,
  });
}
