import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import axiosClient from '../api/axiosClient'
import { useAuth } from '../context/AuthContext'
import Icon from '../components/Icons'

export default function Teams() {
  const [teams, setTeams] = useState(null)
  const [error, setError] = useState('')
  const [newTeam, setNewTeam] = useState({ name: '', description: '', managerId: '' })
  const [memberInputs, setMemberInputs] = useState({})
  const [addMode, setAddMode] = useState({})
  const [newMemberInputs, setNewMemberInputs] = useState({})
  const [memberResult, setMemberResult] = useState(null)
  const [users, setUsers] = useState([])
  const [managers, setManagers] = useState([])
  const { user } = useAuth()

  const load = () => {
    axiosClient.get('/teams').then(({ data }) => setTeams(data)).catch((err) => setError(err.response?.data?.message || 'Failed to load teams'))
  }

  useEffect(() => {
    load()
    axiosClient.get('/users', { params: { role: 'User' } }).then(({ data }) => setUsers(data)).catch(() => {})
    if (user.role === 'Admin') {
      axiosClient.get('/users', { params: { role: 'Manager' } }).then(({ data }) => setManagers(data)).catch(() => {})
    }
  }, [])

  const handleCreateTeam = async (e) => {
    e.preventDefault()
    try {
      await axiosClient.post('/teams', { ...newTeam, managerId: Number(newTeam.managerId) })
      setNewTeam({ name: '', description: '', managerId: '' })
      load()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create team')
    }
  }

  const handleAddMember = async (teamId) => {
    const userId = memberInputs[teamId]
    if (!userId) return
    try {
      await axiosClient.post(`/teams/${teamId}/members`, { userId: Number(userId) })
      setMemberInputs({ ...memberInputs, [teamId]: '' })
      load()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to add member')
    }
  }

  const handleCreateMember = async (teamId, e) => {
    e.preventDefault()
    setError('')
    setMemberResult(null)
    const draft = newMemberInputs[teamId] || { name: '', email: '' }
    if (!draft.name || !draft.email) return
    try {
      const { data } = await axiosClient.post(`/teams/${teamId}/members/create`, draft)
      setNewMemberInputs({ ...newMemberInputs, [teamId]: { name: '', email: '' } })
      setMemberResult({ teamId, ...data })
      load()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create employee')
    }
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>{user.role === 'Manager' ? 'My Team' : 'Teams'}</h1>
          <p className="page-subtitle">{teams ? `${teams.length} team${teams.length === 1 ? '' : 's'}` : 'Loading...'}</p>
        </div>
      </div>

      {error && <div className="alert-error"><Icon name="alert" size={16} /><span>{error}</span></div>}

      {user.role === 'Admin' && (
        <div className="detail-card">
          <h3>Create a new team</h3>
          <form className="inline-form" onSubmit={handleCreateTeam}>
            <div className="form-field">
              <label>Team name</label>
              <input placeholder="e.g. Platform Engineering" value={newTeam.name} onChange={(e) => setNewTeam({ ...newTeam, name: e.target.value })} required />
            </div>
            <div className="form-field">
              <label>Description</label>
              <input placeholder="What does this team own?" value={newTeam.description} onChange={(e) => setNewTeam({ ...newTeam, description: e.target.value })} />
            </div>
            <div className="form-field">
              <label>Manager</label>
              <select value={newTeam.managerId} onChange={(e) => setNewTeam({ ...newTeam, managerId: e.target.value })} required>
                <option value="">Select manager</option>
                {managers.map((m) => <option key={m.id} value={m.id}>{m.name} ({m.email})</option>)}
              </select>
            </div>
            <button type="submit" className="btn-primary">
              <Icon name="plus" size={16} /> Create team
            </button>
          </form>
        </div>
      )}

      {teams === null && !error && (
        <div className="teams-grid">
          {[1, 2, 3].map((i) => <div key={i} className="team-card skeleton" />)}
        </div>
      )}

      {teams && teams.length === 0 && (
        <div className="empty-state">
          <Icon name="team" size={28} />
          <h3>No teams yet</h3>
          <p>{user.role === 'Admin' ? 'Create your first team above.' : "You're not part of a team yet."}</p>
        </div>
      )}

      {teams && teams.length > 0 && (
        <div className="teams-grid">
          {teams.map((team) => (
            <div key={team.id} className="team-card">
              <div className="team-card-header">
                <h3>{team.name}</h3>
                <span className="badge badge-neutral">{team.members.length} member{team.members.length === 1 ? '' : 's'}</span>
              </div>
              {team.description && <p className="task-card-desc">{team.description}</p>}
              <p className="team-manager"><Icon name="user" size={14} /> Manager: <strong>{team.managerName}</strong></p>

              <ul className="member-list">
                {team.members.length === 0 && <li className="muted">No members yet</li>}
                {team.members.map((m) => (
                  <li key={m.userId}>
                    <span>{m.name}</span>
                    <span className="muted">{m.email}</span>
                  </li>
                ))}
              </ul>

              {(user.role === 'Admin' || user.id === team.managerId) && (
                <>
                  {memberResult && memberResult.teamId === team.id && (
                    <div className="alert-success" style={{ marginTop: 8 }}>
                      <Icon name="check" size={14} />
                      {memberResult.emailSent ? (
                        <span>Welcome email sent to {memberResult.user.email}.</span>
                      ) : (
                        <span>
                          Created, but email delivery isn't configured. Temporary password:{' '}
                          <code className="temp-password">{memberResult.temporaryPassword}</code>
                        </span>
                      )}
                    </div>
                  )}

                  <div className="add-member-tabs">
                    <button
                      type="button"
                      className={addMode[team.id] !== 'create' ? 'btn-secondary btn-sm' : 'btn-primary btn-sm'}
                      onClick={() => setAddMode({ ...addMode, [team.id]: 'existing' })}
                    >
                      Add existing user
                    </button>
                    <button
                      type="button"
                      className={addMode[team.id] === 'create' ? 'btn-primary btn-sm' : 'btn-secondary btn-sm'}
                      onClick={() => setAddMode({ ...addMode, [team.id]: 'create' })}
                    >
                      Create new employee
                    </button>
                  </div>

                  {addMode[team.id] === 'create' ? (
                    <form className="add-member-row" onSubmit={(e) => handleCreateMember(team.id, e)}>
                      <input
                        placeholder="Full name"
                        value={(newMemberInputs[team.id] || {}).name || ''}
                        onChange={(e) => setNewMemberInputs({ ...newMemberInputs, [team.id]: { ...(newMemberInputs[team.id] || {}), name: e.target.value } })}
                        required
                      />
                      <input
                        type="email"
                        placeholder="Email"
                        value={(newMemberInputs[team.id] || {}).email || ''}
                        onChange={(e) => setNewMemberInputs({ ...newMemberInputs, [team.id]: { ...(newMemberInputs[team.id] || {}), email: e.target.value } })}
                        required
                      />
                      <button type="submit" className="btn-secondary btn-sm">Create &amp; add</button>
                    </form>
                  ) : (
                    <div className="add-member-row">
                      <select
                        value={memberInputs[team.id] || ''}
                        onChange={(e) => setMemberInputs({ ...memberInputs, [team.id]: e.target.value })}
                      >
                        <option value="">Select user to add</option>
                        {users.map((u) => <option key={u.id} value={u.id}>{u.name} ({u.email})</option>)}
                      </select>
                      <button className="btn-secondary btn-sm" onClick={() => handleAddMember(team.id)}>Add</button>
                    </div>
                  )}
                </>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
