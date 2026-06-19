import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'api_constants.dart';

class DioClient {
  static Dio? _dio;
  static String? _tenantId;

  static final SharedPreferences? _prefs =
      null; // placeholder, will be initialized at runtime
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
      baseUrl: '',
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
  void onRequest(
      RequestOptions options, RequestInterceptorHandler handler) async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    final token = prefs.getString(DioClient._tokenKey);

    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }

    handler.next(options);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode == 401) {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      await prefs.remove(DioClient._tokenKey);
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
