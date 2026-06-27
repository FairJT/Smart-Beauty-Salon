import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/app_colors.dart';
import '../../../data/datasources/dio_client.dart';
import '../../../data/datasources/api_constants.dart';
import '../../../core/format/money_formatter.dart';

class CatalogManagementScreen extends ConsumerStatefulWidget {
  const CatalogManagementScreen({super.key});

  @override
  ConsumerState<CatalogManagementScreen> createState() =>
      _CatalogManagementScreenState();
}

class _CatalogManagementScreenState
    extends ConsumerState<CatalogManagementScreen> {
  List<_ServiceInfo> _services = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadServices();
  }

  Future<void> _loadServices() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final response = await DioClient.instance.get(ApiConstants.services);
      final data = response.data as List;
      setState(() {
        _services = data.map((j) => _ServiceInfo.fromJson(j)).toList();
        _loading = false;
      });
    } catch (e) {
      setState(() {
        _error = e.toString();
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('مدیریت خدمات',
            style: TextStyle(fontWeight: FontWeight.bold)),
        actions: [
          IconButton(
            icon: const Icon(Icons.add_circle_outline),
            onPressed: _showAddServiceDialog,
          ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? _buildError()
              : _services.isEmpty
                  ? _buildEmpty()
                  : RefreshIndicator(
                      onRefresh: _loadServices,
                      child: ListView.builder(
                        padding: const EdgeInsets.all(16),
                        itemCount: _services.length,
                        itemBuilder: (_, i) => _ServiceCard(
                          service: _services[i],
                          onToggleActive: () => _toggleActive(_services[i]),
                          onEdit: () => _showEditServiceDialog(_services[i]),
                          onDelete: () => _confirmDelete(_services[i]),
                        ),
                      ),
                    ),
    );
  }

  Widget _buildEmpty() {
    return const Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.category_outlined, size: 60, color: Colors.grey),
          SizedBox(height: 12),
          Text('هیچ خدماتی ثبت نشده',
              style: TextStyle(color: Colors.grey, fontSize: 16)),
        ],
      ),
    );
  }

  Widget _buildError() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(40),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.wifi_off, size: 60, color: Colors.grey),
            const SizedBox(height: 12),
            Text(_error!,
                textAlign: TextAlign.center,
                style: const TextStyle(color: Colors.grey)),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: _loadServices,
              child: const Text('بازآوری'),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _toggleActive(_ServiceInfo service) async {
    try {
      await DioClient.instance.put(
        '${ApiConstants.services}/${service.id}/toggle-active',
      );
      await _loadServices();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text(e.toString()), backgroundColor: AppColors.danger),
      );
    }
  }

  void _showAddServiceDialog() {
    final nameController = TextEditingController();
    final descriptionController = TextEditingController();
    final priceController = TextEditingController();
    final durationController = TextEditingController();

    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('افزودن خدمت'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: nameController,
                decoration: const InputDecoration(
                  labelText: 'نام خدمت',
                  border: OutlineInputBorder(),
                ),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: descriptionController,
                decoration: const InputDecoration(
                  labelText: 'توضیحات',
                  border: OutlineInputBorder(),
                ),
                maxLines: 3,
              ),
              const SizedBox(height: 12),
              TextField(
                controller: priceController,
                decoration: const InputDecoration(
                  labelText: 'قیمت (ریال)',
                  border: OutlineInputBorder(),
                ),
                keyboardType: TextInputType.number,
              ),
              const SizedBox(height: 12),
              TextField(
                controller: durationController,
                decoration: const InputDecoration(
                  labelText: 'مدت زمان (دقیقه)',
                  border: OutlineInputBorder(),
                ),
                keyboardType: TextInputType.number,
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('لغو'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary),
            onPressed: () async {
              Navigator.pop(context);
              await _addService(
                nameController.text,
                descriptionController.text,
                int.tryParse(priceController.text) ?? 0,
                int.tryParse(durationController.text) ?? 30,
              );
            },
            child: const Text('ایجاد'),
          ),
        ],
      ),
    );
  }

  void _showEditServiceDialog(_ServiceInfo service) {
    final nameController = TextEditingController(text: service.name);
    final descriptionController =
        TextEditingController(text: service.description ?? '');
    final priceController = TextEditingController(
        text: (service.price * 10)
            .toString()); // Convert Toman to Rial for display
    final durationController =
        TextEditingController(text: service.durationMinutes.toString());

    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('ویرایش خدمت'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: nameController,
                decoration: const InputDecoration(
                  labelText: 'نام خدمت',
                  border: OutlineInputBorder(),
                ),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: descriptionController,
                decoration: const InputDecoration(
                  labelText: 'توضیحات',
                  border: OutlineInputBorder(),
                ),
                maxLines: 3,
              ),
              const SizedBox(height: 12),
              TextField(
                controller: priceController,
                decoration: const InputDecoration(
                  labelText: 'قیمت (ریال)',
                  border: OutlineInputBorder(),
                ),
                keyboardType: TextInputType.number,
              ),
              const SizedBox(height: 12),
              TextField(
                controller: durationController,
                decoration: const InputDecoration(
                  labelText: 'مدت زمان (دقیقه)',
                  border: OutlineInputBorder(),
                ),
                keyboardType: TextInputType.number,
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('لغو'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary),
            onPressed: () async {
              Navigator.pop(context);
              await _updateService(
                service.id,
                nameController.text,
                descriptionController.text,
                int.tryParse(priceController.text) ?? 0,
                int.tryParse(durationController.text) ?? 30,
              );
            },
            child: const Text('به‌روزرسانی'),
          ),
        ],
      ),
    );
  }

  void _confirmDelete(_ServiceInfo service) {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('حذف خدمت'),
        content:
            Text('آیا مطمئن هستید که می‌خواهید "${service.name}" را حذف کنید؟'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('لغو'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.danger),
            onPressed: () async {
              Navigator.pop(context);
              await _deleteService(service.id);
            },
            child: const Text('حذف'),
          ),
        ],
      ),
    );
  }

  Future<void> _addService(String name, String description, int priceRials,
      int durationMinutes) async {
    try {
      await DioClient.instance.post(
        ApiConstants.services,
        data: {
          'name': name,
          'description': description,
          'price': priceRials ~/ 10, // Convert Rials to Toman (divide by 10)
          'durationMinutes': durationMinutes,
        },
      );
      await _loadServices();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text(e.toString()), backgroundColor: AppColors.danger),
      );
    }
  }

  Future<void> _updateService(int id, String name, String description,
      int priceRials, int durationMinutes) async {
    try {
      await DioClient.instance.put(
        '${ApiConstants.services}/$id',
        data: {
          'name': name,
          'description': description,
          'price': priceRials ~/ 10, // Convert Rials to Toman (divide by 10)
          'durationMinutes': durationMinutes,
        },
      );
      await _loadServices();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text(e.toString()), backgroundColor: AppColors.danger),
      );
    }
  }

  Future<void> _deleteService(int id) async {
    try {
      await DioClient.instance.delete('${ApiConstants.services}/$id');
      await _loadServices();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text(e.toString()), backgroundColor: AppColors.danger),
      );
    }
  }
}

