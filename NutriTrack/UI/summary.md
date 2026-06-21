# NutriTrack UI — Developer Summary

## Architecture Overview

```
src/
├── api/
│   └── client.ts           # Typed HTTP client (fetch wrapper)
├── context/
│   └── AuthContext.tsx       # Auth state provider + useAuth() hook
├── components/               # Reusable shell/layout components
│   ├── Layout.tsx            # App shell: sticky header + <Outlet />
│   ├── ProtectedRoute.tsx    # Gate: redirects to /login if unauthenticated
│   ├── AdminRoute.tsx        # Gate: redirects non-Admins to /
│   └── AdminLayout.tsx       # Sidebar + <Outlet /> for admin pages
├── pages/                    # Route-level page components
│   ├── LoginPage.tsx
│   ├── RegisterPage.tsx
│   ├── DashboardPage.tsx
│   ├── MealLogsPage.tsx
│   ├── RecipesPage.tsx       # Non-admin: browse + log meals + view nutrition
│   └── admin/
│       ├── AdminDashboard.tsx
│       ├── IngredientsPage.tsx
│       ├── MicronutrientsPage.tsx
│       ├── RecipesPage.tsx    # Admin CRUD for recipes (imported as AdminRecipesPage)
│       ├── RecipeDetailPage.tsx
│       └── IngredientDetailPage.tsx
├── App.tsx                   # Route definitions (all routing lives here)
├── main.tsx                  # Entry point (StrictMode + App)
└── index.css                 # Global styles, design system tokens, utility classes
```

## Router Layout (React Router v6 — Outlet-based)

```
<BrowserRouter>
  <AuthProvider>               ← context from context/AuthContext
    <Routes>
      /login          → LoginPage       (public)
      /register       → RegisterPage    (public)

      /               → ProtectedRoute  (gate)
        → Layout                      (shell: header + <Outlet/>)
          index         → DashboardPage
          meal-logs     → MealLogsPage
          recipes       → RecipesPage         (non-admin browse + log)
          admin         → AdminRoute         (gate)
            → AdminLayout                   (sidebar + <Outlet/>)
              index             → AdminDashboard
              ingredients       → IngredientsPage
              ingredients/:id   → IngredientDetailPage
              micronutrients    → MicronutrientsPage
              recipes           → AdminRecipesPage  (admin CRUD)
              recipes/:id       → RecipeDetailPage
    </Routes>
  </AuthProvider>
</BrowserRouter>
```

Key: `ProtectedRoute` and `AdminRoute` use `<Outlet />` (not `children`). They wrap a `<Route element={...}>` in the route tree, not the JSX. This means the gate components render the header/sidebar only when access is granted.

### How a page load flows:

1. `App.tsx` matches the route
2. `ProtectedRoute` checks `useAuth().user` — if null → `<Navigate to="/login">`
3. `Layout` renders the sticky header with nav links, then `<Outlet />`
4. For admin paths, `AdminRoute` checks for the Admin role → redirects to `/` if missing
5. `AdminLayout` renders the sidebar + `<Outlet />` for the actual admin page

## Data Flow

```
User Action → Page Component → api.get/post/put/delete() → Fetch API → .NET Backend
                                    ↑
                            client.ts intercepts 401
                            → silent refresh via /Auth/refresh
                            → retries original request once
                            → or calls onRefreshFailed() (logs out)
```

- `api/client.ts` reads `localStorage` for tokens on every call via the `configureAuth()` callbacks
- `AuthContext.tsx` calls `configureAuth()` once on mount to wire up the callbacks
- On app load, `AuthContext` tries `POST /Auth/refresh` to restore the session; if that fails it decodes the stored JWT anyway (it might still be valid)
- The `AuthResponse` from login/register contains `userId`, `email`, `name`, `accessToken`, `refreshToken`, `accessTokenExpiresAtUtc`

## Design Choices

### Why no state management library?
The app has one piece of global state: the current user. React Context handles this perfectly. All other state is local to pages (list data, form state, modal state). Adding Redux/Zustand would be over-engineering.

### Why fetch instead of axios?
No extra dependency. The token refresh logic is ~40 lines and fetch is natively available. Axios interceptors are slightly cleaner but not worth the bundle size.

