import 'dart:convert';

/// Decodes a JWT without verifying the signature (client-side UX only).
/// Signature verification happens on the server — the client trusts the
/// server's response and only uses the payload for UI gating (§R8).
class JwtDecoder {
  JwtDecoder._();

  /// Returns the decoded payload map, or null if the token is malformed.
  static Map<String, dynamic>? decode(String token) {
    try {
      final parts = token.split('.');
      if (parts.length != 3) return null;

      // Base64url-decode the payload (second part)
      var payload = parts[1];
      // Pad to a multiple of 4
      switch (payload.length % 4) {
        case 2:
          payload += '==';
          break;
        case 3:
          payload += '=';
          break;
      }

      final decoded = utf8.decode(base64Url.decode(payload));
      return json.decode(decoded) as Map<String, dynamic>;
    } catch (_) {
      return null;
    }
  }

  /// Extracts the list of 'permission' claims from a JWT payload.
  /// The server embeds one claim per permission (Task 3.2).
  static Set<String> extractPermissions(String token) {
    final payload = decode(token);
    if (payload == null) return {};

    final raw = payload['permission'];
    if (raw == null) return {};

    // JWT libraries may encode multiple same-key claims as an array or a single string
    if (raw is List) {
      return raw.whereType<String>().toSet();
    } else if (raw is String) {
      return {raw};
    }
    return {};
  }

  /// Returns true when the 'is_platform_owner' claim is 'true'.
  static bool isPlatformOwner(String token) {
    final payload = decode(token);
    return payload?['is_platform_owner'] == 'true';
  }

  /// Returns the role claim value (e.g. 'Artist', 'Client').
  static String? role(String token) {
    final payload = decode(token);
    return payload?['role'] as String?;
  }
}
