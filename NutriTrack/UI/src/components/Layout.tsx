import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Layout() {
  const { user, logout } = useAuth();
  const initials = user?.name
    ? user.name.charAt(0).toUpperCase()
    : user?.email.charAt(0).toUpperCase() ?? '?';

  return (
    <div className="app-layout">
      <header className="top-header">
        <NavLink to="/" className="logo" end>
          <span className="logo-icon">🥗</span>
          NutriTrack
        </NavLink>

        <nav>
          {user && (
            <>
              <NavLink to="/" end className={({ isActive }) => isActive ? 'active' : ''}>
                Dashboard
              </NavLink>
              <NavLink to="/meal-logs" className={({ isActive }) => isActive ? 'active' : ''}>
                Meal Logs
              </NavLink>
              <NavLink to="/recipes" className={({ isActive }) => isActive ? 'active' : ''}>
                Recipes
              </NavLink>
              {user.roles.includes('Admin') && (
                <NavLink to="/admin" className={({ isActive }) => isActive ? 'active' : ''}>
                  Admin
                </NavLink>
              )}
            </>
          )}
        </nav>

        <div className="user-info">
          {user && (
            <>
              <span className="user-name">{user.name || user.email}</span>
              <div className="user-avatar">{initials}</div>
              <button className="btn btn-ghost" style={{ color: 'rgba(255,255,255,0.85)' }} onClick={logout}>
                Logout
              </button>
            </>
          )}
        </div>
      </header>

      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}