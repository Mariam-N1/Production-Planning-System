# Production Planning — front end

Angular 21 + PrimeNG. Talks to the ASP.NET Core API in `BackendProject`.

## Running it

```bash
npm install
ng serve          # http://localhost:4200
```

The API must be running first — see the backend's README. If it isn't,
every screen shows "Could not reach the API".

The API address lives in **one** place: `src/app/shared/api.ts`.

## How it's put together

| Path | What |
|---|---|
| `app/auth/` | token, guards, sign in / up / reset |
| `app/shared/data-screen/` | one table component, used by three screens |
| `app/shared/screen-config.ts` | the three configs that drive it |
| `app/shared/theme.service.ts` | light / dark, via `data-theme` on `<html>` |
| `app/pages/` | one folder per screen |

**Products, Shades and Packing are the same component.** They differ only
by the `ScreenConfig` passed in — columns, endpoint, search fields.

**Theming** is CSS custom properties. `ThemeService` sets one attribute on
`<html>` and everything follows, PrimeNG included.
