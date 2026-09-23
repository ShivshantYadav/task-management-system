import { Link } from 'react-router-dom'
import Icon from './Icons'

const statusMeta = {
  ToDo: { label: 'To Do', cls: 'badge-neutral' },
  InProgress: { label: 'In Progress', cls: 'badge-info' },
  Done: { label: 'Done', cls: 'badge-success' },
}

const priorityMeta = {
  Low: { label: 'Low', cls: 'badge-success-soft' },
  Medium: { label: 'Medium', cls: 'badge-warning' },
  High: { label: 'High', cls: 'badge-danger' },
}

export default function TaskCard({ task }) {
  const overdue = task.status !== 'Done' && new Date(task.dueDate) < new Date()
  const status = statusMeta[task.status] || { label: task.status, cls: 'badge-neutral' }
  const priority = priorityMeta[task.priority] || { label: task.priority, cls: 'badge-neutral' }

  return (
    <Link to={`/tasks/${task.id}`} className="task-card">
      <div className="task-card-header">
        <h3>{task.title}</h3>
        <span className={`badge ${status.cls}`}>{status.label}</span>
      </div>
      {task.description && <p className="task-card-desc">{task.description}</p>}
      <div className="task-card-meta">
        <span className={`badge ${priority.cls}`}>{priority.label}</span>
        <span className={overdue ? 'meta-pill overdue' : 'meta-pill'}>
          <Icon name="clock" size={14} />
          {new Date(task.dueDate).toLocaleDateString()}
        </span>
      </div>
      <div className="task-card-footer">
        <span><Icon name="user" size={14} /> {task.assignedToName}</span>
        <span><Icon name="team" size={14} /> {task.teamName}</span>
      </div>
    </Link>
  )
}
