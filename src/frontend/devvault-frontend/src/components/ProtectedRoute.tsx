import { Navigate } from 'react-router-dom';

// We reuse the same token decoder logic here
const parseJwt = (token: string) => {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        return JSON.parse(jsonPayload);
    } catch (e) {
        console.log(e);
        return null;
    }
};

interface ProtectedRouteProps {
    children: React.ReactNode;
    requiredRole?: string;
}

export default function ProtectedRoute({ children, requiredRole }: ProtectedRouteProps) {
    const token = localStorage.getItem('accessToken');

    if (!token) {
        return <Navigate to="/login" replace />;
    }

    if (requiredRole) {
        const decoded = parseJwt(token);
        const roleClaim = decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded?.role;
        
        let hasRole;
        if (Array.isArray(roleClaim)) {
            hasRole = roleClaim.includes(requiredRole);
        } else {
            hasRole = roleClaim === requiredRole;
        }

       
        if (!hasRole) {
            return <Navigate to="/dashboard" replace />;
        }
    }

    return <>{children}</>;
}