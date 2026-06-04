import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import type { RecipeProposal } from '../dto';

const MOCK_PROPOSALS: RecipeProposal[] = [
  {
    id: 1,
    title: 'High-Protein Chicken Bowl',
    description: 'Grilled chicken, quinoa, avocado, and mixed greens',
    reason: 'You logged chicken breast 3 times this week — here\'s a complete bowl idea.',
    emoji: '🍗',
  },
  {
    id: 2,
    title: 'Quick Veggie Omelette',
    description: 'Eggs, spinach, bell peppers, and feta cheese',
    reason: 'You\'re low on vitamin A today. This covers 60% of your daily need.',
    emoji: '🍳',
  },
  {
    id: 3,
    title: 'Overnight Oats with Berries',
    description: 'Rolled oats, almond milk, mixed berries, honey drizzle',
    reason: 'Perfect for tomorrow\'s breakfast — prep in 5 minutes tonight.',
    emoji: '🥣',
  },
];

export default function DashboardPage() {
  const { user } = useAuth();

  return (
    <div className="page">
      <div className="flex items-center justify-between mb-2">
        <div>
          <h1>Welcome{user?.name ? `, ${user.name}` : ''}!</h1>
          <p className="text-muted text-sm">
            Track your meals and monitor your nutrition.
          </p>
        </div>
      </div>

      <div className="dashboard-grid">
        <Link to="/meal-logs" style={{ textDecoration: 'none' }}>
          <div className="stat-card">
            <div className="stat-icon">🍽️</div>
            <div className="stat-label">Meal Logs</div>
            <div className="stat-value">Log Meal</div>
            <p className="text-sm text-muted mt-1">
              Record what you ate today
            </p>
          </div>
        </Link>

        {user?.roles.includes('Admin') && (
          <Link to="/admin" style={{ textDecoration: 'none' }}>
            <div className="stat-card">
              <div className="stat-icon">⚙️</div>
              <div className="stat-label">Administration</div>
              <div className="stat-value">Manage Data</div>
              <p className="text-sm text-muted mt-1">
                Ingredients, recipes, micronutrients
              </p>
            </div>
          </Link>
        )}

        <div className="stat-card">
          <div className="stat-icon">📊</div>
          <div className="stat-label">Account</div>
          <div className="stat-value" style={{ fontSize: '0.95rem', fontWeight: 500 }}>
            {user?.email}
          </div>
          <p className="text-sm text-muted mt-1">
            Roles: {user?.roles.join(', ') || 'User'}
          </p>
        </div>
      </div>

      <div className="recipe-proposals">
        <div className="flex items-center justify-between mb-1">
          <h2 className="mb-0">Recipe Suggestions</h2>
          <span className="text-muted text-sm">Those are just suggestions, our data may not be 100% accurate</span>
        </div>
        <p className="text-muted text-sm mb-2">
          Personalized recipe ideas based on your eating patterns.
        </p>
        <div className="proposals-stripe">
          {MOCK_PROPOSALS.map((p) => (
            <div key={p.id} className="proposal-card">
              <div className="proposal-emoji">{p.emoji}</div>
              <div className="proposal-title">{p.title}</div>
              <div className="proposal-desc">{p.description}</div>
              <div className="proposal-reason">{p.reason}</div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}