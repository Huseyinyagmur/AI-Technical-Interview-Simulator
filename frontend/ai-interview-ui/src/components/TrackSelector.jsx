const icons = { 'csharp-backend': '⌘', 'aspnet-core': '⚡', 'sql-developer': '◫', 'software-fundamentals': '◆', 'computer-vision': '◉', mixed: '✦' };

export default function TrackSelector({ tracks, selected, onSelect }) {
  return <div className="track-grid" role="radiogroup" aria-label="Interview track">
    {tracks.map(track => <button type="button" role="radio" aria-checked={selected === track.id} key={track.id} className={`track-card ${selected === track.id ? 'selected' : ''}`} onClick={() => onSelect(track.id)}>
      <span className="track-icon" aria-hidden="true">{icons[track.id] || '•'}</span>
      <span className="track-copy"><strong>{track.title}</strong><span>{track.description}</span><small>{track.domains.join(' · ')}</small></span>
      <span className="track-check" aria-hidden="true">{selected === track.id ? '✓' : ''}</span>
    </button>)}
  </div>;
}
