import { useEffect, useState } from 'react'
import axiosClient from '../api/axiosClient'
import TaskCard from '../components/TaskCard'
import { useAuth } from '../context/AuthContext'
import CreateTaskModal from '../components/CreateTaskModal'
import Icon from '../components/Icons'

const EMPTY_FILTERS = { status: '', priority: '', dueAfter: '', dueBefore: '' }

export default function Tasks() {
  const [tasks, setTasks] = useState(null)
  const [filters, setFilters] = useState(EMPTY_FILTERS)
  const [error, setError] = useState('')
  const [showCreate, setShowCreate] = useState(false)
  const { user } = useAuth()

  const canCreate = user?.role === 'Admin' || user?.role === 'Manager'
  const title = user?.role === 'Admin' ? 'All Tasks' : user?.role === 'Manager' ? 'Team Tasks' : 'My Tasks'
  const filtersActive = Object.values(filters).some(Boolean)

  const load = () => {
    const params = {}
    if (filters.status) params.status = filters.status
    if (filters.priority) params.priority = filters.priority
    if (filters.dueAfter) params.dueAfter = filters.dueAfter
    if (filters.dueBefore) params.dueBefore = filters.dueBefore

    axiosClient.get('/tasks', { params })
      .then(({ data }) => setTasks(data))
      .catch((err) => setError(err.response?.data?.message || 'Failed to load tasks'))
  }

  useEffect(load, [filters])

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>{title}</h1>
          <p className="page-subtitle">{tasks ? `${tasks.length} task${tasks.length === 1 ? '' : 's'}` : 'Loading...'}</p>
        </div>
        {canCreate && (
          <button className="btn-primary" onClick={() => setShowCreate(true)}>
            <Icon name="plus" size={16} /> New Task
          </button>
        )}
      </div>

      <div className="filters">
        <select value={filters.status} onChange={(e) => setFilters({ ...filters, status: e.target.value })}>
          <option value="">All statuses</option>
          <option value="ToDo">To Do</option>
          <option value="InProgress">In Progress</option>
          <option value="Done">Done</option>
        </select>
        <select value={filters.priority} onChange={(e) => setFilters({ ...filters, priority: e.target.value })}>
          <option value="">All priorities</option>
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
        </select>
        <label className="filter-date">
          Due after
          <input type="date" value={filters.dueAfter} onChange={(e) => setFilters({ ...filters, dueAfter: e.target.value })} />
        </label>
        <label className="filter-date">
          Due before
          <input type="date" value={filters.dueBefore} onChange={(e) => setFilters({ ...filters, dueBefore: e.target.value })} />
        </label>
        {filtersActive && (
          <button type="button" className="btn-secondary btn-sm" onClick={() => setFilters(EMPTY_FILTERS)}>
            <Icon name="filterOff" size={15} /> Clear filters
          </button>
        )}
      </div>

      {error && <div className="alert-error"><Icon name="alert" size={16} /><span>{error}</span></div>}

      {tasks === null && !error && (
        <div className="task-grid">
          {[1, 2, 3].map((i) => <div key={i} className="task-card skeleton" />)}
        </div>
      )}

      {tasks !== null && tasks.length === 0 && !error && (
        <div className="empty-state">
          <Icon name="inbox" size={28} />
          <h3>No tasks found</h3>
          <p>{filtersActive ? 'Try adjusting or clearing your filters.' : 'Tasks assigned to you will show up here.'}</p>
        </div>
      )}

      {tasks && tasks.length > 0 && (
        <div className="task-grid">
          {tasks.map((t) => <TaskCard key={t.id} task={t} />)}
        </div>
      )}

      {showCreate && (
        <CreateTaskModal
          onClose={() => setShowCreate(false)}
          onCreated={() => { setShowCreate(false); load() }}
        />
      )}
    </div>
  )
}
