import { useState, useEffect, useCallback } from 'react';
import { api } from '../api/client';
import type { MealLog, CreateMealLogRequest } from '../dto';
import type { Ingredient, Recipe } from '../dto';

type ModalMode = 'create' | 'edit' | null;

function formatDateTime(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  });
}

export default function MealLogsPage() {
  const [logs, setLogs] = useState<MealLog[]>([]);
  const [ingredients, setIngredients] = useState<Ingredient[]>([]);
  const [recipes, setRecipes] = useState<Recipe[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [editingLog, setEditingLog] = useState<MealLog | null>(null);

  // Form state
  const [formDate, setFormDate] = useState(() => new Date().toISOString().slice(0, 16));
  const [formRecipeId, setFormRecipeId] = useState('');
  const [formIngredientId, setFormIngredientId] = useState('');
  const [formAmount, setFormAmount] = useState('');
  const [formServings, setFormServings] = useState('');
  const [formNote, setFormNote] = useState('');
  const [formError, setFormError] = useState<string | null>(null);
  const [formLoading, setFormLoading] = useState(false);
  const [recipeSearch, setRecipeSearch] = useState('');
  const [ingredientSearch, setIngredientSearch] = useState('');

  const fetchLogs = useCallback(async () => {
    setLoading(true);
    setError(null);
    const res = await api.get<MealLog[]>('/meal-logs');
    if (res.data) {
      setLogs(res.data);
    } else {
      setError(res.error);
    }
    setLoading(false);
  }, []);

  const fetchReferenceData = useCallback(async () => {
    const [ingRes, recRes] = await Promise.all([
      api.get<Ingredient[]>('/ingredients'),
      api.get<Recipe[]>('/recipes'),
    ]);
    if (ingRes.data) setIngredients(ingRes.data);
    if (recRes.data) setRecipes(recRes.data);
  }, []);

  useEffect(() => {
    fetchLogs();
    fetchReferenceData();
  }, [fetchLogs, fetchReferenceData]);

  const openCreateModal = () => {
    setEditingLog(null);
    setFormDate(new Date().toISOString().slice(0, 16));
    setFormRecipeId('');
    setFormIngredientId('');
    setFormAmount('');
    setFormServings('');
    setFormNote('');
    setFormError(null);
    setRecipeSearch('');
    setIngredientSearch('');
    setModalMode('create');
  };

  const openEditModal = (log: MealLog) => {
    setEditingLog(log);
    setFormDate(new Date(log.eatenAt).toISOString().slice(0, 16));
    setFormRecipeId(log.recipeId || '');
    setFormIngredientId(log.ingredientId || '');
    setFormAmount(log.amountInGrams?.toString() || '');
    setFormServings(log.servings?.toString() || '');
    setFormNote(log.note || '');
    setFormError(null);
    setModalMode('edit');
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Delete this meal log?')) return;
    const res = await api.delete(`/meal-logs/${id}`);
    if (res.error) {
      setError(res.error);
    } else {
      setLogs((prev) => prev.filter((l) => l.id !== id));
    }
  };

  const buildPayload = (): CreateMealLogRequest => {
    const recipeId = formRecipeId || null;
    const ingredientId = formIngredientId || null;
    return {
      eatenAt: new Date(formDate).toISOString(),
      recipeId,
      ingredientId,
      amountInGrams: formAmount ? parseFloat(formAmount) : null,
      servings: formServings ? parseFloat(formServings) : null,
      note: formNote || null,
    };
  };

  const validate = (payload: CreateMealLogRequest): boolean => {
    if (!payload.recipeId && !payload.ingredientId) {
      setFormError('Select a recipe or ingredient');
      return false;
    }
    setFormError(null);
    return true;
  };

  const handleCreate = async () => {
    const payload = buildPayload();
    if (!validate(payload)) return;

    setFormLoading(true);
    const res = await api.post<MealLog>('/meal-logs', payload);
    setFormLoading(false);

    if (res.data) {
      setLogs((prev) => [res.data!, ...prev]);
      setModalMode(null);
    } else {
      setFormError(res.error);
    }
  };

  const handleUpdate = async () => {
    if (!editingLog) return;
    const payload = buildPayload();
    if (!validate(payload)) return;

    setFormLoading(true);
    const res = await api.put<MealLog>(`/meal-logs/${editingLog.id}`, payload);
    setFormLoading(false);

    if (res.data) {
      setLogs((prev) => prev.map((l) => (l.id === editingLog.id ? res.data! : l)));
      setModalMode(null);
    } else {
      setFormError(res.error);
    }
  };

  const getItemName = (log: MealLog): string => {
    if (log.recipeId) {
      const recipe = recipes.find((r) => r.id === log.recipeId);
      return recipe?.name || 'Recipe';
    }
    if (log.ingredientId) {
      const ingredient = ingredients.find((i) => i.id === log.ingredientId);
      return ingredient?.name || 'Ingredient';
    }
    return 'Unknown';
  };

  const getType = (log: MealLog): string => {
    if (log.recipeId) return '📖 Recipe';
    if (log.ingredientId) return '🥕 Ingredient';
    return '❓';
  };

  // ─── Render ───

  if (loading) {
    return <div className="spinner">Loading meal logs...</div>;
  }

  return (
    <div className="page page-wide">
      <div className="flex items-center justify-between mb-2">
        <div>
          <h1>Meal Logs</h1>
          <p className="text-muted text-sm">Track everything you eat.</p>
        </div>
        <button className="btn btn-primary" onClick={openCreateModal}>
          + Log Meal
        </button>
      </div>

      {error && (
        <div className="alert alert-error mb-2">
          {error}
          <button className="btn btn-ghost btn-sm" onClick={fetchLogs} style={{ marginLeft: 'auto' }}>
            Retry
          </button>
        </div>
      )}

      {!loading && !error && logs.length === 0 && (
        <div className="empty-state">
          <h3>No meals logged yet</h3>
          <p>Start tracking your nutrition by logging your first meal.</p>
          <button className="btn btn-primary mt-1" onClick={openCreateModal}>
            Log Your First Meal
          </button>
        </div>
      )}

      <div className="flex flex-col gap-2 mt-2">
        {logs.map((log) => (
          <div key={log.id} className="meal-log-card">
            <div className="meal-info">
              <div className="meal-name">{getItemName(log)}</div>
              <div className="meal-meta">
                <span>{formatDateTime(log.eatenAt)}</span>
                <span>{getType(log)}</span>
                {log.amountInGrams && <span>{log.amountInGrams}g</span>}
                {log.servings && <span>{log.servings} serving(s)</span>}
                {log.note && <span>— {log.note}</span>}
              </div>
            </div>
            <div className="meal-actions">
              <button className="btn btn-ghost btn-sm" onClick={() => openEditModal(log)}>
                ✏️
              </button>
              <button className="btn btn-ghost btn-sm" onClick={() => handleDelete(log.id)}>
                🗑️
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* ─── Create / Edit Modal ─── */}
      {modalMode && (
        <div className="modal-overlay">
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{modalMode === 'create' ? 'Log Meal' : 'Edit Meal Log'}</h2>
              <button className="btn btn-ghost" onClick={() => setModalMode(null)}>
                ✕
              </button>
            </div>

            <div className="modal-body">
              {formError && <div className="alert alert-error">{formError}</div>}

              <div className="form-group">
                <label htmlFor="meal-date">Date & Time</label>
                <input
                  id="meal-date"
                  type="datetime-local"
                  className="form-input"
                  value={formDate}
                  onChange={(e) => setFormDate(e.target.value)}
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="meal-type">Type</label>
                <div className="flex gap-2">
                  <select
                    className="form-select"
                    value={formRecipeId || formIngredientId ? (formRecipeId ? 'recipe' : 'ingredient') : ''}
                    onChange={(e) => {
                      if (e.target.value === 'recipe') {
                        setFormRecipeId('');
                        setFormIngredientId('');
                      } else if (e.target.value === 'ingredient') {
                        setFormRecipeId('');
                        setFormIngredientId('');
                      }
                    }}
                  >
                    <option value="">Select type</option>
                    <option value="recipe">Recipe</option>
                    <option value="ingredient">Ingredient</option>
                  </select>
                </div>
              </div>

              {(formRecipeId || formIngredientId || true) && (
                <>
                  {recipes.length > 0 && (
                    <div className="form-group">
                      <label htmlFor="meal-recipe">Recipe</label>
                      <input
                        type="text"
                        className="form-input mb-1"
                        placeholder="Search recipes..."
                        value={recipeSearch}
                        onChange={(e) => setRecipeSearch(e.target.value)}
                      />
                      <select
                        id="meal-recipe"
                        className="form-select"
                        value={formRecipeId}
                        onChange={(e) => {
                          setFormRecipeId(e.target.value);
                          if (e.target.value) setFormIngredientId('');
                        }}
                      >
                        <option value="">— None —</option>
                        {recipes
                          .filter((r) => r.name.toLowerCase().includes(recipeSearch.toLowerCase()))
                          .map((r) => (
                          <option key={r.id} value={r.id}>{r.name}</option>
                        ))}
                      </select>
                    </div>
                  )}

                  {ingredients.length > 0 && (
                    <div className="form-group">
                      <label htmlFor="meal-ingredient">Ingredient</label>
                      <input
                        type="text"
                        className="form-input mb-1"
                        placeholder="Search ingredients..."
                        value={ingredientSearch}
                        onChange={(e) => setIngredientSearch(e.target.value)}
                      />
                      <select
                        id="meal-ingredient"
                        className="form-select"
                        value={formIngredientId}
                        onChange={(e) => {
                          setFormIngredientId(e.target.value);
                          if (e.target.value) setFormRecipeId('');
                        }}
                      >
                        <option value="">— None —</option>
                        {ingredients
                          .filter((i) => i.name.toLowerCase().includes(ingredientSearch.toLowerCase()))
                          .map((i) => (
                          <option key={i.id} value={i.id}>{i.name}</option>
                        ))}
                      </select>
                    </div>
                  )}
                </>
              )}

              <div className="flex gap-2">
                <div className="form-group" style={{ flex: 1 }}>
                  <label htmlFor="meal-amount">Amount (g)</label>
                  <input
                    id="meal-amount"
                    type="number"
                    className="form-input"
                    placeholder="e.g. 200"
                    value={formAmount}
                    onChange={(e) => setFormAmount(e.target.value)}
                    min="0"
                    step="0.1"
                  />
                </div>

                <div className="form-group" style={{ flex: 1 }}>
                  <label htmlFor="meal-servings">Servings</label>
                  <input
                    id="meal-servings"
                    type="number"
                    className="form-input"
                    placeholder="e.g. 1"
                    value={formServings}
                    onChange={(e) => setFormServings(e.target.value)}
                    min="0"
                    step="0.25"
                  />
                </div>
              </div>

              <div className="form-group">
                <label htmlFor="meal-note">Note</label>
                <input
                  id="meal-note"
                  type="text"
                  className="form-input"
                  placeholder="Optional note..."
                  value={formNote}
                  onChange={(e) => setFormNote(e.target.value)}
                />
              </div>
            </div>

            <div className="modal-actions">
              <button className="btn btn-secondary" onClick={() => setModalMode(null)}>
                Cancel
              </button>
              <button
                className="btn btn-primary"
                disabled={formLoading}
                onClick={modalMode === 'create' ? handleCreate : handleUpdate}
              >
                {formLoading ? 'Saving...' : modalMode === 'create' ? 'Log Meal' : 'Save Changes'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}