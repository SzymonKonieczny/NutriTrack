import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import { api, configureAuth } from '../api/client';

interface User {
  id: string;
  email: string;
  name: string | null;
  roles: string[];
}

interface AuthState {
  user: User | null;
  accessToken: string | null;
  isLoading: boolean;
}

interface AuthContextType extends AuthState {
  login: (email: string, password: string) => Promise<string | null>;
  register: (email: string, password: string, name?: string) => Promise<string | null>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

function parseJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    const decoded = atob(parts[1].replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(decoded);
  } catch {
    return null;
  }
}

function getRolesFromToken(payload: Record<string, unknown>): string[] {
  const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  if (Array.isArray(roleClaim)) return roleClaim as string[];
  if (typeof roleClaim === 'string') return [roleClaim];
  return [];
}

function getUserFromToken(token: string): User | null {
  const payload = parseJwtPayload(token);
  if (!payload) return null;

  return {
    id: (payload['sub'] as string) || '',
    email: (payload['email'] as string) || '',
    name: null,
    roles: getRolesFromToken(payload),
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    accessToken: null,
    isLoading: true,
  });

  const clearAuth = useCallback(() => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    setState({ user: null, accessToken: null, isLoading: false });
  }, []);

  const tryRestoreSession = useCallback(async () => {
    const storedToken = localStorage.getItem('accessToken');
    const storedRefresh = localStorage.getItem('refreshToken');

    if (!storedToken || !storedRefresh) {
      setState({ user: null, accessToken: null, isLoading: false });
      return;
    }

    // Try a silent refresh to verify the session is still valid
    try {
      const res = await api.post<{
        userId: string;
        email: string;
        name: string | null;
        accessToken: string;
        refreshToken: string;
      }>('/Auth/refresh', {
        accessToken: storedToken,
        refreshToken: storedRefresh,
      });

      if (res.data) {
        localStorage.setItem('accessToken', res.data.accessToken);
        localStorage.setItem('refreshToken', res.data.refreshToken);
        const user = getUserFromToken(res.data.accessToken);
        if (user) {
          user.name = res.data.name;
          setState({ user, accessToken: res.data.accessToken, isLoading: false });
          return;
        }
      }
    } catch {
      // refresh failed
    }

    // Fallback: try to decode the stored token (might still be valid)
    const user = getUserFromToken(storedToken);
    if (user) {
      setState({ user, accessToken: storedToken, isLoading: false });
    } else {
      clearAuth();
    }
  }, [clearAuth]);

  // Configure the API client auth hooks
  useEffect(() => {
    configureAuth({
      getAccessToken: () => localStorage.getItem('accessToken'),
      getRefreshToken: () => localStorage.getItem('refreshToken'),
      onRefreshFailed: () => {
        clearAuth();
      },
    });
  }, [clearAuth]);

  // Restore session on mount
  useEffect(() => {
    tryRestoreSession();
  }, [tryRestoreSession]);

  const login = useCallback(async (email: string, password: string): Promise<string | null> => {
    const res = await api.post<{
      userId: string;
      email: string;
      name: string | null;
      accessToken: string;
      refreshToken: string;
    }>('/Auth/login', { email, password });

    if (res.error) return res.error;
    if (!res.data) return 'Login failed';

    localStorage.setItem('accessToken', res.data.accessToken);
    localStorage.setItem('refreshToken', res.data.refreshToken);
    localStorage.setItem('user', JSON.stringify({
      id: res.data.userId,
      email: res.data.email,
      name: res.data.name,
    }));

    const user = getUserFromToken(res.data.accessToken);
    if (user) {
      user.name = res.data.name;
      setState({ user, accessToken: res.data.accessToken, isLoading: false });
    }

    return null;
  }, []);

  const register = useCallback(async (email: string, password: string, name?: string): Promise<string | null> => {
    const res = await api.post<{
      userId: string;
      email: string;
      name: string | null;
      accessToken: string;
      refreshToken: string;
    }>('/Auth/register', { email, password, name });

    if (res.error) return res.error;
    if (!res.data) return 'Registration failed';

    localStorage.setItem('accessToken', res.data.accessToken);
    localStorage.setItem('refreshToken', res.data.refreshToken);
    localStorage.setItem('user', JSON.stringify({
      id: res.data.userId,
      email: res.data.email,
      name: res.data.name,
    }));

    const user = getUserFromToken(res.data.accessToken);
    if (user) {
      user.name = res.data.name;
      setState({ user, accessToken: res.data.accessToken, isLoading: false });
    }

    return null;
  }, []);

  const logout = useCallback(async () => {
    await api.delete('/Auth/logout');
    clearAuth();
  }, [clearAuth]);

  return (
    <AuthContext.Provider value={{ ...state, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}