# Security Policy

## Supported versions

The most recent released version receives security fixes. Older tags do not.

## Reporting a vulnerability

Please do **not** open a public GitHub issue for a security problem.

Report privately through
[GitHub Security Advisories](https://github.com/antoniopereirabuilds/OutSystems.Extension.TimeZoneToolkit/security/advisories/new),
which opens a channel visible only to the maintainers.

Include the affected version, the action involved, the input that triggers the
issue, and what you observed. A minimal reproduction against the published
package helps most.

Expect an acknowledgement within 5 working days and an assessment within 15.
Confirmed issues are fixed in a new tagged release and disclosed in the advisory
once the fix is published.

## Scope

This is a pure-computation library: it converts date and time values between
time zones. It performs no network, filesystem, or database access, handles no
credentials, and holds no state between invocations.

In scope:

- Input that causes an unhandled .NET exception to escape instead of a
  `TimeZoneToolkitException`
- Input that causes disproportionate CPU or memory use
- A conversion that returns a silently wrong result
- Anything in the release pipeline that would let an unreviewed build be
  published or attested

Out of scope:

- Time zone rule changes in the IANA database itself; the library reports
  whatever tzdata the container image carries
- How a consuming OutSystems app renders `ExceptionMessage`. The library echoes
  the caller's own input back in error messages; an app that renders that with
  **Escape Content = No** is responsible for its own escaping

## Verifying a release

Release artifacts carry a Sigstore provenance attestation:

```bash
gh attestation verify TimeZoneToolkit_vX.Y.Z.zip \
  --repo antoniopereirabuilds/OutSystems.Extension.TimeZoneToolkit
```

The attestation is the binding check. The published `.sha256` file is a
convenience for detecting transfer corruption, not a security control on its own.
