import { useAuth } from '../context/AuthContext'
import Icon from '../components/Icons'

function initials(name = '') {
  return name.trim().split(/\s+/).slice(0, 2).map((p) => p[0]?.toUpperCase()).join('') || 'U'
}

export default function Profile() {
  const { user } = useAuth()

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>My Profile</h1>
          <p className="page-subtitle">Your account information</p>
        </div>
      </div>

      <div className="profile-card">
        <div className="profile-avatar">{initials(user.name)}</div>
        <div className="profile-info">
          <h2>{user.name}</h2>
          <span className="badge badge-role">{user.role}</span>

          <dl className="profile-details">
            <div>
              <dt>Email</dt>
              <dd>{user.email}</dd>
            </div>
            <div>
              <dt>Role</dt>
              <dd>{user.role}</dd>
            </div>
            {user.teamName && (
              <div>
                <dt>Team</dt>
                <dd>{user.teamName}</dd>
              </div>
            )}
          </dl>
        </div>
      </div>

      <div className="empty-note">
        <Icon name="alert" size={16} />
        <span>Editing your profile and changing your password will be available once those API endpoints are connected.</span>
      </div>
    </div>
  )
}
