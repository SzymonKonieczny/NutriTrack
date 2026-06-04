import { useState, useEffect } from 'react';
import { api } from '../../api/client';
import type { Counts } from '../../dto';

export default function AdminDashboard() {
  const [counts, setCounts] = useState<Counts | null>(null);

  useEffect(() => {
    async function load() {
      const [ing, mic, rec, logs] = await Promise.all([
        api.get<unknown[]>('/ingredients'),
        api.get<unknown[]>('/micronutrients'),
        api.get<unknown[]>('/recipes'),
        api.get<unknown[]>('/meal-logs'),
      ]);
      setCounts({
        ingredients: ing.data?.length ?? 0,
        micronutrients: mic.data?.length ?? 0,
        recipes: rec.data?.length ?? 0,
        mealLogs: logs.data?.length ?? 0,
      });
    }
    load();
  }, []);

  return (
    <div>
      <h1>Admin Dashboard</h1>
      <p className="text-muted text-sm mb-2">
        Manage ingredients, micronutrients, recipes, and their associations.
      </p>

      <div className="dashboard-grid">
        <div className="stat-card">
          <div className="stat-icon">🥕</div>
          <div className="stat-label">Ingredients</div>
          <div className="stat-value">{counts?.ingredients ?? '—'}</div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">🔬</div>
          <div className="stat-label">Micronutrients</div>
          <div className="stat-value">{counts?.micronutrients ?? '—'}</div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">📖</div>
          <div className="stat-label">Recipes</div>
          <div className="stat-value">{counts?.recipes ?? '—'}</div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">🍽️</div>
          <div className="stat-label">Meal Logs</div>
          <div className="stat-value">{counts?.mealLogs ?? '—'}</div>
        </div>
      </div>
    </div>
  );
}