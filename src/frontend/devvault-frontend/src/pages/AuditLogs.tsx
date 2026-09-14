import { useState, useEffect } from 'react';
import api from '../services/api';
import Layout from '../components/Layout';
import axios from 'axios';

interface AuditLog {
    id: string;
    userEmail: string;
    entityType: string;
    entityId: string;
    action: string;
    details: string;
    timestamp: string;
}

export default function AuditLogs() {
    const [logs, setLogs] = useState<AuditLog[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');

    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);

    useEffect(() => {
        const fetchLogs = async () => {
            setIsLoading(true);

            try {
                const response = await api.get(`/audit?page=${currentPage}&limit=20`);

                setLogs(response.data.data.items);
                setTotalPages(response.data.data.totalPages);
                setTotalCount(response.data.data.totalCount);

            } catch (err: unknown) {
                if (axios.isAxiosError(err)) {
                    setError(err.response?.data?.message || 'Failed to load audit logs.');
                } else {
                    setError('An unexpected error occurred.')
                }
            } finally {
                setIsLoading(false);
            }
        }

        fetchLogs();
    }, [currentPage]);

    const getActionBadge = (action: string) => {
        const baseClasses = "px-2.5 py-0.5 rounded-full text-xs font-medium border";
        
        switch (action.toLowerCase()) {
            case 'reveal':
                return `${baseClasses} bg-amber-500/10 text-amber-500 border-amber-500/20`;
            case 'create':
                return `${baseClasses} bg-emerald-500/10 text-emerald-500 border-emerald-500/20`;
            case 'delete':
                return `${baseClasses} bg-rose-500/10 text-rose-500 border-rose-500/20`;
            case 'update':
                return `${baseClasses} bg-blue-500/10 text-blue-500 border-blue-500/20`;
            default:
                return `${baseClasses} bg-slate-500/10 text-slate-400 border-slate-500/20`;        
        }
    };


    return (
        <Layout>
            <div className="mb-8">
                <h2 className="text-2xl font-bold text-slate-50">
                    Audit Logs
                </h2>
                <p className="text-sm text-slate-400 mt-1">
                    Monitor system activity, access records, and security events.
                </p>
            </div>

            {error && (
                <div className='mb-6 rounded-md bg-rose-500/10 p-4 text-sm text-rose-500 border border-rose-500/20'>
                    {error}
                </div>
            )}

            {isLoading ? (
                <div className='text-slate-400 text-sm'>
                    Loading security logs...
                </div>

            ): logs.length === 0 ? (
                <div className='rounded-xl border border-dashed border-slate-700 p-12 text-center'>
                    <p className='text-slate-400'>
                        No activity logged yet.
                    </p>
                </div>
            ): (
                <div className='rounded-xl border border-slate-800 bg-slate-900 overflow-hidden'>
                    <div className='overflow-x-auto'>
                        <table className='w-full text-left text-sm text-slate-300'>
                            <thead className='bg-slate-950/50 text-xs uppercase text-slate-400 border-b border-slate-800'>
                                <tr>
                                    <th className="px-6 py-4 font-medium">Timestamp</th>
                                    <th className="px-6 py-4 font-medium">User</th>
                                    <th className="px-6 py-4 font-medium">Action</th>
                                    <th className="px-6 py-4 font-medium">Entity</th>
                                    <th className="px-6 py-4 font-medium">Details</th>
                                </tr>
                            </thead>

                            <tbody className='divide-y divide-slate-800/50'>
                                {logs.map((log) => (
                                    <tr key={log.id} className='hover:bg-slate-800/20 transition-colors'>
                                        <td className="px-6 py-4 whitespace-nowrap text-slate-400">
                                            {new Date(log.timestamp).toLocaleString()}
                                        </td>
                                        <td className="px-6 py-4 font-medium text-slate-200">
                                            {log.userEmail}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <span className={getActionBadge(log.action)}>
                                                {log.action.toUpperCase()}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <span className="text-slate-400 font-mono text-xs">
                                                {log.entityType}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4">
                                            {log.details}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </div>
            )}
        </Layout>
    );
}
