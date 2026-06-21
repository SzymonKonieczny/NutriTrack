import { useState, useEffect, useCallback } from 'react';
import { api } from '../api/client';
import type { Recipe, RecipeNutrition, CreateMealLogRequest, MealLog } from '../dto';

export default function RecipesPage() {
  const [recipes, setRecipes] = useState<Recipe[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');

  // Nutrition modal state
  const [nutritionModal, setNutritionModal] = useState<{ recipe: Recipe; data: RecipeNutrition | null; loading: boolean; error: string | null } | null>(null);

  // Log meal modal state
  const [logModal, setLogModal] = useState<{ recipe: Recipe; servings: string; note: string; saving: boolean; error: string | null; success: boolean } | null>(null);

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    const res = await api.get<Recipe[]>('/recipes');
    if (res.data) setRecipes(res.data);
    else setError(res.error);
    setLoading(false);
  }, []);

  useEffect(() => { fetch(); }, [fetch]);

  const openNutrition = async (recipe: Recipe) => {
    setNutritionModal({ recipe, data: null, loading: true, error: null });
    const res = await api.get<RecipeNutrition>(`/recipes/${recipe.id}/nutrition`);
    if (res.data) {
      setNutritionModal({ recipe, data: res.data, loading: false, error: null });
    } else {
      setNutritionModal({ recipe, data: null, loading: false, error: res.error });
    }
  };

  const openLogModal = (recipe: Recipe) => {
    setLogModal({ recipe, servings: '1', note: '', saving: false, error: null, success: false });
  };

  const handleLogMeal = async () => {
    if (!logModal) return;
    setLogModal({ ...logModal, saving: true, error: null });
    const payload: CreateMealLogRequest = {
      eatenAt: new Date().toISOString(),
      recipeId: logModal.recipe.id,
      ingredientId: null,
      amountInGrams: null,
      servings: logModal.servings ? parseFloat(logModal.servings) : 1,
      note: logModal.note || null,
    };
    const res = await api.post<MealLog>('/meal-logs', payload);
    if (res.data) {
      setLogModal({ ...logModal, success: true, saving: false });
      setTimeout(() => setLogModal(null), 1200);
    } else {
      setLogModal({ ...logModal, saving: false, error: res.error });
    }
  };

  const filtered = recipes.filter((r) =>
    r.name.toLowerCase().includes(search.toLowerCase())
  );

  if (loading) return <div className="spinner">Loading recipes...</div>;

  return (
    <div className="page page-wide">
      <div className="flex items-center justify-between mb-2">
        <div>
          <h1>Recipes</h1>
          <p className="text-muted text-sm">Browse recipes and log them as meals.</p>
        </div>
        <input
          type="text"
          className="form-input"
          style={{ maxWidth: 280 }}
          placeholder="Search recipes..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {error && (
        <div className="alert alert-error mb-2">
          {error}
          <button className="btn btn-ghost btn-sm" onClick={fetch} style={{ marginLeft: 'auto' }}>
            Retry
          </button>
        </div>
      )}

      {!loading && !error && filtered.length === 0 && (
        <div className="empty-state">
          <h3>{search ? 'No matching recipes' : 'No recipes yet'}</h3>
          <p>{search ? 'Try a different search term.' : 'Ask an admin to create some recipes.'}</p>
        </div>
      )}

      <div className="flex flex-col gap-2 mt-2">
        {filtered.map((recipe) => (
          <div key={recipe.id} className="meal-log-card">
            <div className="meal-info">
              <div className="meal-name">{recipe.name}</div>
              <div className="meal-meta">
                {recipe.prepNote && <span>📝 {recipe.prepNote}</span>}
                {recipe.youTubeUrl && (
                  <span>
                    ▶️ <a href={recipe.youTubeUrl} target="_blank" rel="noopener noreferrer" style={{ color: 'var(--accent)' }}>
                      Watch on YouTube
                    </a>
                  </span>
                )}
              </div>
            </div>
            <div className="meal-actions">
              <button className="btn btn-ghost btn-sm" onClick={() => openNutrition(recipe)} title="View nutrition">
                📊
              </button>
              <button className="btn btn-primary btn-sm" onClick={() => openLogModal(recipe)}>
                + Log Meal
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* ─── Nutrition Modal ─── */}
      {nutritionModal && (
        <div className="modal-overlay">
          <div className="modal modal-lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>📊 {nutritionModal.recipe.name} — Nutrition</h2>
              <button className="btn btn-ghost" onClick={() => setNutritionModal(null)}>✕</button>
            </div>
            <div className="modal-body">
              {nutritionModal.loading && <p className="text-muted">Loading nutrition data...</p>}
              {nutritionModal.error && <div className="alert alert-error">{nutritionModal.error}</div>}
              {nutritionModal.data && nutritionModal.data.micronutrients.length === 0 && (
                <div className="empty-state">
                  <h3>No nutrition data</h3>
                  <p>This recipe has no micronutrient data associated with its ingredients.</p>
                </div>
              )}
              {nutritionModal.data && nutritionModal.data.micronutrients.length > 0 && (
                <div className="table-container">
                  <table>
                    <thead>
                      <tr>
                        <th>Micronutrient</th>
                        <th style={{ width: 160, textAlign: 'right' }}>Amount</th>
                      </tr>
                    </thead>
                    <tbody>
                      {nutritionModal.data.micronutrients.map((m) => (
                        <tr key={m.micronutrientId}>
                          <td><strong>{m.micronutrientName}</strong></td>
                          <td style={{ textAlign: 'right' }}>{m.totalAmount} {m.unit}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
            <div className="modal-actions">
              <button className="btn btn-secondary" onClick={() => setNutritionModal(null)}>Close</button>
            </div>
          </div>
        </div>
      )}

      {/* ─── Log Meal Modal ─── */}
      {logModal && (
        <div className="modal-overlay">
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>+ Log Meal — {logModal.recipe.name}</h2>
              <button className="btn btn-ghost" onClick={() => setLogModal(null)}>✕</button>
            </div>
            <div className="modal-body">
              {logModal.success && (
                <div className="alert alert-success mb-1">✅ Meal logged successfully!</div>
              )}
              {logModal.error && <div className="alert alert-error mb-1">{logModal.error}</div>}

              <div className="form-group">
                <label htmlFor="log-servings">Servings</label>
                <input
                  id="log-servings"
                  type="number"
                  className="form-input"
                  value={logModal.servings}
                  onChange={(e) => setLogModal({ ...logModal, servings: e.target.value })}
                  min="0.25"
                  step="0.25"
                />
              </div>
              <div className="form-group">
                <label htmlFor="log-note">Note (optional)</label>
                <input
                  id="log-note"
                  type="text"
                  className="form-input"
                  placeholder="e.g. Had it for lunch"
                  value={logModal.note}
                  onChange={(e) => setLogModal({ ...logModal, note: e.target.value })}
                />
              </div>
            </div>
            <div className="modal-actions">
              <button className="btn btn-secondary" onClick={() => setLogModal(null)}>Cancel</button>
              <button
                className="btn btn-primary"
                disabled={logModal.saving || logModal.success}
                onClick={handleLogMeal}
              >
                {logModal.saving ? 'Logging...' : logModal.success ? 'Done ✓' : 'Log Meal'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
