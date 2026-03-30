import { useState, useEffect } from 'react';
import { Card, Typography, Descriptions, Tag, Button, Spin, Alert, Row, Col, theme, Modal, Form, Input, InputNumber, Select, message, List, Space, Divider } from 'antd';
import { EditOutlined, BuildOutlined, PlusOutlined, DeleteOutlined, FileSearchOutlined, EnvironmentOutlined, ClockCircleOutlined } from '@ant-design/icons';
import api from '../../api/axios';
import { useAuth } from '../../context/AuthContext';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;

const CandidateDashboard = () => {
    const { user } = useAuth();
    const [profile, setProfile] = useState(null);
    const [applications, setApplications] = useState([]);
    const [allSkills, setAllSkills] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const { token } = theme.useToken();

    // Modals
    const [isProfileModalOpen, setIsProfileModalOpen] = useState(false);
    const [isSkillModalOpen, setIsSkillModalOpen] = useState(false);
    const [form] = Form.useForm();
    const [skillForm] = Form.useForm();

    const fetchProfile = async () => {
        try {
            const res = await api.get('/profile/candidate');
            setProfile(res.data);
        } catch (err) {
            console.error(err);
            setError('Failed to load candidate profile.');
        }
    };

    const fetchApplications = async () => {
        try {
            const res = await api.get('/applications/my');
            setApplications(res.data);
        } catch (err) {
            console.error(err);
        }
    };

    const fetchSkills = async () => {
        try {
            const res = await api.get('/skills');
            setAllSkills(res.data);
        } catch (err) {
            console.error(err);
        }
    };

    useEffect(() => {
        const loadAll = async () => {
            setLoading(true);
            await Promise.all([fetchProfile(), fetchApplications(), fetchSkills()]);
            setLoading(false);
        };
        loadAll();
    }, []);

    // Fix: form.setFieldsValue warning. 
    // Synchronize form values when profile is fetched or when modal opens
    useEffect(() => {
        if (isProfileModalOpen && profile) {
            form.setFieldsValue(profile);
        }
    }, [isProfileModalOpen, profile, form]);

    const handleUpdateProfile = async (values) => {
        try {
            await api.put('/profile/candidate', values);
            message.success('Profile updated successfully!');
            fetchProfile();
            setIsProfileModalOpen(false);
        } catch (err) {
            message.error('Failed to update profile.');
        }
    };

    const handleAddSkill = async (values) => {
        try {
            await api.post('/profile/candidate/skills', values);
            message.success('Skill added!');
            fetchProfile();
            setIsSkillModalOpen(false);
            skillForm.resetFields();
        } catch (err) {
            message.error(err.response?.data?.message || 'Failed to add skill.');
        }
    };

    const handleRemoveSkill = async (skillId) => {
        try {
            await api.delete(`/profile/candidate/skills/${skillId}`);
            message.success('Skill removed.');
            fetchProfile();
        } catch (err) {
            message.error('Failed to remove skill.');
        }
    };

    const getStatusColor = (status) => {
        switch (status) {
            case 'Applied': return 'blue';
            case 'Shortlisted': return 'cyan';
            case 'Interview': return 'orange';
            case 'Rejected': return 'error';
            case 'Offer': return 'success';
            default: return 'default';
        }
    };

    if (loading) return <div style={{ display: 'flex', justifyContent: 'center', marginTop: '100px' }}><Spin size="large" /></div>;
    
    if (error) return <Alert type="error" message={error} style={{ marginTop: '20px' }} />;

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
                <Title level={2} style={{ margin: 0 }} className="hawaii-title">My Dashboard</Title>
                <Button type="primary" icon={<EditOutlined />} onClick={() => setIsProfileModalOpen(true)}>Edit Profile</Button>
            </div>

            <Row gutter={[24, 24]}>
                <Col xs={24} lg={16}>
                    {/* Profile Information */}
                    <Card variant="borderless" style={{ boxShadow: token.boxShadowTertiary, borderRadius: token.borderRadiusLG, marginBottom: 24 }}>
                        <Title level={4} style={{ marginBottom: 16 }}>Profile Information</Title>
                        <Descriptions column={2} bordered size="middle" layout="vertical">
                            <Descriptions.Item label="Full Name"><Text strong>{profile?.fullName || user.fullName}</Text></Descriptions.Item>
                            <Descriptions.Item label="Location"><Text italic>{profile?.location || 'Not set'}</Text></Descriptions.Item>
                            <Descriptions.Item label="Headline" span={2}>{profile?.headline || 'Your professional headline'}</Descriptions.Item>
                            <Descriptions.Item label="Experience">{profile?.yearsOfExperience ? `${profile.yearsOfExperience} Years` : 'Unknown'}</Descriptions.Item>
                        </Descriptions>
                        
                        <div style={{ marginTop: 24 }}>
                            <Title level={5}>Professional Bio</Title>
                            <Paragraph type="secondary">
                                {profile?.bio || 'Add a bio to let recruiters know about your background and goals.'}
                            </Paragraph>
                        </div>
                    </Card>

                    {/* Applications List */}
                    <Card title="My Job Applications" variant="borderless" style={{ boxShadow: token.boxShadowTertiary, borderRadius: token.borderRadiusLG }}>
                        <List
                            itemLayout="horizontal"
                            dataSource={applications}
                            renderItem={(item) => (
                                <List.Item>
                                    <List.Item.Meta
                                        avatar={<FileSearchOutlined style={{ fontSize: 24, color: token.colorPrimary }} />}
                                        title={<Text strong style={{ fontSize: 16 }}>{item.jobTitle}</Text>}
                                        description={
                                            <Space separator={<Divider orientation="vertical" />}>
                                                <Text type="secondary">{item.companyName}</Text>
                                                <Text type="secondary"><EnvironmentOutlined /> {item.location}</Text>
                                                <Text type="secondary"><ClockCircleOutlined /> Applied {new Date(item.appliedAt).toLocaleDateString()}</Text>
                                            </Space>
                                        }
                                    />
                                    <div>
                                        <Tag color={getStatusColor(item.status)}>{item.status.toUpperCase()}</Tag>
                                        <div style={{ textAlign: 'right', marginTop: '4px' }}>
                                            <Text type="secondary" size="small">Match: {item.matchScore}%</Text>
                                        </div>
                                    </div>
                                </List.Item>
                            )}
                        />
                    </Card>
                </Col>

                <Col xs={24} lg={8}>
                    {/* Skills Card */}
                    <Card 
                        title={<Title level={4} style={{ margin: 0 }}><BuildOutlined style={{ marginRight: 8, color: token.colorPrimary }} /> Professional Skills</Title>}
                        variant="borderless" 
                        style={{ boxShadow: token.boxShadowTertiary, borderRadius: token.borderRadiusLG }}
                        extra={<Button type="link" icon={<PlusOutlined />} onClick={() => setIsSkillModalOpen(true)}>Add</Button>}
                    >
                        <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap' }}>
                            {profile?.skills && profile.skills.length > 0 ? (
                                profile.skills.map(item => (
                                    <Tag 
                                        color={token.colorPrimary} 
                                        style={{ padding: '4px 12px', fontSize: '14px', borderRadius: '16px', display: 'flex', alignItems: 'center' }} 
                                        key={item.skill.id}
                                        closable
                                        onClose={(e) => { e.preventDefault(); handleRemoveSkill(item.skill.id); }}
                                    >
                                        {item.skill.name} • {item.yearsOfExperience}y
                                    </Tag>
                                ))
                            ) : (
                                <Text type="secondary">No skills added yet.</Text>
                            )}
                        </div>
                    </Card>
                </Col>
            </Row>

            {/* Profile Edit Modal */}
            <Modal
                title="Edit Your Profile"
                open={isProfileModalOpen}
                onCancel={() => setIsProfileModalOpen(false)}
                footer={null}
            >
                <Form form={form} layout="vertical" onFinish={handleUpdateProfile}>
                    <Form.Item name="headline" label="Headline">
                        <Input placeholder="e.g. Full Stack Developer with 5 years experience" />
                    </Form.Item>
                    <Form.Item name="bio" label="Bio">
                        <Input.TextArea rows={4} placeholder="Tell us about yourself..." />
                    </Form.Item>
                    <Form.Item name="location" label="Location">
                        <Input placeholder="City, Country" />
                    </Form.Item>
                    <Form.Item name="yearsOfExperience" label="Years of Experience">
                        <InputNumber min={0} style={{ width: '100%' }} />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit" block>Save Profile</Button>
                    </Form.Item>
                </Form>
            </Modal>

            {/* Add Skill Modal */}
            <Modal
                title="Add New Skill"
                open={isSkillModalOpen}
                onCancel={() => setIsSkillModalOpen(false)}
                footer={null}
            >
                <Form form={skillForm} layout="vertical" onFinish={handleAddSkill}>
                    <Form.Item name="skillId" label="Skill" rules={[{ required: true }]}>
                        <Select placeholder="Select a skill" showSearch optionFilterProp="children">
                            {allSkills.map(s => (
                                <Option key={s.id} value={s.id}>{s.name} ({s.category})</Option>
                            ))}
                        </Select>
                    </Form.Item>
                    <Form.Item name="yearsOfExperience" label="Years of Experience" rules={[{ required: true }]}>
                        <InputNumber min={1} style={{ width: '100%' }} />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit" block>Add to Profile</Button>
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default CandidateDashboard;
