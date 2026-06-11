class Slot {
  final String start;
  final String end;
  final DateTime startFull;

  Slot({
    required this.start,
    required this.end,
    required this.startFull,
  });

  factory Slot.fromJson(Map<String, dynamic> json) {
    return Slot(
      start: json['start'] ?? '',
      end: json['end'] ?? '',
      startFull: DateTime.parse(json['startFull']),
    );
  }
}
