TimeZone Toolkit adds time zone conversion and lookup to OutSystems Developer Cloud apps. Convert a date and time between two zones, into UTC, or out of UTC. Look up the details of a zone, list every available zone for a dropdown, read the server zone, and translate between Windows and IANA naming.

Identifiers accept both styles, so "GMT Standard Time" and "Europe/Lisbon" work equally well, which suits apps that store the IANA name a browser reports. Every action validates its input and raises an exception that names the parameter to fix, so bad data fails loudly instead of quietly returning the wrong instant. Daylight saving transitions are handled explicitly: times that a clock change skips are rejected rather than guessed.

Built on .NET 10 and TimeZoneConverter, covering 419 time zones, with 79 unit tests at 100 percent line and branch coverage. MIT licensed.

Ideal for apps that store timestamps in UTC and show them in each user's own local time.
