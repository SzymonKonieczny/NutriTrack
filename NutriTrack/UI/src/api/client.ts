const API_BASE = import.meta.env.VITE_API_BASE ?? '/api';

interface ApiResponse<T> {
  data: T | null;
  error: string | null;
  status: number;
}

let getAccessToken: () => string | null = () => null;
let getRefreshToken: () => string | null = () => null;
let onRefreshFailed: (() => void) | null = null;
let isRefreshing = false;
let refreshPromise: Promise<boolean> | null = null;

export function configureAuth(config: {
  getAccessToken: () => string | null;
  getRefreshToken: () => string | null;
  onRefreshFailed: () => void;
}) {
  getAccessToken = config.getAccessToken;
  getRefreshToken = config.getRefreshToken;
  onRefreshFailed = config.onRefreshFailed;
}

async function tryRefresh(): Promise<boolean> {
  if (isRefreshing && refreshPromise) {
    return refreshPromise;
  }

  const currentRefreshToken = getRefreshToken();
  if (!currentRefreshToken) return false;

  isRefreshing = true;
  refreshPromise = (async () => {
    try {
      const res = await fetch(`${API_BASE}/Auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          accessToken: getAccessToken(),
          refreshToken: currentRefreshToken,
        }),
      });

      if (!res.ok) return false;

      const data = await res.json();
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      localStorage.setItem('user', JSON.stringify({
        id: data.userId,
        email: data.email,
        name: data.name,
      }));
      return true;
    } catch {
      return false;
    } finally {
      isRefreshing = false;
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

async function request<T>(
  method: string,
  url: string,
  body?: unknown
): Promise<ApiResponse<T>> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  const token = getAccessToken();
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const fullUrl = url.startsWith('http') ? url : `${API_BASE}${url}`;

  let res = await fetch(fullUrl, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });

  // On 401, attempt silent refresh once
  if (res.status === 401 && token) {
    const refreshed = await tryRefresh();
    if (refreshed) {
      headers['Authorization'] = `Bearer ${getAccessToken()}`;
      res = await fetch(fullUrl, {
        method,
        headers,
        body: body ? JSON.stringify(body) : undefined,
      });
    } else {
      onRefreshFailed?.();
      return { data: null, error: 'Session expired. Please log in again.', status: 401 };
    }
  }

  if (res.status === 204) {
    return { data: null, error: null, status: 204 };
  }

  let data: T | null = null;
  let error: string | null = null;

  try {
    if (res.ok) {
      data = (await res.json()) as T;
    } else {
      const errBody = await res.json();
      error = errBody.message || errBody.title || `Request failed (${res.status})`;
      if (errBody.errors && Array.isArray(errBody.errors)) {
        error = errBody.errors.join('; ');
      }
    }
  } catch {
    error = `Request failed (${res.status})`;
  }

  return { data, error, status: res.status };
}

export const api = {
  get: <T>(url: string) => request<T>('GET', url),
  post: <T>(url: string, body?: unknown) => request<T>('POST', url, body),
  put: <T>(url: string, body?: unknown) => request<T>('PUT', url, body),
  delete: (url: string) => request<never>('DELETE', url),
};

export type { ApiResponse };