### Why `<Outlet />` instead of `<Layout><children/></Layout>`?
Outlet-based routing means the gate components (`ProtectedRoute`, `AdminRoute`) are actual `<Route>` elements. This keeps the URL/auth check in the router layer rather than mixing it into page components. If someone navigates directly to `/admin/ingredients` while logged out, the route tree naturally redirects them.

### Why no custom hooks for CRUD?
The admin pages (Ingredients, Micronutrients, Recipes) follow an almost identical pattern. Extracting a `useCrud<T>` hook would save ~40% boilerplate. I didn't do it in v1 because it adds indirection and the pages have slightly different shapes (different fields, different modal layouts). **Recommendation:** If you add a 4th admin entity, extract the common pattern into `hooks/useCrud.ts`.

### Style approach: plain CSS with custom properties
No Tailwind, no CSS-in-JS, no component library. The app is small enough that a well-organized single stylesheet with CSS custom properties is faster to iterate and zero build overhead. The tradeoff: as the app grows, styles could become unwieldy. If that happens, migrate to CSS Modules or Tailwind.

### Color palette
- Emerald 600 (#059669) → Teal 600 (#0d9488) gradient for primary actions and header
- Amber for accent highlights (sparingly)
- Warm gray scale (Stone from Tailwind palette) for UI chrome
- All colors exist as `--color-*` custom properties in `index.css`

### Responsive strategy
- Mobile-first CSS with one breakpoint at 768px
- Admin sidebar collapses to horizontal strip on mobile
- Dashboard grid uses `auto-fill, minmax(280px, 1fr)`
- Tables scroll horizontally on small screens via `.table-container { overflow-x: auto }`

## Tips for Future You

### Adding a new page
1. Create the file in `pages/` (or `pages/admin/` if admin-only)
2. Add a `<Route>` in `App.tsx` inside the right route group
3. Add a nav link in `Layout.tsx` or `AdminLayout.tsx`

### Adding a new entity with admin CRUD
Copy-paste one existing admin page (e.g. `IngredientsPage.tsx`) and adapt:
- The `interface` for the entity
- API endpoints (`/api/your-entity`)
- Form fields in the modal
- Table columns

Then also copy `RecipeDetailPage.tsx` / `IngredientDetailPage.tsx` if you need nested CRUD associations.

### Token lifecycle
- Access token: stored in `localStorage('accessToken')`, sent as `Authorization: Bearer`
- Refresh token: stored in `localStorage('refreshToken')`, sent to `/Auth/refresh`
- On 401 response: client.ts attempts one silent refresh, retries the original request
- On refresh failure: `onRefreshFailed()` fires → clears auth state → user is redirected to `/login`
- **Important:** The refresh endpoint expects `{ accessToken, refreshToken }` body, not just the refresh token alone

### JWT payload structure
The backend uses ASP.NET Core Identity claims:
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name` → email
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier` → user ID
- `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` → string or string[]
- The API client parses these in `AuthContext.tsx` → `getUserFromToken()`

### CORS
Backend allows only `http://localhost:5173` (Vite dev default). If you deploy or change ports:
- Update the origin in `NutriTrackerAPI/Program.cs` `AddCors()` call
- Update `API_BASE` in `api/client.ts`
- The backend uses `AllowCredentials()` so you CAN send auth cookies if you ever switch to cookie-based auth

### Things to watch out for
- The React Compiler Babel plugin is enabled in `vite.config.ts`. It auto-memoizes components. If you see stale closures in callbacks, it's likely the React Compiler optimizing too aggressively — add `"use no memo"` at the top of the file to disable it locally.
- `tsconfig.app.json` has `erasableSyntaxOnly: true` — you cannot use `enum`, `namespace`, or parameter properties. Use `const` objects or union types instead.
- The API uses `Guid` route constraints (`{id:guid}`). The client sends GUIDs as plain strings.
- The `/refresh` endpoint is `[AllowAnonymous]` — it does NOT need a valid access token, only a valid refresh token. The client sends both, but technically only the refresh token is required by the server.

### Potential improvements
- Extract `useCrud<T>` hook for the 3 isomorphic admin pages
- Add toast notifications for create/update/delete success
- Add form validation that shows per-field errors (currently all errors go to a banner)
- Pagination for meal logs (the API returns all, which won't scale)
- Replace emoji icons with SVG icons for better consistency
- Add tests (Vitest + Testing Library)
- Add an admin user management page (user list, role assignment)