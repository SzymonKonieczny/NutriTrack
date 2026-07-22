import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { api } from '../api/client';
import type { Ingredient } from '../dto';
import type { MicronutrientRef as Micronutrient } from '../dto';

type ModalMode = 'create' | 'edit' | null;

const VISIBILITY_LABELS: Record<string, string> = {
  Private: '🔒 Private',
  Unlisted: '🔎 Unlisted',
  Public: '🌍 Public',
  Rejected: '❌ Rejected',
};

const VISIBILITY_OPTIONS = [
  { value: 'Private', label: '🔒 Private — only visible to you' },
  { value: 'Unlisted', label: '🔎 Unlisted — request admin review for publication' },
];

export default function MyIngredientsPage() {
  const { user } = useAuth();
  const [items, setItems] = useState<Ingredient[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [editing, setEditing] = useState<Ingredient | null>(null);

  const [formName, setFormName] = useState('');
  const [formNote, setFormNote] = useState('');
  const [formVisibility, setFormVisibility] = useState('Unlisted');
  const [formError, setFormError] = useState<string | null>(null);
  const [formLoading, setFormLoading] = useState(false);

  // Micronutrient multi-select state (create only)
  const [micronutrients, setMicronutrients] = useState<Micronutrient[]>([]);
  const [selectedMicros, setSelectedMicros] = useState<Record<string, string>>({});
  const [microSearch, setMicroSearch] = useState('');

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    const res = await api.get<Ingredient[]>('/ingredients');
    if (res.data) {
      // Filter to show only the current user's ingredients
      const myItems = res.data.filter((i) => i.authorId === user?.id);
      setItems(myItems);
    } else setError(res.error);
    setLoading(false);
  }, [user?.id]);

  useEffect(() => { fetch(); }, [fetch]);

  const openCreate = async () => {
    setEditing(null);
    setFormName('');
    setFormNote('');
    setFormVisibility('Unlisted');
    setFormError(null);
    setSelectedMicros({});
    setMicroSearch('');
    const res = await api.get<Micronutrient[]>('/micronutrients');
    if (res.data) setMicronutrients(res.data);
    else setMicronutrients([]);
    setModalMode('create');
  };

  const openEdit = (item: Ingredient) => {
    setEditing(item);
    setFormName(item.name);
    setFormNote(item.note || '');
    setFormVisibility(item.visibility || 'Private');
    setFormError(null);
    setSelectedMicros({});
    setModalMode('edit');
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Delete this ingredient?')) return;
    const res = await api.delete(`/ingredients/${id}`);
    if (res.error) setError(res.error);
    else setItems((prev) => prev.filter((i) => i.id !== id));
  };

  const handleCreate = async () => {
    if (!formName.trim()) { setFormError('Name is required'); return; }
    setFormLoading(true);

    const micronutrientsPayload = Object.entries(selectedMicros)
      .filter(([, amount]) => amount && parseFloat(amount) > 0)
      .map(([id, amount]) => ({
        micronutrientId: id,
        amountPer100g: parseFloat(amount),
      }));

    const res = await api.post<Ingredient>('/ingredients', {
      name: formName.trim(),
      note: formNote || null,
      visibility: formVisibility,
      micronutrients: micronutrientsPayload.length > 0 ? micronutrientsPayload : undefined,
    });
    setFormLoading(false);
    if (res.data) {
      setItems((prev) => [...prev, res.data!]);
      setModalMode(null);
    } else {
      setFormError(res.error);
    }
  };

  const handleUpdate = async () => {
    if (!editing || !formName.trim()) { setFormError('Name is required'); return; }
    setFormLoading(true);
    const res = await api.put<Ingredient>(`/ingredients/${editing.id}`, {
      name: formName.trim(),
      note: formNote || null,
      visibility: formVisibility,
    });
    setFormLoading(false);
    if (res.data) {
      setItems((prev) => prev.map((i) => (i.id === editing.id ? res.data! : i)));
      setModalMode(null);
    } else {
      setFormError(res.error);
    }
  };

  const handleMicroAmountChange = (id: string, value: string) => {
    setSelectedMicros((prev) => ({ ...prev, [id]: value }));
  };

  if (loading) return <div className="spinner">Loading your ingredients...</div>;

  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <div>
          <h1>My Ingredients</h1>
          <p className="text-muted text-sm">Create and manage your own ingredients.</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>+ New Ingredient</button>
      </div>

      {error && <div className="alert alert-error mb-2">{error}<button className="btn btn-ghost btn-sm" onClick={fetch} style={{marginLeft:'auto'}}>Retry</button></div>}

      {!loading && !error && items.length === 0 && (
        <div className="empty-state">
          <h3>No ingredients yet</h3>
          <p>Create your own ingredients with custom micronutrient data.</p>
          <button className="btn btn-primary mt-1" onClick={openCreate}>Create Ingredient</button>
        </div>
      )}

      <div className="card">
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Note</th>
                <th>Visibility</th>
                <th style={{ width: 160 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td><strong>{item.name}</strong></td>
                  <td className="text-muted text-sm">{item.note || '—'}</td>
                  <td><span className="badge">{VISIBILITY_LABELS[item.visibility ?? 'Private'] || item.visibility}</span></td>
                  <td>
                    <div className="flex gap-1">
                      <Link to={`/my-ingredients/${item.id}/micronutrients`} className="btn btn-ghost btn-sm" title="Manage micronutrients">🔗</Link>
                      <button className="btn btn-ghost btn-sm" onClick={() => openEdit(item)}>✏️</button>
                      <button className="btn btn-ghost btn-sm" onClick={() => handleDelete(item.id)}>🗑️</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {modalMode && (
        <div className="modal-overlay">
          <div className="modal modal-lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{modalMode === 'create' ? 'New Ingredient' : 'Edit Ingredient'}</h2>
              <button className="btn btn-ghost" onClick={() => setModalMode(null)}>✕</button>
            </div>
            <div className="modal-body">
              {formError && <div className="alert alert-error">{formError}</div>}
              <div className="form-group">
                <label htmlFor="my-ing-name">Name</label>
                <input id="my-ing-name" type="text" className="form-input" value={formName} onChange={(e) => setFormName(e.target.value)} placeholder="e.g. Chicken Breast" required />
              </div>
              <div className="form-group">
                <label htmlFor="my-ing-note">Note</label>
                <input id="my-ing-note" type="text" className="form-input" value={formNote} onChange={(e) => setFormNote(e.target.value)} placeholder="Optional note" />
              </div>
              <div className="form-group">
                <label htmlFor="my-ing-visibility">Visibility</label>
                <select id="my-ing-visibility" className="form-select" value={formVisibility} onChange={(e) => setFormVisibility(e.target.value)}>
                  {VISIBILITY_OPTIONS.map((opt) => (
                    <option key={opt.value} value={opt.value}>{opt.label}</option>
                  ))}
                </select>
                <p className="text-muted text-sm mt-1">
                  {formVisibility === 'Private'
                    ? 'Only you and admins can see this ingredient.'
                    : 'Admins will review your ingredient for potential publication.'}
                </p>
              </div>

              {modalMode === 'create' && micronutrients.length > 0 && (
                <div className="form-group">
                  <label>Micronutrients (per 100g)</label>
                  <p className="text-muted text-sm mb-1">Optionally set micronutrient values for this ingredient.</p>
                  <input
                    type="text"
                    className="form-input mb-1"
                    placeholder="Search micronutrients..."
                    value={microSearch}
                    onChange={(e) => setMicroSearch(e.target.value)}
                  />
                  <div className="association-table">
                    <table>
                      <thead>
                        <tr>
                          <th>Micronutrient</th>
                          <th style={{ width: 160 }}>Amount per 100g</th>
                        </tr>
                      </thead>
                      <tbody>
                        {micronutrients
                          .filter((m) => m.name.toLowerCase().includes(microSearch.toLowerCase()))
                          .map((m) => (
                          <tr key={m.id}>
                            <td><strong>{m.name}</strong> <span className="text-muted text-sm">({m.unit})</span></td>
                            <td>
                              <input
                                type="number"
                                className="form-input"
                                placeholder={m.unit}
                                min="0"
                                step="0.01"
                                value={selectedMicros[m.id] ?? ''}
                                onChange={(e) => handleMicroAmountChange(m.id, e.target.value)}
                              />
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}
            </div>
            <div className="modal-actions">
              <button className="btn btn-secondary" onClick={() => setModalMode(null)}>Cancel</button>
              <button className="btn btn-primary" disabled={formLoading} onClick={modalMode === 'create' ? handleCreate : handleUpdate}>
                {formLoading ? 'Saving...' : modalMode === 'create' ? 'Create' : 'Save'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}