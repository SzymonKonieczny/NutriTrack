# NutriTrack UI — Implementation Plan

## Phase 1: Backend CORS fix

Add a CORS policy in `Program.cs` that allows the Vite dev server origin so the browser doesn't block requests.

## Phase 2: UI Architecture & Dependencies

Install `react-router-dom` as the only new dependency.

Build 8 core groups under `UI/src/`:

```
UI/src/
├── main.tsx                        # Entry — wraps App with AuthProvider + BrowserRouter
├── App.tsx                         # Route definitions (public, user, admin shells)
├── index.css                       # Global styles (theme, reset, utility classes)
│
├── api/
│   └── client.ts                   # Fetch wrapper — auto-attaches JWT, handles refresh, typed helpers
│
├── context/
│   └── AuthContext.tsx              # AuthProvider + useAuth() hook — login/register/logout, token storage, user state
│
├── components/
│   ├── Layout.tsx                  # App shell: header with nav, main content area
│   ├── ProtectedRoute.tsx          # Redirects to /login if unauthenticated
│   ├── AdminRoute.tsx              # Redirects to / if user lacks Admin role
│   └── AdminLayout.tsx             # Sidebar + top nav for admin area
│
└── pages/
    ├── LoginPage.tsx               # Email/password form, link to register
    ├── RegisterPage.tsx            # Name/email/password/confirm form, link to login
    ├── DashboardPage.tsx           # Welcome screen showing today's meal summary
    ├── MealLogsPage.tsx            # List all own meal logs, create/edit/delete inline
    ├── admin/
    │   ├── AdminDashboard.tsx      # Stats overview (counts of entities)
    │   ├── IngredientsPage.tsx     # Table + CRUD modal for ingredients
    │   ├── MicronutrientsPage.tsx  # Table + CRUD modal for micronutrients
    │   ├── RecipesPage.tsx         # Table + CRUD modal for recipes
    │   ├── RecipeDetailPage.tsx    # Manage ingredients within a specific recipe
    │   └── IngredientDetailPage.tsx# Manage micronutrients within a specific ingredient
```

## Phase 3: App Routing

| Path | Component | Guard |
|------|-----------|-------|
| `/login` | LoginPage | Public |
| `/register` | RegisterPage | Public |
| `/` | DashboardPage | Authenticated |
| `/meal-logs` | MealLogsPage | Authenticated |
| `/admin` | AdminDashboard | Admin |
| `/admin/ingredients` | IngredientsPage | Admin |
| `/admin/micronutrients` | MicronutrientsPage | Admin |
| `/admin/recipes` | RecipesPage | Admin |
| `/admin/recipes/:recipeId` | RecipeDetailPage | Admin |
| `/admin/ingredients/:ingredientId` | IngredientDetailPage | Admin |

## Phase 4: API Client Design

A typed fetch wrapper (`api/client.ts`) that:

1. Stores access token and refresh token in `localStorage`
2. Reads tokens from context on each call
3. On 401, attempts silent refresh before retrying once
4. Provides typed helpers: `api.get<T>(url)`, `api.post<T>(url, body)`, `api.put<T>(url, body)`, `api.delete(url)`
5. All return `{data, error, status}` tuples so components can handle loading/error states uniformly

## Phase 5: Auth Flow

- `AuthContext` stores `user: {id, email, name, roles} | null` + `accessToken`
- `login()` calls POST `/api/auth/login`, stores tokens + user info decoded from JWT
- `register()` calls POST `/api/auth/register`, same response shape as login
- `logout()` calls POST `/api/auth/logout`, clears localStorage, redirects to /login
- On app mount, tries token refresh if an access token exists (restores session)
- `ProtectedRoute` checks `user !== null`; `AdminRoute` checks `user.roles.includes('Admin')`

## Phase 6: Component States

Every data-fetching component handles:
- **Loading**: Skeleton/spinner placeholder
- **Empty**: Informational message with call-to-action (e.g., "No meal logs yet. Log your first meal!")
- **Error**: Error banner with retry button
- **Success**: Normal data rendering

## Phase 7: Design Direction

Modern health/fitness app aesthetic:
- **Primary palette**: Emerald green (#059669) → Teal (#0d9488) gradient
- **Accent**: Amber (#f59e0b) for highlights
- **Background**: Warm white (#fafaf9) with subtle gray cards (#ffffff with shadows)
- **Typography**: System font stack (clean, fast)
- **Components**: Rounded cards, soft shadows, subtle borders, generous spacing
- **Forms**: Clean inputs with floating labels, validation messages inline
- **Tables**: Striped rows, sticky headers, action buttons as icon-only
- **Responsive**: Mobile-first, collapsible sidebar for admin, full-width for user pages

## Implementation order (files to create)

1. `index.css` — full theme
2. `api/client.ts` — HTTP layer
3. `context/AuthContext.tsx` — auth state
4. `components/Layout.tsx` + `ProtectedRoute.tsx` + `AdminRoute.tsx` + `AdminLayout.tsx`
5. `LoginPage.tsx` + `RegisterPage.tsx`
6. `DashboardPage.tsx`
7. `MealLogsPage.tsx`
8. Admin pages (5 files)
9. `App.tsx` + `main.tsx` — wire everything

Total: ~16 source files, 1 backend edit, 1 npm install