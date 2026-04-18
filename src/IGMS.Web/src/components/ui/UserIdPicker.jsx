import { useState, useEffect, useRef, useId } from 'react'
import { userApi } from '../../services/userApi'

// ─── Chip ─────────────────────────────────────────────────────────────────────

function Chip({ label, onRemove, color = 'blue' }) {
  const colors = {
    blue:   'bg-blue-50   text-blue-800   ring-blue-200',
    red:    'bg-red-50    text-red-800    ring-red-200',
    amber:  'bg-amber-50  text-amber-800  ring-amber-200',
    gray:   'bg-gray-100  text-gray-700   ring-gray-200',
  }
  return (
    <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full
      text-xs font-semibold ring-1 ${colors[color] ?? colors.blue}`}>
      {label}
      <button type="button" onClick={onRemove}
        className="flex items-center justify-center w-3.5 h-3.5 rounded-full
          hover:bg-black/10 transition-colors focus:outline-none"
        aria-label="إزالة">
        <svg width="8" height="8" viewBox="0 0 10 10" fill="currentColor">
          <path d="M6.4 5l2.8-2.8a1 1 0 00-1.4-1.4L5 3.6 2.2
            .8A1 1 0 00.8 2.2L3.6 5 .8 7.8a1 1 0 001.4
            1.4L5 6.4l2.8 2.8a1 1 0 001.4-1.4L6.4 5z"/>
        </svg>
      </button>
    </span>
  )
}

// ─── UserIdPicker ─────────────────────────────────────────────────────────────
/**
 * Dropdown picker for users (multi or single).
 * Fetches users from GET /api/v1/users/lookup on first open.
 *
 * Props:
 *   values   : number[]  – selected IDs (multi mode)
 *   value    : number|'' – selected ID  (single mode)
 *   onChange : (ids: number[]) | (id: number|'') => void
 *   multi    : boolean   – default false (single)
 *   badge    : 'R'|'A'|'C'|'I'
 *   label    : string
 */

const BADGE_COLOR = { R: 'blue', A: 'red', C: 'amber', I: 'gray' }

export default function UserIdPicker({ values, value, onChange, multi = false, badge = 'R', label }) {
  const [allUsers,  setAllUsers]  = useState([])
  const [search,    setSearch]    = useState('')
  const [open,      setOpen]      = useState(false)
  const [loaded,    setLoaded]    = useState(false)
  const inputRef  = useRef(null)
  const wrapRef   = useRef(null)
  const pickerId  = useId()

  const color   = BADGE_COLOR[badge] ?? 'blue'
  const current = multi ? (values ?? []) : (value ? [value] : [])

  // Fetch lookup on first open
  useEffect(() => {
    if (!open || loaded) return
    userApi.getLookup()
      .then((r) => {
        setAllUsers(r.data?.data ?? [])
        setLoaded(true)
      })
      .catch(() => setLoaded(true))
  }, [open])

  // Close on outside click
  useEffect(() => {
    const handler = (e) => {
      if (wrapRef.current && !wrapRef.current.contains(e.target)) setOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  const filtered = allUsers.filter((u) => {
    if (current.includes(u.id)) return false
    if (!search) return true
    const q = search.toLowerCase()
    return u.fullNameAr.includes(search) ||
           u.fullNameEn?.toLowerCase().includes(q) ||
           u.username?.toLowerCase().includes(q)
  })

  const nameOf = (id) => {
    const u = allUsers.find((x) => x.id === id)
    return u ? u.fullNameAr : `#${id}`
  }

  const select = (user) => {
    if (multi) {
      onChange([...current, user.id])
    } else {
      onChange(user.id)
      setOpen(false)
    }
    setSearch('')
    inputRef.current?.focus()
  }

  const remove = (id) => {
    if (multi) onChange(current.filter((x) => x !== id))
    else onChange('')
  }

  const canOpen = multi || current.length === 0

  return (
    <div className="space-y-1.5" ref={wrapRef}>
      {label && (
        <label htmlFor={pickerId} className="block text-xs font-medium text-gray-600">
          {label}
        </label>
      )}

      {/* Selected chips */}
      {current.length > 0 && (
        <div className="flex flex-wrap gap-1.5">
          {current.map((uid) => (
            <Chip key={uid} label={nameOf(uid)} color={color} onRemove={() => remove(uid)} />
          ))}
        </div>
      )}

      {/* Search input */}
      {canOpen && (
        <div className="relative">
          <input
            id={pickerId}
            ref={inputRef}
            type="text"
            value={search}
            onChange={(e) => { setSearch(e.target.value); setOpen(true) }}
            onFocus={() => setOpen(true)}
            placeholder={multi ? 'ابحث عن مستخدم...' : 'اختر مستخدم...'}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm
              focus:outline-none focus:ring-2 focus:ring-green-600 bg-white"
          />

          {/* Dropdown */}
          {open && (
            <div className="absolute z-20 mt-1 w-full bg-white border border-gray-200
              rounded-lg shadow-lg max-h-48 overflow-y-auto">
              {!loaded ? (
                <div className="px-3 py-4 text-sm text-gray-400 text-center">جاري التحميل...</div>
              ) : filtered.length === 0 ? (
                <div className="px-3 py-4 text-sm text-gray-400 text-center">لا توجد نتائج</div>
              ) : (
                filtered.map((u) => (
                  <button
                    key={u.id}
                    type="button"
                    onMouseDown={(e) => e.preventDefault()}
                    onClick={() => select(u)}
                    className="w-full text-start px-3 py-2 text-sm hover:bg-green-50
                      hover:text-green-800 transition-colors border-b border-gray-50 last:border-0"
                  >
                    <span className="font-medium">{u.fullNameAr}</span>
                    {u.username && (
                      <span className="text-gray-400 text-xs ms-2" dir="ltr">@{u.username}</span>
                    )}
                  </button>
                ))
              )}
            </div>
          )}
        </div>
      )}

      {!multi && current.length > 0 && (
        <p className="text-xs text-gray-400">اضغط × لتغيير الاختيار</p>
      )}
    </div>
  )
}
