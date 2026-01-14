# Contributing to Acumatica Inventory Scanner

Thanks for your interest in contributing! We welcome contributions in **all areas**. By contributing, you agree to follow the Coding Rules below and acknowledge that all contribution rights are transferred to **AcuPower LTD**.

---

## Ground Rules (Coding Rules)
- **Language & Style:** C#/.NET MAUI code should follow the existing patterns: async/await for I/O, `ConfigureAwait(false)` only when necessary, explicit access modifiers, nullable reference types respected, and meaningful naming.
- **Architecture:** Keep business logic in services/models; keep UI logic minimal in pages/viewmodels. Avoid mixing UI with data access.
- **Error Handling:** Prefer graceful handling and user-friendly messages; avoid swallowing exceptions silently.
- **Logging:** When adding logging, prefer concise, actionable messages without leaking secrets.
- **Security:** Do not log secrets; use secure storage for credentials (consistent with `SettingsService`); HTTPS only.
- **Serialization:** Use the existing source-generated JSON context and avoid reflection-based serialization that breaks trimming.
- **Tests:** Add or update tests when you change behavior. Keep tests deterministic and isolated.
- **Dependencies:** Avoid adding heavy dependencies; discuss first if uncertain.
- **Accessibility/UI:** Keep XAML clean and aligned; prefer styles and resources over inline duplication.

---

## Contribution Process
1. **Fork and branch** from `main`.
2. **Open an issue first** for major changes; minor fixes are fine directly with a PR.
3. **Follow templates**: use the provided issue templates and PR template.
4. **Coding rules agreement**: By opening a PR you confirm you agree to the Coding Rules and transfer ownership of submitted ideas/code to AcuPower LTD.
5. **Tests:** Run relevant tests (`dotnet test`) before submitting.
6. **PR reviews:** AcuPower team reviews all PRs. Please respond to feedback promptly.

---

## Commit & PR Guidelines
- Keep commits focused and messages clear.
- Reference related issues in the PR description.
- Describe user-visible changes and testing performed.

---

## Development Quick Notes
- **Tech stack:** .NET MAUI app + WPF barcode generator.
- **JSON:** Use the source-generated `JsonContext` for models; keep `<TrimMode>partial</TrimMode>` intact.
- **Storage:** Credentials/settings via platform secure storage (`SettingsService`).
- **Branding:** Keep the teal barcode scanner icon and splash assets unchanged unless intentionally updating branding.

---

## Code of Conduct
Participation is governed by the [Code of Conduct](CODE_OF_CONDUCT.md).
