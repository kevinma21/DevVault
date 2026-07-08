import { useState, useEffect } from 'react';
import api from '../services/api';
import Layout from '../components/Layout';
import axios from 'axios';

// Define the shape of your Project data based on your C# backend
interface Project {
  id: string;
  name: string;
  description: string;
}

export default function Dashboard() {
    const [projects, setProjects] = useState<Project[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');
    const [refreshKey, setRefreshKey] = useState(0); // Used to trigger re-fetching of projects

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [newProjectName, setNewProjectName] = useState('');
    const [newProjectDesc, setNewProjectDesc] = useState('');
    const [isCreating, setIsCreating] = useState(false);

    useEffect(() => {
        // Fetch projects on load
        const fetchProjects = async () => {
            setIsLoading(true);
            try {
                const response = await api.get('/projects');
                setProjects(response.data.data);
            } catch (err: unknown) {
                if (axios.isAxiosError(err)) {
                    setError(err.response?.data?.message || 'Failed to load projects.');
                } else {
                    setError('An unexpected error occurred.');
                }
            } finally {
                setIsLoading(false);
            }
        }

        fetchProjects();
    }, [refreshKey]);

    const handleCreateProject = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsCreating(true);
        setError('');

        try {
            await api.post('/projects', {
                name: newProjectName,
                description: newProjectDesc
            });
            
            setIsModalOpen(false);
            setNewProjectName('');
            setNewProjectDesc('');

            // Refresh the list of projects after creating a new one
            setRefreshKey(prev => prev + 1);
        } catch (err: unknown) {
            if (axios.isAxiosError(err)) {
                setError(err.response?.data?.message || 'Failed to create project.');
            }
        } finally {
            setIsCreating(false);
        }
    }

  return (
    <Layout>
      <div className="flex items-center justify-between mb-8">
        <div>
            <h2 className="text-2xl font-bold text-slate-50">Projects</h2>
            <p className="text-sm text-slate-400">Select a project to manage its secrets.</p>
        </div>
        <button 
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700"
            onClick={() => setIsModalOpen(true)}
        >
          + New Project
        </button>
      </div>

      {error && (
        <div className="mb-6 rounded-md bg-rose-500/10 p-4 text-sm text-rose-500 border border-rose-500/20">
            {error}
        </div>
      )}

      {isLoading ? (
        <div className="text-slate-400 text-sm">Loading projects...</div>
      ) : projects.length === 0 ? (
        <div className="rounded-xl border border-dashed border-slate-700 p-12 text-center">
            <p className="text-slate-400">No projects found. Create one to get started.</p>
        </div>
      ) : (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {projects.map((project) => (
            <div 
              key={project.id} 
              className="rounded-xl border border-slate-800 bg-slate-900 p-6 transition-colors hover:border-slate-700 cursor-pointer"
            >
                <h3 className="text-lg font-semibold text-slate-50">{project.name}</h3>
                <p className="mt-2 text-sm text-slate-400 line-clamp-2">
                    {project.description || 'No description provided.'}
                </p>
                <div className="mt-6 flex items-center justify-between">
                    <span className="inline-flex items-center rounded-full bg-slate-800 px-2.5 py-0.5 text-xs font-medium text-slate-300">
                        Active
                    </span>
                    <button className="text-sm font-medium text-blue-500 hover:text-blue-400">
                        View Secrets →
                    </button>
                </div>
            </div>
          ))}
        </div>
      )}

        {/* Modal for creating a new project */}
        {isModalOpen && (
            <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm">
                <div className="w-full max-w-md rounded-xl border border-slate-800 bg-slate-900 p-6 shadow-2xl">
                    <h3 className="text-xl font-bold text-slate-50 mb-4">Create New Project</h3>
                    <form onSubmit={handleCreateProject} className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium text-slate-300 mb-1">Project Name</label>
                            <input
                                type="text"
                                value={newProjectName}
                                onChange={(e) => setNewProjectName(e.target.value)}
                                className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                                placeholder="e.g., Payment Gateway"
                                required
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-slate-300 mb-1">Description (Optional)</label>
                            <textarea
                                value={newProjectDesc}
                                onChange={(e) => setNewProjectDesc(e.target.value)}
                                className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-50 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                                placeholder="What is this project for?"
                                rows={3}
                            />
                        </div>
                        <div className="mt-6 flex justify-end space-x-3">
                            <button
                                type="button"
                                onClick={() => setIsModalOpen(false)}
                                className="rounded-lg px-4 py-2 text-sm font-medium text-slate-300 hover:bg-slate-800"
                            >
                                Cancel
                            </button>
                            <button
                                type="submit"
                                disabled={isCreating}
                                className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-50"
                            >
                                {isCreating ? 'Creating...' : 'Create Project'}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        )}
    </Layout>
  );
}