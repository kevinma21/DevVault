import { useState, useEffect } from 'react';
import api from '../services/api';
import Layout from '../components/Layout';
import axios from 'axios';

interface User {
  id: string;
  firstName: string;
  lastName?: string;
  email: string;
  isActive: boolean;
  roles: string[];
}

export default function Users() {
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [refreshKey, setRefreshKey] = useState(0);

  // --- ADD USER STATES ---
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [newFirstName, setNewFirstName] = useState('');
  const [newLastName, setNewLastName] = useState('');
  const [newEmail, setNewEmail] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [newRole, setNewRole] = useState('Developer');
  const [isAdding, setIsAdding] = useState(false);

  // --- EDIT USER STATES ---
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [editingUserId, setEditingUserId] = useState('');
  const [editFirstName, setEditFirstName] = useState('');
  const [editLastName, setEditLastName] = useState('');
  const [editRole, setEditRole] = useState('');
  const [editIsActive, setEditIsActive] = useState(true);
  const [isEditing, setIsEditing] = useState(false);

  // --- FETCH USERS ---
  useEffect(() => {
    const fetchUsers = async () => {
      setIsLoading(true);
      try {
        const response = await api.get('/users'); 
        setUsers(response.data.data.items || []);
      } catch (err: unknown) {
        if (axios.isAxiosError(err)) {
          setError(err.response?.data?.message || 'Failed to load users.');
        } else {
          setError('An unexpected error occurred.');
        }
      } finally {
        setIsLoading(false);
      }
    };

    fetchUsers();
  }, [refreshKey]);

  // --- ADD USER HANDLER ---
  const handleAddUser = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsAdding(true);
    setError('');

    try {
        await api.post('/users', {
            firstName: newFirstName,
            lastName: newLastName,
            email: newEmail,
            password: newPassword,
            roleName: newRole
        });

        setIsAddModalOpen(false);
        setNewFirstName('');
        setNewLastName('');
        setNewEmail('');
        setNewPassword('');
        setNewRole('Developer');

        setRefreshKey(prev => prev + 1);
    } catch (err: unknown) {
        if(axios.isAxiosError(err)) {
            setError(err.response?.data?.message || 'Failed to add user. Check validation');
        } else {
            setError('An unexpected error occurred.');
        }
    } finally {
        setIsAdding(false);
    }
  }

  // --- EDIT USER HANDLER ---
  const handleEditUser = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsEditing(true);
    setError('');

    try {
      // 1. Update the Profile & Active Status
      await api.put(`/users/${editingUserId}`, {
        firstName: editFirstName,
        lastName: editLastName,
        isActive: editIsActive
      });

      // 2. Update the Role
      await api.post(`/users/${editingUserId}/roles`, {
        role: editRole
      });

      setIsEditModalOpen(false);
      setRefreshKey(prev => prev + 1);
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || 'Failed to update user.');
      } else {
        setError('An unexpected error occurred.');
      }
    } finally {
      setIsEditing(false);
    } 
  }

  return (
    <Layout>
      <div className="flex items-center justify-between mb-8">
        <div>
          <h2 className="text-2xl font-bold text-slate-50">User Management</h2>
          <p className="text-sm text-slate-400">Manage team access and roles.</p>
        </div>
        <button
            onClick={() => setIsAddModalOpen(true)}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 transition-colors"
        >
          + Add User
        </button>
      </div>

      {error && (
        <div className="mb-6 rounded-md bg-rose-500/10 p-4 text-sm text-rose-500 border border-rose-500/20">
          {error}
        </div>
      )}

      {isLoading ? (
        <div className="text-slate-400 text-sm">Loading users...</div>
      ) : (
        <div className="rounded-xl border border-slate-800 bg-slate-900 overflow-hidden">
          <table className="w-full text-left text-sm text-slate-300">
            <thead className="bg-slate-950/50 text-xs uppercase text-slate-400 border-b border-slate-800">
              <tr>
                <th className="px-6 py-4 font-medium">Name</th>
                <th className="px-6 py-4 font-medium">Email</th>
                <th className="px-6 py-4 font-medium">Role</th>
                <th className="px-6 py-4 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/50">
              {users.map((user) => (
                <tr 
                  key={user.id} 
                  className={`hover:bg-slate-800/20 transition-colors ${!user.isActive ? 'opacity-50 bg-slate-900/40' : ''}`}
                >
                  <td className="px-6 py-4 font-medium text-slate-200 flex items-center gap-2">
                    {user.firstName} {user.lastName || ''}
                    
                    {!user.isActive && (
                      <span className="inline-flex items-center rounded-full bg-rose-500/10 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wider text-rose-400 border border-rose-500/20">
                        Inactive
                      </span>
                    )}
                  </td>
                  <td className="px-6 py-4 text-slate-400">
                    {user.email}
                  </td>
                  <td className="px-6 py-4 space-x-2">
                    {user.roles.map(role => (
                        <span key={role} className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium border ${user.isActive ? 'bg-blue-500/10 text-blue-400 border-blue-500/20' : 'bg-slate-700/50 text-slate-400 border-slate-600/50'}`}>
                            {role}
                        </span>
                    ))}
                  </td>
                  <td className="px-6 py-4 text-right space-x-3">
                    <button 
                      className="text-blue-500 hover:text-blue-400 font-medium transition-colors"
                      onClick={() => {
                        setEditingUserId(user.id);
                        setEditFirstName(user.firstName);
                        setEditLastName(user.lastName || '');
                        setEditRole(user.roles[0] || 'Developer');
                        setEditIsActive(user.isActive); 

                        setIsEditModalOpen(true);
                      }}
                    >
                      Edit User
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* --- ADD USER MODAL --- */}
      {isAddModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm">
            <div className="w-full max-w-md rounded-xl font-bold border border-slate-800 bg-slate-900 p-6 shadow-2xl">
                <h3 className="text-xl font-bold text-slate-50 mb-4">
                    Invite New User
                </h3>

                <form onSubmit={handleAddUser} className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium text-slate-300 mb-1">
                                First Name
                            </label>
                            <input 
                                type="text"
                                value={newFirstName}
                                onChange={(e) => setNewFirstName(e.target.value)}
                                className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                                placeholder="first name"
                                required
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-slate-300 mb-1">
                                Last Name
                            </label>
                            <input 
                                type="text"
                                value={newLastName}
                                onChange={(e) => setNewLastName(e.target.value)}
                                className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                                placeholder="last name"
                            />
                        </div>
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-slate-300 mb-1">Email Address</label>
                        <input
                            type="email"
                            value={newEmail}
                            onChange={(e) => setNewEmail(e.target.value)}
                            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                            placeholder="email@example.com"
                            required
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-slate-300 mb-1">Initial Password</label>
                        <input 
                            type="password"
                            value={newPassword}
                            onChange={(e) => setNewPassword(e.target.value)}
                            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                            placeholder="••••••••"
                            required
                            minLength={8}
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-slate-300 mb-1">Assign Role</label>
                        <select
                            value={newRole}
                            onChange={(e) => setNewRole(e.target.value)}
                            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                        >
                            <option value="Developer">Developer</option>
                            <option value="Auditor">Auditor</option>
                            <option value="Administrator">Administrator</option>
                        </select>
                    </div>
                    <div className="mt-6 flex justify-end space-x-3">
                        <button
                            type="button"
                            onClick={() => setIsAddModalOpen(false)}
                            className="rounded-lg px-4 py-2 text-sm font-medium text-slate-300 hover:bg-slate-800 transition-colors"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={isAdding}
                            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-50 transition-colors"
                        >
                            {isAdding ? 'Inviting...' : 'Send Invite'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
      )}

      {/* --- EDIT USER MODAL --- */}
      {isEditModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm">
          <div className="w-full max-w-md rounded-xl font-bold border border-slate-800 bg-slate-900 p-6 shadow-2xl">
            <h3 className="text-xl font-bold text-slate-50 mb-4">
              Edit User Profile 
            </h3>

            <form onSubmit={handleEditUser} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-slate-300 mb-1">First Name</label>
                  <input 
                    type="text"
                    value={editFirstName}
                    onChange={(e) => setEditFirstName(e.target.value)}
                    className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-300 mb-1">Last Name</label>
                  <input 
                    type="text"
                    value={editLastName}
                    onChange={(e) => setEditLastName(e.target.value)}
                    className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-300 mb-1">Assign Role</label>
                <select
                  value={editRole}
                  onChange={(e) => setEditRole(e.target.value)}
                  className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="Developer">Developer</option>
                  <option value="Auditor">Auditor</option>
                  <option value="Administrator">Administrator</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-300 mb-1">Account Status</label>
                <select
                  value={editIsActive ? "true" : "false"}
                  onChange={(e) => setEditIsActive(e.target.value === "true")}
                  className={`w-full rounded-lg border px-3 py-2 text-sm focus:outline-none focus:ring-1 ${
                    editIsActive 
                      ? 'border-blue-500/50 bg-blue-500/10 text-blue-400 focus:border-blue-500 focus:ring-blue-500' 
                      : 'border-rose-500/50 bg-rose-500/10 text-rose-400 focus:border-rose-500 focus:ring-rose-500'
                  }`}
                >
                  <option value="true">Active (Has Access)</option>
                  <option value="false">Inactive (Suspended)</option>
                </select>
              </div>

              <div className="mt-6 flex justify-end space-x-3">
                <button
                  type="button"
                  onClick={() => setIsEditModalOpen(false)}
                  className="rounded-lg px-4 py-2 text-sm font-medium text-slate-300 hover:bg-slate-800 transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isEditing}
                  className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-50 transition-colors"
                >
                  {isEditing ? 'Saving...' : 'Save Changes'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </Layout>
  );
}