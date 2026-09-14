import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import Login from './pages/Login'
import Projects from './pages/Projects'
import ProjectSecrets from './pages/ProjectSecrets'
import Users from './pages/Users';
import AuditLogs from './pages/AuditLogs';
import ProtectedRoute from './components/ProtectedRoute';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/login" element={<Login />} />

        <Route
          path="/projects"
          element={
            <ProtectedRoute>
                <Projects />
            </ProtectedRoute>
          }
        />

        <Route
          path="/projects/:id"
          element={
            <ProtectedRoute>
                <ProjectSecrets />
            </ProtectedRoute>
          }
        />

        <Route
          path="/users"
          element={
            <ProtectedRoute allowedRoles={['Administrator']}>
                <Users />
            </ProtectedRoute>
          }
        />

        <Route 
          path="/audit"
          element={
            <ProtectedRoute allowedRoles={['Administrator', 'Auditor']}>
                <AuditLogs />
            </ProtectedRoute>
          }
        />
      </Routes>
    </BrowserRouter>
  )
}

export default App
