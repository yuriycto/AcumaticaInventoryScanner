# Security Policy

## Reporting a Vulnerability
- Email: **info@acupowererp.com**
- Please include: affected version, platform (Android/iOS/Windows/macOS), reproduction steps, and impact.
- Do **not** open public issues for vulnerabilities.

## Our Commitments
- We will acknowledge receipt within 5 business days.
- We will provide an initial assessment within 10 business days.
- We will coordinate disclosure timing with you.
- We do **not** offer bug bounties; we provide acknowledgments for responsibly disclosed issues.

## Scope
- .NET MAUI mobile app and WPF barcode generator within this repository.
- Issues with third-party services (e.g., Acumatica ERP itself) are out of scope.

## Secure Development Notes
- Credentials are stored using platform secure storage.
- All network traffic should be HTTPS/TLS only.
- Do not log secrets or tokens.
- Use the source-generated JSON serialization (`JsonContext`) to remain linker-safe.
