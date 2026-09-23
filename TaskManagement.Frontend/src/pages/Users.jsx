import { useEffect, useState } from 'react'
import axiosClient from '../api/axiosClient'
import Icon from '../components/Icons'

const emptyForm = { name: '', email: '', role: 'User', teamId: '' }

export default function Users() {
  const [users, setUsers] = useState(null)
  const [teams, setTeams] = useState([])
  const [roleFilter, setRoleFilter] = useState('')
  const [form, setForm] = useState(emptyForm)
  const [error, setError] = useState('')
  const [result, setResult] = useState(null)
  const [busy, setBusy] = useState(false)

  const loadUsers = (role) => {
    axiosClient
      .get('/users', { params: role ? { role } : {} })
      .then(({ data }) => setUsers(data))
      .catch((err) => setError(err.response?.data?.message || 'Failed to load users'))
  }

  useEffect(() => {
    loadUsers(roleFilter)
  }, [roleFilter])

  useEffect(() => {
    axiosClient.get('/teams').then(({ data }) => setTeams(data)).catch(() => {})
  }, [])

  const handleCreate = async (e) => {
    e.preventDefault()
    setError('')
    setResult(null)
    setBusy(true)
    try {
      const payload = {
        name: form.name,
        email: form.email,
        role: form.role,
        teamId: form.role === 'User' && form.teamId ? Number(form.teamId) : null
      }
      const { data } = await axiosClient.post('/users', payload)
      setResult(data)
      setForm(emptyForm)
      loadUsers(roleFilter)
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create user')
    } finally {
      setBusy(false)
    }
  }

  const copyPassword = () => {
    if (result?.temporaryPassword) {
      navigator.clipboard?.writeText(result.temporaryPassword).catch(() => {})
    }
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Users</h1>
          <p className="page-subtitle">{users ? `${users.length} user${users.length === 1 ? '' : 's'}` : 'Loading...'}</p>
        </div>
      </div>

      {error && <div className="alert-error"><Icon name="alert" size={16} /><span>{error}</span></div>}

      {result && (
        <div className="alert-success">
          <Icon name="check" size={16} />
          {result.emailSent ? (
            <span>Account created for <strong>{result.user.name}</strong>. A welcome email with sign-in credentials was sent to {result.user.email}.</span>
          ) : (
            <span>
              Account created for <strong>{result.user.name}</strong>, but the email could not be delivered (SMTP is not configured).
              Share this temporary password with them securely: <code className="temp-password">{result.temporaryPassword}</code>
              <button type="button" className="btn-secondary btn-sm" onClick={copyPassword} style={{ marginLeft: 8 }}>Copy</button>
            </span>
          )}
        </div>
      )}

      <div className="detail-card">
        <h3>Create a Manager or User account</h3>
        <p className="task-card-desc">
          Admin/Manager-created accounts skip the public registration form. A temporary password is generated and
          emailed to the new account (see the "Email / SMTP" section of the README to enable real delivery).
        </p>
        <form className="inline-form" onSubmit={handleCreate}>
          <div className="form-field">
            <label>Full name</label>
            <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="Jane Cooper" required />
          </div>
          <div className="form-field">
            <label>Email</label>
            <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} placeholder="jane@company.com" required />
          </div>
          <div className="form-field">
            <label>Role</label>
            <select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value, teamId: '' })}>
              <option value="User">User</option>
              <option value="Manager">Manager</option>
            </select>
          </div>
          {form.role === 'User' && (
            <div className="form-field">
              <label>Add to team (optional)</label>
              <select value={form.teamId} onChange={(e) => setForm({ ...form, teamId: e.target.value })}>
                <option value="">No team yet</option>
                {teams.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </div>
          )}
          <button type="submit" className="btn-primary" disabled={busy}>
            <Icon name="plus" size={16} /> {busy ? 'Creating...' : 'Create account'}
          </button>
        </form>
      </div>

      <div className="detail-card">
        <div className="page-header" style={{ marginBottom: 12 }}>
          <h3>User directory</h3>
          <select value={roleFilter} onChange={(e) => setRoleFilter(e.target.value)}>
            <option value="">All roles</option>
            <option value="Admin">Admin</option>
            <option value="Manager">Manager</option>
            <option value="User">User</option>
          </select>
        </div>

        {users === null && !error && <div className="empty-state"><span className="spinner" /></div>}

        {users && users.length === 0 && (
          <div className="empty-state">
            <Icon name="users" size={28} />
            <h3>No users found</h3>
            <p>Try a different role filter.</p>
          </div>
        )}

        {users && users.length > 0 && (
          <ul className="member-list">
            {users.map((u) => (
              <li key={u.id}>
                <span>{u.name}</span>
                <span className="muted">{u.email}</span>
                <span className="badge badge-neutral">{u.role}</span>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  )
}
