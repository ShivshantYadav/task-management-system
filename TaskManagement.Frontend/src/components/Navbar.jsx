import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import axiosClient from '../api/axiosClient'
import Icon from './Icons'

function initials(name = '') {
  return name.trim().split(/\s+/).slice(0, 2).map((p) => p[0]?.toUpperCase()).join('') || 'U'
}

export default function Navbar({ onToggleSidebar }) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [menuOpen, setMenuOpen] = useState(false)
  const [unread, setUnread] = useState(0)
  const menuRef = useRef(null)

  useEffect(() => {
    if (!user) return
    axiosClient.get('/notifications')
      .then(({ data }) => setUnread(data.filter((n) => !n.isRead).length))
      .catch(() => {})
  }, [user])

  useEffect(() => {
    const onClick = (e) => {
      if (menuRef.current && !menuRef.current.contains(e.target)) setMenuOpen(false)
    }
    document.addEventListener('mousedown', onClick)
    return () => document.removeEventListener('mousedown', onClick)
  }, [])

  if (!user) return null

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <header className="navbar">
      <div className="navbar-left">
        <button className="icon-btn sidebar-toggle" onClick={onToggleSidebar} aria-label="Toggle navigation">
          <Icon name="menu" />
        </button>
        <div className="navbar-search">
          <Icon name="search" size={16} />
          <input type="text" placeholder="Search tasks, teams, people..." aria-label="Search" />
        </div>
      </div>

      <div className="navbar-right">
        <Link to="/notifications" className="icon-btn navbar-bell" aria-label="Notifications">
          <Icon name="bell" />
          {unread > 0 && <span className="badge-dot">{unread > 9 ? '9+' : unread}</span>}
        </Link>

        <div className="navbar-user" ref={menuRef}>
          <button className="navbar-user-btn" onClick={() => setMenuOpen((v) => !v)}>
            <span className="avatar">{initials(user.name)}</span>
            <span className="navbar-user-info">
              <strong>{user.name}</strong>
              <span>{user.role}</span>
            </span>
            <Icon name="chevronDown" size={16} />
          </button>

          {menuOpen && (
            <div className="navbar-dropdown">
              <Link to="/profile" onClick={() => setMenuOpen(false)}>
                <Icon name="user" size={16} /> My Profile
              </Link>
              <button onClick={handleLogout}>
                <Icon name="logout" size={16} /> Logout
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  )
}
