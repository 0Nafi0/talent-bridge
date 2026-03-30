import React, { createContext, useContext, useState, useEffect } from 'react';
import { jwtDecode } from 'jwt-decode';
import api from '../api/axios';

const AuthContext = createContext();

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        // Hydrate token on hard refresh
        const token = localStorage.getItem('token');
        if (token) {
            try {
                const decodedToken = jwtDecode(token);
                if (decodedToken.exp * 1000 < Date.now()) {
                    logout(); // Expired token
                } else {
                    const roleClaimUrl = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
                    setUser({
                        id: decodedToken.sub,
                        email: decodedToken.email,
                        fullName: decodedToken.fullName,
                        role: decodedToken[roleClaimUrl] || decodedToken.userRole
                    });
                }
            } catch (err) {
                logout();
            }
        }
        setLoading(false);
    }, []);

    const login = async (email, password) => {
        const response = await api.post('/auth/login', { email, password });
        const { accessToken } = response.data;
        
        localStorage.setItem('token', accessToken);
        const decodedToken = jwtDecode(accessToken);
        const roleClaimUrl = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        
        setUser({
            id: decodedToken.sub,
            email: decodedToken.email,
            fullName: decodedToken.fullName,
            role: decodedToken[roleClaimUrl] || decodedToken.userRole
        });
        
        return response.data;
    };

    const register = async (userData) => {
        const response = await api.post('/auth/register', userData);
        return response.data;
    };

    const logout = () => {
        // Optional Backend Call to invalidate Security Stamp
        api.post('/auth/logout').catch(() => {});
        localStorage.removeItem('token');
        setUser(null);
    };

    return (
        <AuthContext.Provider value={{ user, login, register, logout, loading }}>
            {!loading && children}
        </AuthContext.Provider>
    );
};