class _ServiceInfo {
  final dynamic id;
  final String name;
  final String? description;
  final int price; // Price in Toman (stored as integer)
  final int durationMinutes;
  final String? imageUrl;
  final bool isActive;

  _ServiceInfo({
    required this.id,
    required this.name,
    this.description,
    required this.price,
    required this.durationMinutes,
    this.imageUrl,
    required this.isActive,
  });

  factory _ServiceInfo.fromJson(Map<String, dynamic> json) => _ServiceInfo(
        id: json['id'] ?? 0,
        name: json['name'] ?? '',
        description: json['description'],
        price: json['price'] ?? 0, // Price in Toman from API
        durationMinutes: json['durationMinutes'] ?? 30,
        imageUrl: json['imageUrl'],
        isActive: json['isActive'] ?? true,
      );
}

class _ServiceCard extends StatelessWidget {
  final _ServiceInfo service;
  final VoidCallback onToggleActive;
  final VoidCallback onEdit;
  final VoidCallback onDelete;

  const _ServiceCard({
    required this.service,
    required this.onToggleActive,
    required this.onEdit,
    required this.onDelete,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    service.name,
                    style: const TextStyle(
                        fontWeight: FontWeight.bold, fontSize: 15),
                  ),
                  const SizedBox(height: 4),
                  if (service.description != null &&
                      service.description!.isNotEmpty)
                    Text(
                      service.description!,
                      style: const TextStyle(color: Colors.grey, fontSize: 13),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      const Icon(Icons.attach_money,
                          size: 16, color: Colors.green),
                      const SizedBox(width: 4),
                      Text(
                        MoneyFormatter.format(service.price *
                            10), // Convert Toman to Rial for display
                        style: const TextStyle(fontSize: 13),
                      ),
                      const SizedBox(width: 16),
                      const Icon(Icons.access_time,
                          size: 16, color: Colors.blue),
                      const SizedBox(width: 4),
                      Text(
                        '${service.durationMinutes} دقیقه',
                        style: const TextStyle(fontSize: 13),
                      ),
                    ],
                  ),
                ],
              ),
            ),
            Column(
              children: [
                IconButton(
                  icon: const Icon(Icons.edit_outlined, color: Colors.blue),
                  onPressed: onEdit,
                ),
                const SizedBox(height: 8),
                IconButton(
                  icon: Icon(
                    service.isActive ? Icons.toggle_on : Icons.toggle_off,
                    color: service.isActive ? Colors.green : Colors.grey,
                  ),
                  onPressed: onToggleActive,
                ),
                const SizedBox(height: 8),
                IconButton(
                  icon: const Icon(Icons.delete_outline, color: Colors.red),
                  onPressed: onDelete,
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
