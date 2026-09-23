import { NavLink } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import Icon from './Icons'

export default function Sidebar({ collapsed, mobileOpen, onNavigate }) {
  const { user } = useAuth()
  if (!user) return null

  const tasksLabel = user.role === 'Admin' ? 'All Tasks' : user.role === 'Manager' ? 'Team Tasks' : 'My Tasks'

  const items = [
    { to: '/', icon: 'dashboard', label: 'Dashboard', end: true },
    { to: '/tasks', icon: 'tasks', label: tasksLabel },
    ...(user.role === 'Admin' ? [{ to: '/users', icon: 'users', label: 'Users' }] : []),
    ...(user.role === 'Admin' || user.role === 'Manager'
      ? [{ to: '/teams', icon: 'team', label: user.role === 'Manager' ? 'My Team' : 'Teams' }]
      : []),
    { to: '/notifications', icon: 'bell', label: 'Notifications' },
    { to: '/profile', icon: 'user', label: 'Profile' },
  ]

  return (
    <aside className={`sidebar${collapsed ? ' collapsed' : ''}${mobileOpen ? ' mobile-open' : ''}`}>
      <div className="sidebar-brand">
        <span className="brand-mark">TF</span>
        {!collapsed && <span className="brand-name">TaskFlow</span>}
      </div>

      <nav className="sidebar-nav">
        {items.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            onClick={onNavigate}
            className={({ isActive }) => `sidebar-link${isActive ? ' active' : ''}`}
            title={collapsed ? item.label : undefined}
          >
            <Icon name={item.icon} />
            {!collapsed && <span>{item.label}</span>}
          </NavLink>
        ))}
      </nav>

      {!collapsed && (
        <div className="sidebar-footer">
          <div className="sidebar-footer-card">
            <span className="footer-role-badge">{user.role}</span>
            <p>Signed in as</p>
            <strong>{user.name}</strong>
          </div>
        </div>
      )}
    </aside>
  )
}
