import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import type { User, LoginResponse } from '@/types';

interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
  getUser: () => User | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function decodeJWT(token: string): Record<string, unknown> | null {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    const stored = localStorage.getItem('auth_user');
    if (stored) {
      try {
        return JSON.parse(stored);
      } catch {
        return null;
      }
    }
    return null;
  });

  const [token, setToken] = useState<string | null>(() => {
    return localStorage.getItem('auth_token');
  });

  const isAuthenticated = Boolean(token && user);

  useEffect(() => {
    if (token && !user) {
      const decoded = decodeJWT(token);
      if (decoded) {
        const extractedUser: User = {
          id: (decoded.sub as string) || (decoded.id as string) || '',
          username: (decoded.username as string) || '',
          email: (decoded.email as string) || '',
          role: (decoded.role as 'admin' | 'superadmin') || 'admin',
          createdAt: (decoded.iat as string) || new Date().toISOString(),
        };
        setUser(extractedUser);
        localStorage.setItem('auth_user', JSON.stringify(extractedUser));
      } else {
        logout();
      }
    }
  }, [token, user]);

  const login = useCallback(async (username: string, password: string) => {
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password }),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Login failed' }));
      throw new Error(error.message || 'Login failed');
    }

    const data: LoginResponse = await response.json();
    localStorage.setItem('auth_token', data.token);
    localStorage.setItem('auth_user', JSON.stringify(data.user));
    setToken(data.token);
    setUser(data.user);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_user');
    setToken(null);
    setUser(null);
  }, []);

  const getUser = useCallback(() => user, [user]);

  return React.createElement(
    AuthContext.Provider,
    { value: { user, token, isAuthenticated, login, logout, getUser } },
    children
  );
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

export function getToken(): string | null {
  return localStorage.getItem('auth_token');
}
