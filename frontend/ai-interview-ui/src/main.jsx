import React from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';
import './styles.css';
import './home.css';
import './dashboard.css';
import './track-selection.css';
createRoot(document.getElementById('root')).render(<React.StrictMode><App /></React.StrictMode>);
