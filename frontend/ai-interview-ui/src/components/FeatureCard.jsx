export default function FeatureCard({ icon, title, description }) {
  return <article className="feature-card"><span className="feature-icon">{icon}</span><div><strong>{title}</strong><p>{description}</p></div></article>;
}
