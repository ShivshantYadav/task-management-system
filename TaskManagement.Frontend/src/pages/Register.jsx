import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import Icon from '../components/Icons'

export default function Register() {
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const { register } = useAuth()
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')

    // Frontend validation
    if (name.trim().length < 2) {
      setError('Name must be at least 2 characters.')
      return
    }

    if (name.trim().length > 100) {
      setError('Name cannot exceed 100 characters.')
      return
    }

    if (email.trim().length > 150) {
      setError('Email cannot exceed 150 characters.')
      return
    }

    if (password.length < 8) {
      setError('Password must be at least 8 characters.')
      return
    }

    if (password.length > 100) {
      setError('Password cannot exceed 100 characters.')
      return
    }

    if (password !== confirmPassword) {
      setError('Password and confirm password do not match.')
      return
    }

    setBusy(true)

    try {
      await register(
        name.trim(),
        email.trim(),
        password,
        confirmPassword
      )

      navigate('/')
    } catch (err) {
      setError(
        err.response?.data?.message ||
        err.response?.data?.title ||
        'Registration failed. Please try again.'
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

        <h1>Bring your team's work into one workspace.</h1>

        <p>
          Create an account to start tracking tasks, teams, and deadlines.
        </p>

        <ul className="auth-panel-list">
          <li>
            <Icon name="check" size={16} />
            Free to get started
          </li>

          <li>
            <Icon name="check" size={16} />
            An admin can manage your role later
          </li>

          <li>
            <Icon name="check" size={16} />
            Works across desktop, tablet, and mobile
          </li>
        </ul>
      </div>

      <div className="auth-form-side">
        <form className="auth-card" onSubmit={handleSubmit}>
          <h2>Create your account</h2>

          <p className="auth-subtitle">
            Get started with TaskFlow in a minute.
          </p>

          {error && (
            <div className="alert-error">
              <Icon name="alert" size={16} />
              <span>{error}</span>
            </div>
          )}

          <label htmlFor="name">Full name</label>

          <input
            id="name"
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Jane Cooper"
            required
            minLength={2}
            maxLength={100}
            autoFocus
          />

          <label htmlFor="reg-email">Email</label>

          <input
            id="reg-email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="you@company.com"
            required
            maxLength={150}
          />

          <label htmlFor="reg-password">Password</label>

          <input
            id="reg-password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="At least 8 characters"
            required
            minLength={8}
            maxLength={100}
          />

          <label htmlFor="reg-confirm-password">Confirm password</label>

          <input
            id="reg-confirm-password"
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder="Re-enter your password"
            required
            minLength={8}
            maxLength={100}
          />

          <button
            type="submit"
            className="btn-primary btn-block"
            disabled={busy}
          >
            {busy ? 'Creating account...' : 'Create account'}
          </button>

          <p className="hint">
            New users get the "User" role by default. Admins can create or
            manage Manager and User accounts.
          </p>

          <p className="auth-footer-text">
            Already have an account? <Link to="/login">Log in</Link>
          </p>
        </form>
      </div>
    </div>
  )
}
