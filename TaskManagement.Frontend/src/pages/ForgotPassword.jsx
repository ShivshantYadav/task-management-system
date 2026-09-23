import { useState } from 'react'
import { Link } from 'react-router-dom'
import Icon from '../components/Icons'

export default function ForgotPassword() {
  const [email, setEmail] = useState('')
  const [sent, setSent] = useState(false)

  const handleSubmit = (e) => {
    e.preventDefault()
    setSent(true)
  }

  return (
    <div className="auth-page auth-page-single">
      <div className="auth-form-side">
        <form className="auth-card" onSubmit={handleSubmit}>
          <h2>Reset your password</h2>
          <p className="auth-subtitle">Enter your email and we'll send you a reset link.</p>

          {sent ? (
            <div className="alert-success">
              <Icon name="check" size={16} />
              <span>If an account exists for {email}, a reset link is on its way.</span>
            </div>
          ) : (
            <>
              <label htmlFor="reset-email">Email</label>
              <input
                id="reset-email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@company.com"
                required
                autoFocus
              />
              <button type="submit" className="btn-primary btn-block">Send reset link</button>
            </>
          )}

          <p className="auth-footer-text">
            <Link to="/login">&larr; Back to login</Link>
          </p>
        </form>
      </div>
    </div>
  )
}
