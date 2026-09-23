
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import Icon from '../components/Icons'

export default function Login() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [remember, setRemember] = useState(true)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const { login } = useAuth()
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')

    // Frontend validation
    if (!email.trim()) {
      setError('Email is required.')
      return
    }

    if (email.trim().length > 150) {
      setError('Email cannot exceed 150 characters.')
      return
    }

    if (!password) {
      setError('Password is required.')
      return
    }

    setBusy(true)

    try {
      await login(email.trim(), password)
      navigate('/')
    } catch (err) {
      setError(
        err.response?.data?.message ||
        err.response?.data?.title ||
        'Invalid email or password.'
      )
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-panel">
        <div className="auth-panel-brand">
          <span className="brand-mark">TF</span>
          <span className="brand-name">TaskFlow</span>
        </div>

        <h1>Plan, assign, and track work in one place.</h1>

        <p>
          A focused task management workspace for admins, managers, and teams.
        </p>

        <ul className="auth-panel-list">
          <li>
            <Icon name="check" size={16} />
            Role-based dashboards for every team
          </li>

          <li>
            <Icon name="check" size={16} />
            Clear task ownership and due dates
          </li>

          <li>
            <Icon name="check" size={16} />
            Comments and activity in one thread
          </li>
        </ul>
      </div>

      <div className="auth-form-side">
        <form className="auth-card" onSubmit={handleSubmit}>
          <h2>Welcome back</h2>

          <p className="auth-subtitle">
            Log in to continue to your dashboard.
          </p>

          {error && (
            <div className="alert-error">
              <Icon name="alert" size={16} />
              <span>{error}</span>
            </div>
          )}

          <label htmlFor="email">Email</label>

          <input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="you@company.com"
            required
            maxLength={150}
            autoFocus
          />

          <label htmlFor="password">Password</label>

          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            required
          />

          <div className="auth-row">
            <label className="checkbox-label">
              <input
                type="checkbox"
                checked={remember}
                onChange={(e) => setRemember(e.target.checked)}
              />
              Remember me
            </label>

            <Link to="/forgot-password" className="link-muted">
              Forgot password?
            </Link>
          </div>

          <button
            type="submit"
            className="btn-primary btn-block"
            disabled={busy}
          >
            {busy ? 'Logging in...' : 'Log in'}
          </button>

          <p className="auth-footer-text">
            Don't have an account? <Link to="/register">Create one</Link>
          </p>
        </form>
      </div>
    </div>
  )
}