# Agent Tasks — SalonOS plan reconciliation

Hand these to the local agent (deepseek-coder) **one file at a time, in order**.
Each file = ONE small change. Don't give the agent more than one file at once.

Flags: 🟢 safe · 🟡 do it, then you review · 🔴 do NOT give to the agent — bring back to Claude.

## Order

| # | File | What | Flag |
|---|------|------|------|
| 01 | `01-manager-can-book.md` | SalonManager gets `appointment.create` (book-on-behalf) | 🟢 |
| 02 | `02-remove-receptionist-perms.md` | Delete Receptionist permission block | 🟢 |
| 03 | `03-deprecate-receptionist-enum.md` | Mark `MembershipRole.Receptionist` deprecated | 🟢 |
| 04 | `04-clean-receptionist-comments.md` | Remove Receptionist from 3 comments | 🟢 |
| 05 | `05-login-remap-receptionist.md` | Legacy Receptionist logs in as SalonManager | 🟡 review |
| 06 | `06-rls-add-tables.md` | Add ArtistSchedules + Leaves to RLS policy | 🟢 |
| 07 | `07-rls-apply-on-startup.md` | Run AddRLS.sql automatically on boot | 🟡 review |
| 08 | `08-rls-audit.md` | List tenant tables not under RLS (output only) | 🟢 |
| 09 | `09-jobseeker-permissions.md` | Add JobSeeker permission constants | 🟢 |
| 10 | `10-jobseeker-flag.md` | Add `JobSeekerEnabled` flag + migration | 🟡 review |
| 11 | `11-verify-no-gateway-leak.md` | Check no gateway SDK outside payments | 🟢 |
| 12 | `12-build.md` | `dotnet build` to confirm everything compiles | 🟢 |

## Do NOT give to the agent (bring back to Claude)

- **C1 — automated tenant query filter (Layer 2).** EF allows one filter per entity, must merge with
  soft-delete + PlatformOwner bypass + regenerate migrations. Too interdependent for a weak model.
- **D3 — JobPosting / JobApplication module.** A whole new location-scoped bounded context. Needs an
  atomic split first.

## Rule for the agent (paste at top of each task if needed)
> Make ONLY the change described. Do not refactor, rename, or edit other files.
> If the "Find" text is not found exactly, STOP and report — do not guess.
