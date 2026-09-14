import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../services/api';
import Layout from '../components/Layout';
import axios from 'axios';

interface Secret {
  id: string;
  projectId: string;
  key: string;
  decryptedValue?: string;
  createdAt: string;
}

interface ProjectMember {
  userId: string,
  email: string,
  role: string,
}

export default function ProjectSecrets() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [secrets, setSecrets] = useState<Secret[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const [refreshKey, setRefreshKey] = useState(0);

  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [newSecretKey, setNewSecretKey] = useState('');
  const [newSecretValue, setNewSecretValue] = useState('');
  const [isAdding, setIsAdding] = useState(false);
  
  const [revealedValues, setRevealedValues] = useState<Record<string, string>>({});
  const [isRevealing, setIsRevealing] = useState<string | null>(null); // Tracks which button is loading

  const [members, setMembers] = useState<ProjectMember[]>([]);
  const [isMembersModalOpen, setIsMembersModalOpen] = useState(false);
  const [newMemberEmail, setNewMemberEmail] = useState('');
  const [newMemberRole, setNewMemberRole] = useState('Viewer');
  const [isAssigning, setIsAssigning] = useState(false);

  const [userRole, setUserRole] = useState<string>('');


  useEffect(() => {
    const fetchSecrets = async () => {
      setIsLoading(true);
      try {
        const [secretsRes, projectRes] = await Promise.all ([
          api.get(`/projects/${id}/secrets`),
          api.get(`/projects/${id}`)
        ]);

        setSecrets(secretsRes.data.data);
        setUserRole(projectRes.data.data.currentUserRole)
      } catch (err: unknown) {
        if (axios.isAxiosError(err)) {
          setError(err.response?.data?.message || 'Failed to load vault secrets.');
        } else {
          setError('An unexpected error occurred.');
        }
      } finally {
        setIsLoading(false);
      }
    };

    if (id) {
      fetchSecrets();
    }
  }, [id, refreshKey]);

  const handleAddSecret = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsAdding(true);
    setError('');

    try {
        await api.post(`projects/${id}/secrets`, {
            projectId: id,
            key: newSecretKey,
            value: newSecretValue
        });

        setIsAddModalOpen(false);
        setNewSecretKey('');
        setNewSecretValue('');

        setRefreshKey(prev => prev + 1);
    } catch (err: unknown) {
        if (axios.isAxiosError(err)) {
            setError(err.response?.data?.message || "Failed to add secret.");
        } else {
            setError('An expected errors occurred.')
        }
    } finally {
        setIsAdding(false);
    }
  };

  const handleReveal = async (secretId: string) => {
    setIsRevealing(secretId);
    try {
      const response = await api.get(`/projects/${id}/secrets/${secretId}/reveal`);
      
      const decrypted = response.data.data.decryptedValue;
      
      // Update our dictionary of revealed secrets
      setRevealedValues(prev => ({
        ...prev,
        [secretId]: decrypted
      }));
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        alert(err.response?.data?.message || "Failed to decrypt secret. Check your permissions.");
      } else {
        alert("An unexpected error occurred during decryption.");
      }
    } finally {
      setIsRevealing(null);
    }
  };

  const handleDelete = async (secretId: string) => {
    const isConfirmed = window.confirm("Are you sure you want to delete this secret? This action cannot be undone.");
    if(!isConfirmed) return;

    try {
      await api.delete(`/projects/${id}/secrets/${secretId}`);
      
      setRefreshKey(prev => prev + 1);
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        alert(err.response?.data?.message || "Failed to delete secret.");
      } else {
        alert("An unexpected error occurred during deletion.");
      }
    } finally {
      setIsAdding(false);
    }
  }

  const handleCopy = (text: string) => {
    navigator.clipboard.writeText(text);
    alert("Copied to clipboard!"); 
  };

  const fetchMembers = async () => {
    try {
      const response = await api.get(`/projects/${id}/members`);
      setMembers(response.data.data);
      
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        alert(err.response?.data?.message || "Failed to fetch members.");
      } else {
        alert("An unexpected error occurred during fetching.");
      }
    }
  };

  const handleOpenMembers = () => {
    fetchMembers();
    setIsMembersModalOpen(true);
  }

  const handleAssignMember = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsAssigning(true);

    try {
      await api.post(`/projects/${id}/members`, {
        email: newMemberEmail,
        role: newMemberRole
      });

      setNewMemberEmail('');
      setNewMemberRole('Viewer');
      await fetchMembers();
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
          alert(err.response?.data?.message || 'Failed to assign member.');
      }
    } finally {
      setIsAssigning(false);
    }
  };

  const handleRemoveMember = async (memberId: string) => {
    if (!window.confirm("Are you sure you want to remove this user's access?")) return;

    try {
      await api.delete(`/projects/${id}/members/${memberId}`);
      await fetchMembers();
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        alert(err.response?.data?.message || 'Failed to remove member.');
      }
    }
  };

  return (
    <Layout>
      <div className="mb-6 flex items-center space-x-2 text-sm text-slate-400">
        <button onClick={() => navigate('/dashboard')} className="hover:text-slate-200">
          Projects
        </button>
        <span>/</span>
        <span className="text-slate-200 font-medium">Secrets Vault</span>
      </div>

      <div className="flex items-center justify-between mb-8">
        <div>
          <h2 className="text-2xl font-bold text-slate-50">Secrets Management</h2>
          <p className="text-sm text-slate-400 font-mono">Project ID: {id}</p>
        </div>
        <div className="flex items-center justify-end">
          {!isLoading && (
            <>
              {userRole === 'Owner' && (
                <button 
                  onClick={handleOpenMembers}
                  className="rounded-lg border border-slate-700 bg-slate-800 px-4 py-2 text-sm font-semibold text-slate-300 hover:bg-slate-700 transition-colors mr-3"
                >
                    Manage Access
                </button>
              )}

              {userRole !== 'Viewer' && (
                <button
                    onClick={() => setIsAddModalOpen(true)}
                    className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700"
                >
                    + Add Secret
                </button>
              )}
            </>
          )}
        </div>
        
      </div>

      {error && (
        <div className="mb-6 rounded-md bg-rose-500/10 p-4 text-sm text-rose-500 border border-rose-500/20">
          {error}
        </div>
      )}

      {isLoading ? (
        <div className="text-slate-400 text-sm">Loading encrypted vault...</div>
      ) : secrets.length === 0 ? (
        <div className="rounded-xl border border-dashed border-slate-700 p-12 text-center">
          <p className="text-slate-400">This project's vault is currently empty.</p>
        </div>
      ) : (
        <div className="rounded-xl border border-slate-800 bg-slate-900 overflow-hidden">
          <table className="w-full text-left text-sm text-slate-300">
            <thead className="bg-slate-950/50 text-xs uppercase text-slate-400 border-b border-slate-800">
              <tr>
                <th className="px-6 py-4 font-medium">Key Name</th>
                <th className="px-6 py-4 font-medium">Value</th>
                <th className="px-6 py-4 font-medium">Added On</th>
                <th className="px-6 py-4 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/50">
              {secrets.map((secret) => {
                const isRevealed = revealedValues[secret.id] !== undefined;
                const displayValue = isRevealed ? revealedValues[secret.id] : '••••••••••••••••';

                return (
                  <tr key={secret.id} className="hover:bg-slate-800/20 transition-colors">
                    <td className="px-6 py-4 font-mono font-medium text-slate-200">
                      {secret.key}
                    </td>
                    <td className="px-6 py-4 font-mono">
                      {displayValue}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {new Date(secret.createdAt).toLocaleDateString()}
                    </td>
                    <td className="px-6 py-4 text-right space-x-3">
                      {!isRevealed ? (
                        <button
                          onClick={() => handleReveal(secret.id)}
                          disabled={isRevealing === secret.id}
                          className="text-emerald-500 hover:text-emerald-400 font-medium disabled:opacity-50"
                        >
                          {isRevealing === secret.id ? 'Decrypting...' : 'Reveal'}
                        </button>
                      ) : (
                        <button
                          onClick={() => handleCopy(revealedValues[secret.id])}
                          className="text-blue-500 hover:text-blue-400 font-medium"
                        >
                          Copy
                        </button>
                      )}
                      {userRole !== 'Viewer' && (
                          <button
                            onClick={() => handleDelete(secret.id)}
                            className="text-rose-500 hover:text-rose-400 font-medium"
                          >
                            Delete
                          </button>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {isAddModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm">
            <div className="w-full max-w-md rounded-xl border border-slate-800 bg-slate-900 p-6 shadow-2xl">
                <h3 className="text-xl font-bold text-slate-50 mb-4">Add New Secret</h3>
                <form onSubmit={handleAddSecret} className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium text-slate-300 mb-1">Key Name</label>
                        <input
                            type="text"
                            value={newSecretKey}
                            onChange={(e) => setNewSecretKey(e.target.value)}
                            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 font-mono text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                            placeholder="e.g., AWS_ACCESS_KEY"
                            required
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-slate-300 mb-1">Secret Value</label>
                        <input
                            type="password"
                            value={newSecretValue}
                            onChange={(e) => setNewSecretValue(e.target.value)}
                            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 font-mono text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                            placeholder="••••••••••••••••"
                            required
                        />
                    </div>
                    <div className="mt-6 flex justify-end space-x-3">
                        <button
                            type="button"
                            onClick={() => setIsAddModalOpen(false)}
                            className="rounded-lg px-4 py-2 text-sm font-medium text-slate-300 hover:bg-slate-800"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={isAdding}
                            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-50"
                        >
                            {isAdding ? 'Encrypting...' : 'Save Secret'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
      )}

      {/* --- MANAGE ACCESS MODAL --- */}
      {isMembersModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm">
          <div className="w-full max-w-md rounded-xl border border-slate-800 bg-slate-900 p-6 shadow-2xl">
            <div className="flex justify-between items-center mb-6">
                <h3 className="text-xl font-bold text-slate-50">Manage Access</h3>
                <button onClick={() => setIsMembersModalOpen(false)} className="text-slate-400 hover:text-slate-200">
                    ✕
                </button>
            </div>

            {/* Invite Form */}
            <form onSubmit={handleAssignMember} className="mb-8 space-y-4 rounded-lg bg-slate-950 p-4 border border-slate-800">
                <h4 className="text-sm font-semibold text-slate-300">Invite Team Member</h4>
                <div className="flex gap-2">
                    <input
                        type="email"
                        value={newMemberEmail}
                        onChange={(e) => setNewMemberEmail(e.target.value)}
                        className="flex-1 rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-50 text-sm focus:border-blue-500 focus:outline-none"
                        placeholder="developer@example.com"
                        required
                    />
                    <select
                        value={newMemberRole}
                        onChange={(e) => setNewMemberRole(e.target.value)}
                        className="rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-50 text-sm focus:border-blue-500 focus:outline-none"
                    >
                        <option value="Editor">Editor</option>
                        <option value="Viewer">Viewer</option>
                    </select>
                </div>
                <button
                    type="submit"
                    disabled={isAssigning}
                    className="w-full rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-50"
                >
                    {isAssigning ? 'Inviting...' : 'Grant Access'}
                </button>
            </form>

            <h4 className="text-sm font-semibold text-slate-300 mb-3">Current Members</h4>
            <div className="max-h-60 overflow-y-auto space-y-2">
                {members.length === 0 ? (
                    <p className="text-sm text-slate-500 italic">No members assigned yet.</p>
                ) : (
                    members.map(member => (
                        <div key={member.userId} className="flex items-center justify-between rounded-lg border border-slate-800 bg-slate-800/50 p-3">
                            <div>
                                <p className="text-sm font-medium text-slate-200">{member.email}</p>
                            </div>
                            <div className="flex items-center gap-3">
                                <span className="text-xs font-medium text-blue-400 bg-blue-500/10 px-2 py-1 rounded-full border border-blue-500/20">
                                    {member.role}
                                </span>
                                <button 
                                    onClick={() => handleRemoveMember(member.userId)}
                                    className="text-slate-500 hover:text-rose-500 transition-colors"
                                    title="Remove Access"
                                >
                                    ✕
                                </button>
                            </div>
                        </div>
                    ))
                )}
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
}