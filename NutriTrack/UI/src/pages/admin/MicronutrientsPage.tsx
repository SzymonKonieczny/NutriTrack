import { useState, useEffect, useCallback } from 'react';
import { api } from '../../api/client';
import type { Micronutrient } from '../../dto';

type ModalMode = 'create' | 'edit' | null;

const UNITS = ['Mg', 'Mcg', 'IU'];

export default function MicronutrientsPage() {
  const [items, setItems] = useState<Micronutrient[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [editing, setEditing] = useState<Micronutrient | null>(null);

  const [formName, setFormName] = useState('');
  const [formUnit, setFormUnit] = useState('Mg');
  const [formDra, setFormDra] = useState('');
  const [formNote, setFormNote] = useState('');
  const [formNonFood, setFormNonFood] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [formLoading, setFormLoading] = useState(false);

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    const res = await api.get<Micronutrient[]>('/micronutrients');
    if (res.data) setItems(res.data);
    else setError(res.error);
    setLoading(false);
  }, []);

  useEffect(() => { fetch(); }, [fetch]);

  const openCreate = () => {
    setEditing(null);
    setFormName(''); setFormUnit('Mg'); setFormDra(''); setFormNote(''); setFormNonFood(false);
    setFormError(null); setModalMode('create');
  };

  const openEdit = (item: Micronutrient) => {
    setEditing(item);
    setFormName(item.name); setFormUnit(item.unit); setFormDra(item.dailyReferenceAmount.toString());
    setFormNote(item.note || ''); setFormNonFood(item.isNonFoodSource); setFormError(null); setModalMode('edit');
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Delete this micronutrient?')) return;
    const res = await api.delete(`/micronutrients/${id}`);
    if (res.error) setError(res.error);
    else setItems((prev) => prev.filter((i) => i.id !== id));
  };

  const handleCreate = async () => {
    if (!formName.trim() || !formDra) { setFormError('Name and Daily Reference Amount are required'); return; }
    setFormLoading(true);
    const res = await api.post<Micronutrient>('/micronutrients', {
      name: formName.trim(),
      dailyReferenceAmount: parseFloat(formDra),
      unit: formUnit,
      note: formNote || null,
      isNonFoodSource: formNonFood,
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
    if (!editing || !formName.trim() || !formDra) { setFormError('Name and Daily Reference Amount are required'); return; }
    setFormLoading(true);
    const res = await api.put<Micronutrient>(`/micronutrients/${editing.id}`, {
      name: formName.trim(),
      dailyReferenceAmount: parseFloat(formDra),
      unit: formUnit,
      note: formNote || null,
      isNonFoodSource: formNonFood,
    });
    setFormLoading(false);
    if (res.data) {
      setItems((prev) => prev.map((i) => (i.id === editing.id ? res.data! : i)));
      setModalMode(null);
    } else {
      setFormError(res.error);
    }
  };

  if (loading) return <div className="spinner">Loading micronutrients...</div>;

  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <div>
          <h1>Micronutrients</h1>
          <p className="text-muted text-sm">Manage vitamins, minerals, and their daily reference amounts.</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>+ New Micronutrient</button>
      </div>

      {error && <div className="alert alert-error mb-2">{error}<button className="btn btn-ghost btn-sm" onClick={fetch} style={{marginLeft:'auto'}}>Retry</button></div>}

      {!loading && !error && items.length === 0 && (
        <div className="empty-state">
          <h3>No micronutrients yet</h3>
          <p>Create your first micronutrient to get started.</p>
          <button className="btn btn-primary mt-1" onClick={openCreate}>Create Micronutrient</button>
        </div>
      )}

      <div className="card">
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Daily Reference</th>
                <th>Unit</th>
                <th>Non-food?</th>
                <th>Note</th>
                <th style={{ width: 100 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td><strong>{item.name}</strong></td>
                  <td>{item.dailyReferenceAmount}</td>
                  <td>{item.unit}</td>
                  <td>{item.isNonFoodSource ? '✅ Yes' : '—'}</td>
                  <td className="text-muted text-sm">{item.note || '—'}</td>
                  <td>
                    <div className="flex gap-1">
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
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{modalMode === 'create' ? 'New Micronutrient' : 'Edit Micronutrient'}</h2>
              <button className="btn btn-ghost" onClick={() => setModalMode(null)}>✕</button>
            </div>
            <div className="modal-body">
              {formError && <div className="alert alert-error">{formError}</div>}
              <div className="form-group">
                <label htmlFor="mic-name">Name</label>
                <input id="mic-name" type="text" className="form-input" value={formName} onChange={(e) => setFormName(e.target.value)} placeholder="e.g. Vitamin C" required />
              </div>
              <div className="flex gap-2">
                <div className="form-group" style={{ flex: 1 }}>
                  <label htmlFor="mic-dra">Daily Reference Amount</label>
                  <input id="mic-dra" type="number" className="form-input" value={formDra} onChange={(e) => setFormDra(e.target.value)} placeholder="e.g. 90" min="0" step="0.01" required />
                </div>
                <div className="form-group" style={{ flex: 1 }}>
                  <label htmlFor="mic-unit">Unit</label>
                  <select id="mic-unit" className="form-select" value={formUnit} onChange={(e) => setFormUnit(e.target.value)}>
                    {UNITS.map((u) => <option key={u} value={u}>{u}</option>)}
                  </select>
                </div>
              </div>
              <div className="form-group">
                <label htmlFor="mic-note">Note</label>
                <input id="mic-note" type="text" className="form-input" value={formNote} onChange={(e) => setFormNote(e.target.value)} placeholder="Optional note" />
              </div>
              <div className="form-group">
                <label className="checkbox-label">
                  <input type="checkbox" checked={formNonFood} onChange={(e) => setFormNonFood(e.target.checked)} />
                  <span>Non-food source (e.g. sunlight, supplements)</span>
                </label>
              </div>
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