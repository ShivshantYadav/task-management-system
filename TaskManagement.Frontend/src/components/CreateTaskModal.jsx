import { useEffect, useState } from 'react'
import axiosClient from '../api/axiosClient'
import Icon from './Icons'

export default function CreateTaskModal({ onClose, onCreated }) {
  const [teams, setTeams] = useState([])
  const [form, setForm] = useState({
    title: '', description: '', priority: 'Medium', dueDate: '', teamId: '', assignedToId: ''
  })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    axiosClient.get('/teams').then(({ data }) => setTeams(data)).catch(() => {})
  }, [])

  const selectedTeam = teams.find((t) => String(t.id) === String(form.teamId))
  const assigneeOptions = selectedTeam
    ? [{ userId: selectedTeam.managerId, name: `${selectedTeam.managerName} (Manager)` }, ...selectedTeam.members]
    : []

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setBusy(true)
    try {
      await axiosClient.post('/tasks', {
        ...form,
        teamId: Number(form.teamId),
        assignedToId: Number(form.assignedToId)
      })
      onCreated()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create task')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="modal-backdrop" onMouseDown={(e) => e.target === e.currentTarget && onClose()}>
      <form className="modal-card" onSubmit={handleSubmit}>
        <div className="modal-header">
          <h2>New task</h2>
          <button type="button" className="icon-btn" onClick={onClose} aria-label="Close">
            <Icon name="close" size={18} />
          </button>
        </div>

        {error && <div className="alert-error"><Icon name="alert" size={16} /><span>{error}</span></div>}

        <div className="form-field">
          <label>Task title</label>
          <input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} placeholder="e.g. Prepare Q3 report" required />
        </div>

        <div className="form-field">
          <label>Description</label>
          <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="Add task details (optional)" />
        </div>

        <div className="form-grid">
          <div className="form-field">
            <label>Priority</label>
            <select value={form.priority} onChange={(e) => setForm({ ...form, priority: e.target.value })}>
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
            </select>
          </div>

          <div className="form-field">
            <label>Due date</label>
            <input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} required />
          </div>
        </div>

        <div className="form-field">
          <label>Team</label>
          <select value={form.teamId} onChange={(e) => setForm({ ...form, teamId: e.target.value, assignedToId: '' })} required>
            <option value="">Select team</option>
            {teams.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
          </select>
        </div>

        <div className="form-field">
          <label>Assign to</label>
          <select value={form.assignedToId} onChange={(e) => setForm({ ...form, assignedToId: e.target.value })} required disabled={!form.teamId}>
            <option value="">{form.teamId ? 'Select assignee' : 'Select a team first'}</option>
            {assigneeOptions.map((m) => <option key={m.userId} value={m.userId}>{m.name}</option>)}
          </select>
        </div>

        <div className="modal-actions">
          <button type="button" onClick={onClose} className="btn-secondary">Cancel</button>
          <button type="submit" className="btn-primary" disabled={busy}>{busy ? 'Creating...' : 'Save task'}</button>
        </div>
      </form>
    </div>
  )
}
