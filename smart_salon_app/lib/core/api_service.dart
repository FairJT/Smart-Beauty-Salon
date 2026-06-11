import 'dart:convert';
import 'dart:async';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import 'api_constants.dart';

class ApiException implements Exception {
  final String message;
  final int? statusCode;

  ApiException(this.message, {this.statusCode});

  @override
  String toString() => message;
}

class ApiService {
  static const Duration _timeout = Duration(seconds: 30);

  // ─── Token Management ────────────────────────────────────
  static Future<void> saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('token', token);
    await prefs.setString('token_saved_at', DateTime.now().toIso8601String());
  }

  static Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');
    if (token == null) return null;

    final savedAt = prefs.getString('token_saved_at');
    if (savedAt != null) {
      final savedDate = DateTime.parse(savedAt);
      if (DateTime.now().difference(savedDate).inDays >= 29) {
        await clearToken();
        return null;
      }
    }

    return token;
  }

  static Future<void> clearToken() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('token');
    await prefs.remove('token_saved_at');
  }

  static Future<bool> get isLoggedIn async => (await getToken()) != null;

  // ─── Headers ─────────────────────────────────────────────
  static Future<Map<String, String>> _headers() async {
    final token = await getToken();
    return {
      'Content-Type': 'application/json; charset=utf-8',
      'Accept': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  // ─── GET ─────────────────────────────────────────────────
  static Future<dynamic> get(String url) async {
    try {
      final response = await http
          .get(Uri.parse(url), headers: await _headers())
          .timeout(_timeout);
      return _handleResponse(response);
    } on TimeoutException {
      throw ApiException('زمان اتصال تمام شد. لطفاً دوباره تلاش کنید');
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException('خطا در ارتباط با سرور');
    }
  }

  // ─── POST ────────────────────────────────────────────────
  static Future<dynamic> post(String url, Map<String, dynamic> body) async {
    try {
      final response = await http
          .post(Uri.parse(url), headers: await _headers(), body: jsonEncode(body))
          .timeout(_timeout);
      return _handleResponse(response);
    } on TimeoutException {
      throw ApiException('زمان اتصال تمام شد. لطفاً دوباره تلاش کنید');
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException('خطا در ارتباط با سرور');
    }
  }

  // ─── PUT ─────────────────────────────────────────────────
  static Future<dynamic> put(String url, Map<String, dynamic> body) async {
    try {
      final response = await http
          .put(Uri.parse(url), headers: await _headers(), body: jsonEncode(body))
          .timeout(_timeout);
      return _handleResponse(response);
    } on TimeoutException {
      throw ApiException('زمان اتصال تمام شد. لطفاً دوباره تلاش کنید');
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException('خطا در ارتباط با سرور');
    }
  }

  // ─── DELETE ──────────────────────────────────────────────
  static Future<dynamic> delete(String url) async {
    try {
      final response = await http
          .delete(Uri.parse(url), headers: await _headers())
          .timeout(_timeout);
      return _handleResponse(response);
    } on TimeoutException {
      throw ApiException('زمان اتصال تمام شد. لطفاً دوباره تلاش کنید');
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException('خطا در ارتباط با سرور');
    }
  }

  // ─── Response Handler ────────────────────────────────────
  static dynamic _handleResponse(http.Response response) {
    final data = jsonDecode(utf8.decode(response.bodyBytes));

    if (response.statusCode == 401) {
      throw ApiException('session_expired', statusCode: 401);
    }

    if (response.statusCode >= 200 && response.statusCode < 300) {
      return data;
    }

    final message = data['message'] ?? data['errors'] ?? 'خطا در ارتباط با سرور';
    throw ApiException(message is String ? message : message.toString(),
        statusCode: response.statusCode);
  }
}
