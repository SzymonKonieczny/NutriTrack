import { NavLink, Outlet } from 'react-router-dom';

const adminLinks = [
  { to: '/admin', label: 'Overview', icon: '📊', end: true },
  { to: '/admin/ingredients', label: 'Ingredients', icon: '🥕', end: false },
  { to: '/admin/micronutrients', label: 'Micronutrients', icon: '🔬', end: false },
  { to: '/admin/recipes', label: 'Recipes', icon: '📖', end: false },
];

export default function AdminLayout() {
  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <h3>Administration</h3>
        <nav>
          {adminLinks.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              end={link.end}
              className={({ isActive }) => isActive ? 'active' : ''}
            >
              <span className="sidebar-icon">{link.icon}</span>
              {link.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="admin-main">
        <Outlet />
      </div>
    </div>
  );
}