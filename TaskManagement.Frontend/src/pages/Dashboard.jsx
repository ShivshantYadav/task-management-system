import { useEffect, useState } from 'react'
import axiosClient from '../api/axiosClient'
import TaskCard from '../components/TaskCard'
import Icon from '../components/Icons'
import { useAuth } from '../context/AuthContext'

const STAT_CARDS = [
  { key: 'toDoCount', label: 'To Do', icon: 'tasks', tone: 'neutral' },
  { key: 'inProgressCount', label: 'In Progress', icon: 'clock', tone: 'info' },
  { key: 'doneCount', label: 'Done', icon: 'check', tone: 'success' },
  { key: 'overdueCount', label: 'Overdue', icon: 'alert', tone: 'danger' },
]

export default function Dashboard() {
  const { user } = useAuth()
  const [summary, setSummary] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    axiosClient.get('/dashboard/summary')
      .then(({ data }) => setSummary(data))
      .catch((err) => setError(err.response?.data?.message || 'Failed to load dashboard'))
  }, [])

  if (error) return <div className="alert-error"><Icon name="alert" size={16} /><span>{error}</span></div>

  if (!summary) {
    return (
      <div className="page">
        <div className="stats-grid">
          {STAT_CARDS.map((c) => <div key={c.key} className="stat-card skeleton" />)}
        </div>
      </div>
    )
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Dashboard</h1>
          <p className="page-subtitle">Welcome back, {user?.name?.split(' ')[0]}. Here's what's happening.</p>
        </div>
      </div>

      <div className="stats-grid">
        {STAT_CARDS.map((c) => (
          <div key={c.key} className={`stat-card tone-${c.tone}`}>
            <div className="stat-card-icon"><Icon name={c.icon} /></div>
            <div>
              <span className="stat-count">{summary[c.key] ?? 0}</span>
              <span className="stat-label">{c.label}</span>
            </div>
          </div>
        ))}
      </div>

      <section className="section">
        <h2>Task status by user</h2>
        {summary.statusByUser.length === 0 ? (
          <div className="empty-state small">
            <Icon name="inbox" size={22} />
            <p>No assigned tasks yet.</p>
          </div>
        ) : (
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr><th>User</th><th>To Do</th><th>In Progress</th><th>Done</th></tr>
              </thead>
              <tbody>
                {summary.statusByUser.map((item) => (
                  <tr key={item.userId}>
                    <td className="cell-strong">{item.userName}</td>
                    <td>{item.toDoCount}</td>
                    <td>{item.inProgressCount}</td>
                    <td>{item.doneCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="section">
        <h2>Upcoming deadlines</h2>
        {summary.upcomingTasks.length === 0 ? (
          <div className="empty-state small">
            <Icon name="check" size={22} />
            <p>Nothing upcoming. You're all caught up.</p>
          </div>
        ) : (
          <div className="task-grid">
            {summary.upcomingTasks.map((t) => <TaskCard key={t.id} task={t} />)}
          </div>
        )}
      </section>
    </div>
  )
}
