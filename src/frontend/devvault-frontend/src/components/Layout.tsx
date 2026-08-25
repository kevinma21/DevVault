import { useNavigate, Link, useLocation } from 'react-router-dom';

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

function Layout({ children }: { children: React.ReactNode }) {
    const navigate = useNavigate();
    const location = useLocation();

    const handleLogout = () => {
        localStorage.removeItem('accessToken');
        navigate('/login');
    }

    const token = localStorage.getItem('accessToken');
    let isAdmin = false;

    if (token) {
        const decoded = parseJwt(token);
        const roleClaims = decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded?.role;
        
        if (Array.isArray(roleClaims)) {
            isAdmin = roleClaims.includes('Administrator');
        } else {
            isAdmin = roleClaims === 'Administrator';
        }
    }
    const navItems = [
        { name: 'Project', path: '/projects' },
        { name: 'Users', path: '/users' },
        { name: 'Audit', path: '/audit' }
    ];

    const visibleNavItems = navItems.filter(item => {
        if (item.name === 'Users' && !isAdmin) {
            return false;
        }
        return true;
    })

    return (
        <div className="flex min-h-screen bg-slate-950 text-slate-50">
            {/* Sidebar */}
            <aside className="w-64 flex-col border-r border-slate-800 bg-slate-900 flex">
                <div className="flex h-16 items-center justify-center border-b border-slate-800">
                    <h1 className="text-xl font-bold">DevVault</h1>
                </div>

                <nav className="flex-1 space-y-1 px-3 py-4">
                    {visibleNavItems.map((item) => {
                        const isActive = location.pathname === item.path;
                        return (
                            <Link
                                key={item.name}
                                to={item.path}
                                className={`block rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                                    isActive ? 'bg-slate-800 text-emerald-500' 
                                            : 'text-slate-400 hover:bg-slate-800 hover:text-slate-50'
                                }`}
                            >
                                {item.name}
                            </Link>
                        );
                    })}
                </nav>
            
                <div className="border-t border-slate-800 p-4">
                    <button 
                        onClick={handleLogout}
                        className="block rounded-md px-3 py-2 text-sm font-medium text-slate-400 hover:bg-slate-800 hover:text-slate-50"
                    >
                        Sign Out
                    </button>
                </div>
            </aside>

            {/* Main Content */}
            <main className="flex-1 flex flex-col min-w-0 overflow-hidden">
                    <div className="flex-1 overflow-y-auto p-6">
                        {children}
                    </div>
            </main>
        </div>
    )
}

export default Layout;
