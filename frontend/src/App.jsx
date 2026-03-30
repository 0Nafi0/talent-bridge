import { Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import JobBoard from './pages/JobBoard';
import ProtectedRoute from './components/ProtectedRoute';
import CandidateDashboard from './pages/candidate/CandidateDashboard';
import RecruiterDashboard from './pages/recruiter/RecruiterDashboard';

import { App as AntdApp } from 'antd';

function App() {
  return (
    <AntdApp>
      <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', background: 'transparent' }}>
        <Navbar />
        <div style={{ flex: 1, padding: '24px', maxWidth: '1200px', margin: '0 auto', width: '100%' }}>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/jobs" element={<JobBoard />} />
            
            <Route element={<ProtectedRoute allowedRoles={['Candidate']} />}>
                <Route path="/dashboard/candidate" element={<CandidateDashboard />} />
            </Route>

            <Route element={<ProtectedRoute allowedRoles={['Recruiter', 'Admin']} />}>
                <Route path="/dashboard/recruiter" element={<RecruiterDashboard />} />
            </Route>
          </Routes>
        </div>
      </div>
    </AntdApp>
  );
}

export default App;
