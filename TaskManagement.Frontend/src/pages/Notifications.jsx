import { useEffect, useState } from 'react'
import axiosClient from '../api/axiosClient'
import Icon from '../components/Icons'

export default function Notifications() {
  const [items, setItems] = useState(null)
  const [error, setError] = useState('')

  const load = () => {
    axiosClient.get('/notifications').then(({ data }) => setItems(data)).catch((err) => setError(err.response?.data?.message || 'Failed to load notifications'))
  }

  useEffect(load, [])

  const markRead = async (id) => {
    await axiosClient.patch(`/notifications/${id}/read`)
    load()
  }

  const markAllRead = async () => {
    const unread = (items || []).filter((n) => !n.isRead)
    await Promise.all(unread.map((n) => axiosClient.patch(`/notifications/${n.id}/read`)))
    load()
  }

  const unreadCount = items ? items.filter((n) => !n.isRead).length : 0

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Notifications</h1>
          <p className="page-subtitle">{unreadCount > 0 ? `${unreadCount} unread` : 'You are all caught up'}</p>
        </div>
        {unreadCount > 0 && (
          <button className="btn-secondary" onClick={markAllRead}>Mark all as read</button>
        )}
      </div>

      {error && <div className="alert-error"><Icon name="alert" size={16} /><span>{error}</span></div>}

      {items === null && !error && (
        <div className="notifications-list">
          {[1, 2, 3].map((i) => <div key={i} className="notification skeleton" />)}
        </div>
      )}

      {items && items.length === 0 && (
        <div className="empty-state">
          <Icon name="bell" size={28} />
          <h3>No notifications</h3>
          <p>Updates about your tasks will show up here.</p>
        </div>
      )}

      {items && items.length > 0 && (
        <div className="notifications-list">
          {items.map((n) => (
            <div key={n.id} className={n.isRead ? 'notification' : 'notification unread'}>
              {!n.isRead && <span className="unread-dot" aria-hidden="true" />}
              <div className="notification-body">
                <strong>{n.taskTitle}</strong>
                <p>{n.message}</p>
                <span className="comment-date">{new Date(n.createdAt).toLocaleString()}</span>
              </div>
              {!n.isRead && (
                <button className="btn-secondary btn-sm" onClick={() => markRead(n.id)}>Mark read</button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
