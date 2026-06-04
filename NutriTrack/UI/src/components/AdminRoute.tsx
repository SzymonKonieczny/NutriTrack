import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function AdminRoute() {
  const { user, isLoading } = useAuth();

  if (isLoading) {
    return <div className="spinner">Loading...</div>;
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (!user.roles.includes('Admin')) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}