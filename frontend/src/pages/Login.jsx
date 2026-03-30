import { Form, Input, Button, Card, Typography, message, Alert, theme } from 'antd';
import { UserOutlined, LockOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { useAuth } from '../context/AuthContext';

const { Title, Text } = Typography;

const Login = () => {
    const navigate = useNavigate();
    const { login } = useAuth();
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const { token } = theme.useToken();

    const onFinish = async (values) => {
        setLoading(true);
        setError(null);
        try {
            const data = await login(values.email, values.password);
            message.success(`Welcome back, ${data.fullName}!`);
            
            // Route based on role
            const userRole = data.role || data.userRole;
            if (userRole === 'Candidate') {
                navigate('/dashboard/candidate');
            } else if (userRole === 'Recruiter') {
                navigate('/dashboard/recruiter');
            } else {
                navigate('/');
            }
        } catch (err) {
            setError(err.response?.data?.message || 'Login failed due to unexpected error.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', marginTop: '60px' }}>
            <Card variant="borderless" style={{ width: 400, boxShadow: token.boxShadowTertiary, borderRadius: token.borderRadiusLG }}>
                <div style={{ textAlign: 'center', marginBottom: 24 }}>
                    <Title level={3} className="hawaii-title">Welcome Back</Title>
                    <Text type="secondary">Login to access your TalentBridge dashboard</Text>
                </div>

                {error && <Alert title="Login Failed" description={error} type="error" showIcon style={{ marginBottom: 24 }} />}

                <Form name="normal_login" className="login-form" initialValues={{ remember: true }} onFinish={onFinish} size="large">
                    <Form.Item name="email" rules={[{ required: true, message: 'Please input your Email!' }]}>
                        <Input prefix={<UserOutlined className="site-form-item-icon" />} placeholder="Email Address" type="email" />
                    </Form.Item>
                    
                    <Form.Item name="password" rules={[{ required: true, message: 'Please input your Password!' }]}>
                        <Input.Password prefix={<LockOutlined className="site-form-item-icon" />} type="password" placeholder="Password" />
                    </Form.Item>

                    <Form.Item>
                        <Button type="primary" htmlType="submit" className="login-form-button" loading={loading} style={{ width: '100%' }}>
                            Log in
                        </Button>
                        <div style={{ marginTop: 16, textAlign: 'center' }}>
                            <Text>Don't have an account? <a onClick={() => navigate('/register')}>Register now!</a></Text>
                        </div>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
};

export default Login;
