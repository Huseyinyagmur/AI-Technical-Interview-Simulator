const topics = ['C#', 'OOP', 'SQL', 'ASP.NET Core Web API'];
export default function TopicSelector({ topic, setTopic, difficulty, setDifficulty }) {
  return <><label>Topic</label><select value={topic} onChange={e => setTopic(e.target.value)}>{topics.map(x => <option key={x}>{x}</option>)}</select><label>Difficulty</label><select value={difficulty} onChange={e => setDifficulty(e.target.value)}>{['Junior', 'Mid-level', 'Senior'].map(x => <option key={x}>{x}</option>)}</select></>;
}
