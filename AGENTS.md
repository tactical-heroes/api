Keep scope narrow. Full solution checks only for shared contracts, build files, CI, or cross-module changes.
For optional constructor parameters in Minimal API request DTOs, always specify an explicit default (e.g. `string? Note = null`). Nullable annotations alone do not make JSON fields optional.
