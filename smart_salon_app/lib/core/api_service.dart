import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class ApiService {
  // ─── ذخیره و خواندن توکن ─────────────────────────────
  static Future<void> saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('token', token);
  }

  static Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('token');
  }

  static Future<void> clearToken() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('token');
  }

  // ─── ساخت Header با توکن ─────────────────────────────
  static Future<Map<String, String>> _headers() async {
    final token = await getToken();
    return {
      'Content-Type': 'application/json; charset=utf-8',
      'Accept': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  // ─── GET ──────────────────────────────────────────────
  static Future<dynamic> get(String url) async {
    final response = await http.get(
      Uri.parse(url),
      headers: await _headers(),
    );
    return _handleResponse(response);
  }

  // ─── POST ─────────────────────────────────────────────
  static Future<dynamic> post(String url, Map<String, dynamic> body) async {
    final headers = await _headers();
    print('Headers: $headers');  // برای دیباگ
    print('URL: $url');          // برای دیباگ

    final response = await http.post(
      Uri.parse(url),
      headers: await _headers(),
      body: jsonEncode(body),
    );
    return _handleResponse(response);
  }
  

  // ─── PUT ──────────────────────────────────────────────
  static Future<dynamic> put(String url, Map<String, dynamic> body) async {
    final response = await http.put(
      Uri.parse(url),
      headers: await _headers(),
      body: jsonEncode(body),
    );
    return _handleResponse(response);
  }

  static Future<dynamic> delete(String url) async {
  final headers = await _headers();
  final response = await http.delete(
    Uri.parse(url),
    headers: headers,
  );
  return _handleResponse(response);
}

  // ─── پردازش پاسخ سرور ────────────────────────────────
  static dynamic _handleResponse(http.Response response) {
    final data = jsonDecode(utf8.decode(response.bodyBytes));

    if (response.statusCode >= 200 && response.statusCode < 300) {
      return data;
    }

    // خطا
    throw Exception(data['message'] ?? 'خطا در ارتباط با سرور');
  }
}