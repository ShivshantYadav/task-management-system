import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import axiosClient from '../api/axiosClient'
import { useAuth } from '../context/AuthContext'
import Icon from '../components/Icons'

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

function initials(name = '') {
  return name.trim().split(/\s+/).slice(0, 2).map((p) => p[0]?.toUpperCase()).join('') || '?'
}

export default function TaskDetail() {
  const { id } = useParams()
  const { user } = useAuth()
  const navigate = useNavigate()
  const [task, setTask] = useState(null)
  const [comments, setComments] = useState([])
  const [newComment, setNewComment] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const load = () => {
    axiosClient.get(`/tasks/${id}`).then(({ data }) => setTask(data)).catch((err) => setError(err.response?.data?.message || 'Failed to load task'))
    axiosClient.get(`/tasks/${id}/comments`).then(({ data }) => setComments(data)).catch(() => {})
  }

  useEffect(load, [id])

  const canChangeStatus = task && (
    user.role === 'Admin' ||
    user.role === 'Manager' ||
    task.assignedToId === user.id
  )

  const handleStatusChange = async (status) => {
    setBusy(true)
    try {
      await axiosClient.patch(`/tasks/${id}/status`, { status })
      load()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update status')
    } finally {
      setBusy(false)
    }
  }

  const handleAddComment = async (e) => {
    e.preventDefault()
    if (!newComment.trim()) return
    try {
      await axiosClient.post(`/tasks/${id}/comments`, { commentText: newComment })
      setNewComment('')
      load()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to add comment')
    }
  }

  if (error && !task) return <div className="alert-error"><Icon name="alert" size={16} /><span>{error}</span></div>
  if (!task) return <div className="page-loading"><span className="spinner" /><span>Loading task...</span></div>

  const status = statusMeta[task.status] || { label: task.status, cls: 'badge-neutral' }
  const priority = priorityMeta[task.priority] || { label: task.priority, cls: 'badge-neutral' }
  const overdue = task.status !== 'Done' && new Date(task.dueDate) < new Date()

  return (
    <div className="page">
      <button className="btn-link" onClick={() => navigate('/tasks')}>
        <Icon name="back" size={16} /> Back to tasks
      </button>

      <div className="task-detail-layout">
        <div className="task-detail-main">
          <div className="detail-card">
            <div className="task-detail-title-row">
              <h1>{task.title}</h1>
              <span className={`badge ${status.cls}`}>{status.label}</span>
            </div>
            <p className="task-desc">{task.description || 'No description provided.'}</p>

            {canChangeStatus && (
              <div className="status-actions">
                <label htmlFor="status-select">Update status</label>
                <select id="status-select" disabled={busy} value={task.status} onChange={(e) => handleStatusChange(e.target.value)}>
                  <option value="ToDo">To Do</option>
                  <option value="InProgress">In Progress</option>
                  <option value="Done">Done</option>
                </select>
              </div>
            )}
          </div>

          <div className="detail-card">
            <h2>Comments</h2>
            <div className="comments-list">
              {comments.length === 0 && (
                <div className="empty-state small">
                  <Icon name="inbox" size={20} />
                  <p>No comments yet. Start the conversation.</p>
                </div>
              )}
              {comments.map((c) => (
                <div key={c.id} className="comment">
                  <span className="avatar avatar-sm">{initials(c.userName)}</span>
                  <div className="comment-body">
                    <div className="comment-head">
                      <strong>{c.userName}</strong>
                      <span className="comment-date">{new Date(c.createdAt).toLocaleString()}</span>
                    </div>
                    <p>{c.commentText}</p>
                  </div>
                </div>
              ))}
            </div>

            <form className="comment-form" onSubmit={handleAddComment}>
              <textarea
                value={newComment}
                onChange={(e) => setNewComment(e.target.value)}
                placeholder="Add a comment..."
              />
              <button type="submit" className="btn-primary">Post comment</button>
            </form>
          </div>
        </div>

        <aside className="task-detail-side">
          <div className="detail-card">
            <h3>Details</h3>
            <dl className="meta-list">
              <div><dt>Priority</dt><dd><span className={`badge ${priority.cls}`}>{priority.label}</span></dd></div>
              <div><dt>Due date</dt><dd className={overdue ? 'overdue' : ''}>{new Date(task.dueDate).toLocaleDateString()}{overdue && ' (overdue)'}</dd></div>
              <div><dt>Team</dt><dd>{task.teamName}</dd></div>
              <div><dt>Assigned to</dt><dd>{task.assignedToName}</dd></div>
              <div><dt>Created by</dt><dd>{task.createdByName}</dd></div>
            </dl>
          </div>
        </aside>
      </div>
    </div>
  )
}
