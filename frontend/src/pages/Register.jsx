import { Form, Input, Button, Card, Typography, message, Alert, Radio, theme } from 'antd';
import { UserOutlined, LockOutlined, MailOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { useAuth } from '../context/AuthContext';

const { Title, Text } = Typography;

const Register = () => {
    const navigate = useNavigate();
    const { register } = useAuth();
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const { token } = theme.useToken();

    const onFinish = async (values) => {
        setLoading(true);
        setError(null);
        try {
            await register({
                fullName: values.fullName,
                email: values.email,
                password: values.password,
                role: values.role // Candidate or Recruiter
            });
            message.success('Account created successfully! Please log in.');
            navigate('/login');
        } catch (err) {
            setError(err.response?.data?.message || 'Registration failed. Try a completely different email and a password with numbers and symbols.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', marginTop: '40px' }}>
            <Card variant="borderless" style={{ width: 450, boxShadow: token.boxShadowTertiary, borderRadius: token.borderRadiusLG }}>
                <div style={{ textAlign: 'center', marginBottom: 24 }}>
                    <Title level={3} className="hawaii-title">Create an Account</Title>
                    <Text type="secondary">Join TalentBridge today</Text>
                </div>

                {error && <Alert title="Registration Error" description={error} type="error" showIcon style={{ marginBottom: 24 }} />}

                <Form layout="vertical" onFinish={onFinish} size="large" initialValues={{ role: 'Candidate' }}>
                    <Form.Item label="I am a:" name="role">
                        <Radio.Group optionType="button" buttonStyle="solid" style={{ display: 'flex' }}>
                            <Radio.Button value="Candidate" style={{ flex: 1, textAlign: 'center' }}>Candidate</Radio.Button>
                            <Radio.Button value="Recruiter" style={{ flex: 1, textAlign: 'center' }}>Recruiter</Radio.Button>
                        </Radio.Group>
                    </Form.Item>

                    <Form.Item label="Full Name" name="fullName" rules={[{ required: true, message: 'Please input your Name!' }]}>
                        <Input prefix={<UserOutlined />} placeholder="John Doe" />
                    </Form.Item>

                    <Form.Item label="Email" name="email" rules={[{ required: true, message: 'Please input your Email!' }, { type: 'email', message: 'Enter a valid email' }]}>
                        <Input prefix={<MailOutlined />} placeholder="john@example.com" />
                    </Form.Item>
                    
                    <Form.Item label="Password" name="password" rules={[
                        { required: true, message: 'Please create a Password!' },
                        { min: 8, message: 'Must be at least 8 characters' }
                    ]}>
                        <Input.Password prefix={<LockOutlined />} placeholder="Password" />
                    </Form.Item>

                    <Form.Item>
                        <Button type="primary" htmlType="submit" loading={loading} style={{ width: '100%', marginTop: 8 }}>
                            Register
                        </Button>
                        <div style={{ marginTop: 16, textAlign: 'center' }}>
                            <Text>Already have an account? <a onClick={() => navigate('/login')}>Log In here</a></Text>
                        </div>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
};

export default Register;
