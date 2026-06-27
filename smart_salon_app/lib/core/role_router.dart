/// Maps a backend userType to its home route.
/// SuperAdmin=1, SalonManager=2, Artist=3, Client=4.
String roleHome(int userType) {
  switch (userType) {
    case 1:
      return '/admin-dashboard';
    case 2:
      return '/manager-dashboard';
    case 3:
      return '/artist-dashboard';
    case 4:
      return '/client-dashboard';
    default:
      return '/home';
  }
}
