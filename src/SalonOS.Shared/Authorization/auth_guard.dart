// Auth guard helper for authorization checks
// This file is part of the authentication system.
//
// It provides helper methods for checking user permissions and roles.
//
// Example usage:
//   if (await AuthGuard.isAuthorized(user, 'Booking')) { ... }
//
class AuthGuard {
  // Check if the user is authorized for a specific permission
  static Future<bool> isAuthorized(dynamic user, String permission) async {
    // Implementation would go here
    return false;
  }
}
