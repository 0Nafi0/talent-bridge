import { Typography, Button, Row, Col, Card } from 'antd';
import { SearchOutlined, RocketOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useState, useEffect } from 'react';

const { Title, Paragraph } = Typography;

const Home = () => {
    const navigate = useNavigate();
    const { user } = useAuth();
    const [mounted, setMounted] = useState(false);

    useEffect(() => { setMounted(true); }, []);

    const handleAction = (targetRole) => {
        if (!user) {
            navigate('/register');
            return;
        }
        if (user.role === 'Candidate') navigate('/dashboard/candidate');
        else if (user.role === 'Recruiter') navigate('/dashboard/recruiter');
        else navigate('/');
    };

    return (
        <div style={{ padding: '40px 0', opacity: mounted ? 1 : 0, transform: mounted ? 'translateY(0)' : 'translateY(20px)', transition: 'all 0.6s cubic-bezier(0.2, 0.8, 0.2, 1)' }}>
            <div style={{ textAlign: 'center', marginBottom: 60 }}>
                <Title className="hawaii-title" style={{ fontSize: '4rem', marginBottom: 16 }}>Find Your Dream Job Today</Title>
                <Paragraph style={{ fontSize: '1.2rem', color: '#666', maxWidth: 600, margin: '0 auto 32px' }}>
                    TalentBridge connects world-class candidates with top-tier recruiters. 
                    Set up your profile, browse jobs, and get hired instantly.
                </Paragraph>
                <div style={{ display: 'flex', gap: '16px', justifyCode: 'center', justifyContent: 'center' }}>
                    {(!user || user.role === 'Candidate') && (
                        <Button type="primary" size="large" icon={<SearchOutlined />} onClick={() => handleAction('Candidate')}>
                            I am looking for a job
                        </Button>
                    )}
                    {(!user || user.role === 'Recruiter') && (
                        <Button size="large" icon={<RocketOutlined />} onClick={() => handleAction('Recruiter')}>
                            I want to hire talent
                        </Button>
                    )}
                </div>
            </div>

            <Row gutter={[24, 24]} justify="center">
                <Col xs={24} sm={12} md={8}>
                    <Card title="For Candidates" variant="borderless" style={{ height: '100%', boxShadow: '0 4px 12px rgba(0,0,0,0.05)', transition: 'transform 0.3s ease' }} onMouseEnter={(e) => e.currentTarget.style.transform = 'translateY(-5px)'} onMouseLeave={(e) => e.currentTarget.style.transform = 'translateY(0)'}>
                        Build a striking profile, highlight your top skills, and track all your applications from a unified dashboard.
                    </Card>
                </Col>
                <Col xs={24} sm={12} md={8}>
                    <Card title="For Recruiters" variant="borderless" style={{ height: '100%', boxShadow: '0 4px 12px rgba(0,0,0,0.05)', transition: 'transform 0.3s ease' }} onMouseEnter={(e) => e.currentTarget.style.transform = 'translateY(-5px)'} onMouseLeave={(e) => e.currentTarget.style.transform = 'translateY(0)'}>
                        Post jobs instantly, filter through candidates by match scores, and hire the perfect fit.
                    </Card>
                </Col>
            </Row>
        </div>
    );
};

export default Home;
