import { motion } from 'framer-motion';

export default function InterviewHero() {
  return <section className="track-hero"><div><p className="eyebrow">AI Technical Interview Simulator</p><h1>Gerçek kariyer yoluna göre pratik yap.</h1><p>AI destekli teknik mülakatlarla eksiklerini keşfet, gelişim raporları al ve kariyerine hazırlan.</p></div><motion.div className="hero-preview" initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: .45 }}><div className="preview-top"><span>Canlı performans</span><b>82<span>/100</span></b></div><div className="preview-line"><i style={{ width: '82%' }} /></div><div className="preview-stats"><span>5 soru</span><span>3 güçlü konu</span></div></motion.div></section>;
}
