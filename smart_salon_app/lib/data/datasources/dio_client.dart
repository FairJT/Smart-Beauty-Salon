import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'api_constants.dart';

class DioClient {
  static Dio? _dio;
  static String? _tenantId;

  static const _storage = FlutterSecureStorage();
  static const _tokenKey = 'auth_token';

  static Dio get instance {
    _dio ??= _createDio();
    return _dio!;
  }

  static void setTenantId(String? tenantId) {
    _tenantId = tenantId;
  }

  static Dio _createDio() {
    final dio = Dio(BaseOptions(
      baseUrl: ApiConstants.baseUrl,
      connectTimeout: const Duration(seconds: 30),
      receiveTimeout: const Duration(seconds: 30),
      headers: {
        'Content-Type': 'application/json; charset=utf-8',
        'Accept': 'application/json',
      },
    ));

    dio.interceptors.addAll([
      _AuthInterceptor(),
      _TenantInterceptor(),
      LogInterceptor(requestBody: true, responseBody: true),
    ]);

    return dio;
  }
}

class _AuthInterceptor extends Interceptor {
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    final token = await DioClient._storage.read(key: DioClient._tokenKey);

    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }

    handler.next(options);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode == 401) {
      await DioClient._storage.delete(key: DioClient._tokenKey);
    }

    handler.next(err);
  }
}

class _TenantInterceptor extends Interceptor {
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    if (DioClient._tenantId != null) {
      options.headers['X-Tenant-Id'] = DioClient._tenantId;
    }

    handler.next(options);
  }
}
