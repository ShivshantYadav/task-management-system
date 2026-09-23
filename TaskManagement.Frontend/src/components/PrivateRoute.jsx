import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function PrivateRoute({ roles }) {
  const { user, loading } = useAuth()

  if (loading) {
    return (
      <div className="page-loading">
        <span className="spinner" />
        <span>Loading TaskFlow...</span>
      </div>
    )
  }
  if (!user) return <Navigate to="/login" replace />
  if (roles && !roles.includes(user.role)) return <Navigate to="/" replace />

  return <Outlet />
}
