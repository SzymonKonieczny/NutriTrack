import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import { useState, useEffect, useCallback } from 'react';
import type { DeficiencyAnalysis } from '../dto';

function suggestionEmoji(recipeName: string): string {
  const lower = recipeName.toLowerCase();
  if (lower.includes('chicken') || lower.includes('turkey') || lower.includes('meat')) return '🍗';
  if (lower.includes('salad') || lower.includes('greens') || lower.includes('spinach') || lower.includes('veggie')) return '🥗';
  if (lower.includes('egg') || lower.includes('omelette') || lower.includes('frittata')) return '🍳';
  if (lower.includes('oat') || lower.includes('berry') || lower.includes('granola')) return '🥣';
  if (lower.includes('fish') || lower.includes('salmon') || lower.includes('tuna')) return '🐟';
  if (lower.includes('soup') || lower.includes('stew')) return '🍲';
  if (lower.includes('pasta') || lower.includes('noodle') || lower.includes('spaghetti')) return '🍝';
  if (lower.includes('rice') || lower.includes('burrito') || lower.includes('taco') || lower.includes('bowl')) return '🌮';
  if (lower.includes('shake') || lower.includes('smoothie')) return '🥤';
  return '🍽️';
}

export default function DashboardPage() {
  const { user } = useAuth();
  const [analysis, setAnalysis] = useState<DeficiencyAnalysis | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchAnalysis = useCallback(async () => {
    setLoading(true);
    setError(null);
    const res = await api.get<DeficiencyAnalysis>('/RecipieProposals/deficiency');
    if (res.data) setAnalysis(res.data);
    else setError(res.error);
    setLoading(false);
  }, []);

  useEffect(() => { fetchAnalysis(); }, [fetchAnalysis]);

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

        {loading && (
          <div className="flex items-center justify-center" style={{ padding: '2rem' }}>
            <div className="spinner">Analyzing your meal history...</div>
          </div>
        )}

        {error && (
          <div>
            <div className="alert alert-error mb-1">
              {error === 'Request failed (404)' ? 'Log some meals first to get recipe suggestions.' : error}
              <button className="btn btn-ghost btn-sm" onClick={fetchAnalysis} style={{ marginLeft: 'auto' }}>Retry</button>
            </div>
          </div>
        )}

        {!loading && !error && (!analysis || analysis.suggestedRecipes.length === 0) && (
          <div className="empty-state">
            <h3>No suggestions yet</h3>
            <p>Log some meals to get personalized recipe recommendations based on your nutritional needs.</p>
            <Link to="/meal-logs" className="btn btn-primary mt-1">Log a Meal</Link>
          </div>
        )}

        {!loading && !error && analysis && analysis.suggestedRecipes.length > 0 && (
          <>
            {analysis.topDeficits.length > 0 && (
              <div className="deficit-summary" style={{ marginBottom: '1rem' }}>
                <h3 className="text-sm mb-1">Your top micronutrient gaps</h3>
                <div className="flex gap-1" style={{ flexWrap: 'wrap' }}>
                  {analysis.topDeficits.map((d) => (
                    <span
                      key={d.micronutrientId}
                      className="badge badge-warning"
                      title={`${d.micronutrientName}: ${d.averageDailyConsumed.toFixed(1)} / ${d.recommendedDailyAmount} ${d.unit} daily (${d.deficitPercentage.toFixed(0)}% deficient)`}
                    >
                      {d.micronutrientName} — {d.deficitPercentage.toFixed(0)}% low
                    </span>
                  ))}
                </div>
              </div>
            )}
            <div className="proposals-stripe">
              {analysis.suggestedRecipes.map((r) => (
                <div key={r.recipeId} className="proposal-card">
                  <div className="proposal-emoji">{suggestionEmoji(r.recipeName)}</div>
                  <div className="proposal-title">{r.recipeName}</div>
                  {r.prepNote && <div className="proposal-desc">{r.prepNote}</div>}
                  <div className="proposal-reason">
                    Rich in:{' '}
                    {r.coveredMicronutrients.map((m, i) => (
                      <span key={m.micronutrientId}>
                        {i > 0 && ', '}
                        {m.micronutrientName} ({m.totalAmount} {m.unit})
                      </span>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  );
}