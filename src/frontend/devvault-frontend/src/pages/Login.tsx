import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';
import axios from 'axios';

function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const navigate = useNavigate();

  const handleSubmit = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    try 
    {
        // send the login request to the backend
        const response = await api.post('/auth/login', { 
            email, 
            password 
        });

        // extract the access token from the response
        const token = response.data.data.accessToken;

        // save localstorage in the browser
        localStorage.setItem('accessToken', token);

        // redirect to the dashboard
        navigate('/dashboard');

    } 
    catch (err: unknown) 
    {
        // Handle failed logins (e.g., wrong password, 401 Unauthorized)
        if (axios.isAxiosError(err)) {
            const message = err.response?.data?.message || 'Authentication failed.';
            setError(message);
        } else {
            setError('An unexpected error occurred. Is the backend running?');
        }
    } 
    finally 
    {
        setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-950 p-4">
      {/* The Login Card */}
      <div className="w-full max-w-md rounded-xl border border-slate-800 bg-slate-900 p-8 shadow-2xl">
        
        {/* Header */}
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold text-slate-50">DevVault</h1>
          <p className="mt-2 text-sm text-slate-400">Sign in to manage your secrets</p>
        </div>

        {/* Error Banner (Hidden by default) */}
        {error && (
          <div className="mb-6 rounded-md bg-rose-500/10 p-4 text-sm text-rose-500 border border-rose-500/20">
            {error}
          </div>
        )}

        {/* The Form */}
        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">
              Email Address
            </label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-4 py-2.5 text-slate-50 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="admin@devvault.local"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">
              Password
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-4 py-2.5 text-slate-50 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="••••••••••••••••"
              required
            />
          </div>

          <button
            type="submit"
            className="w-full rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:ring-offset-slate-900"
          >
            {isLoading ? 'Logging in...' : 'Login'}
          </button>
        </form>
        
      </div>
    </div>
  );
}

export default Login;