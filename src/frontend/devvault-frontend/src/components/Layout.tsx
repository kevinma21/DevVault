import { useNavigate, Link, useLocation } from 'react-router-dom';

function Layout({ children }: { children: React.ReactNode }) {
    const navigate = useNavigate();
    const location = useLocation();

    const handleLogout = () => {
        localStorage.removeItem('accessToken');
        navigate('/login');
    }

    const navItems = [
        { name: 'Dashboard', path: '/dashboard' },
        { name: 'Users', path: '/users' },
        { name: 'Audit', path: '/audit' }
    ];

    return (
        <div className="flex min-h-screen bg-slate-950 text-slate-50">
            {/* Sidebar */}
            <aside className="w-64 flex-col border-r border-slate-800 bg-slate-900 flex">
                <div className="flex h-16 items-center justify-center border-b border-slate-800">
                    <h1 className="text-xl font-bold">DevVault</h1>
                </div>

                <nav className="flex-1 space-y-1 px-3 py-4">
                    {navItems.map((item) => {
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
