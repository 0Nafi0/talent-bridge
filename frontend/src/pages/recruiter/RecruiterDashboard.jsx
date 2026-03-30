import { useState, useEffect } from 'react';
import { Card, Typography, Row, Col, Button, Table, Tag, Space, Modal, Form, Input, InputNumber, Select, message, Spin, Tabs, Badge, Avatar, Tooltip, Progress, Divider } from 'antd';
import { PlusOutlined, UserOutlined, AppstoreOutlined, TeamOutlined, EditOutlined, EyeOutlined, RocketOutlined, BarChartOutlined, TrophyOutlined } from '@ant-design/icons';
import api from '../../api/axios';
import { useAuth } from '../../context/AuthContext';
import { theme } from 'antd';

const { Title, Text } = Typography;
const { Option } = Select;

const RecruiterDashboard = () => {
    const { user } = useAuth();
    const [profile, setProfile] = useState(null);
    const [jobs, setJobs] = useState([]);
    const [applicants, setApplicants] = useState([]);
    const [allSkills, setAllSkills] = useState([]);
    const [analytics, setAnalytics] = useState(null);
    const [loading, setLoading] = useState(true);
    const { token } = theme.useToken();
    
    // Modals
    const [isJobModalOpen, setIsJobModalOpen] = useState(false);
    const [isAppStatusModalOpen, setIsAppStatusModalOpen] = useState(false);
    const [isProfileModalOpen, setIsProfileModalOpen] = useState(false);
    const [selectedApp, setSelectedApp] = useState(null);
    const [jobForm] = Form.useForm();
    const [statusForm] = Form.useForm();
    const [profileForm] = Form.useForm();

    const fetchDashboardData = async () => {
        setLoading(true);
        try {
            const [profRes, jobsRes, appsRes, skillsRes, analyticsRes] = await Promise.all([
                api.get('/dashboard/profile'),
                api.get('/dashboard/jobs'),
                api.get('/dashboard/applicants'),
                api.get('/skills'),
                api.get('/analytics/recruiter')
            ]);
            setProfile(profRes.data);
            setJobs(jobsRes.data);
            setApplicants(appsRes.data);
            setAllSkills(skillsRes.data);
            setAnalytics(analyticsRes.data);
        } catch (err) {
            console.error(err);
            message.error('Failed to load dashboard data.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchDashboardData();
    }, []);

    // Sync profile form values when profile is fetched or modal opens
    useEffect(() => {
        if (isProfileModalOpen && profile) {
            profileForm.setFieldsValue(profile);
        }
    }, [isProfileModalOpen, profile, profileForm]);

    const handleUpdateProfile = async (values) => {
        try {
            await api.put('/dashboard/profile', values);
            message.success('Company profile updated!');
            setIsProfileModalOpen(false);
            fetchDashboardData();
        } catch (err) {
            message.error('Failed to update company profile.');
        }
    };

    const handleCreateJob = async (values) => {
        try {
            const payload = {
                ...values,
                requiredSkillIds: values.requiredSkillIds || [],
                optionalSkillIds: values.optionalSkillIds || []
            };
            await api.post('/jobs', payload);
            message.success('Job posted successfully!');
            setIsJobModalOpen(false);
            jobForm.resetFields();
            fetchDashboardData();
        } catch (err) {
            message.error(err.response?.data?.message || 'Failed to post job.');
        }
    };

    const handleUpdateStatus = async (values) => {
        try {
            await api.put(`/applications/${selectedApp.applicationId}/status`, values);
            message.success('Application status updated!');
            setIsAppStatusModalOpen(false);
            fetchDashboardData();
        } catch (err) {
            message.error('Failed to update status.');
        }
    };

    const getStatusTag = (status) => {
        let color = 'default';
        if (status === 'Applied') color = 'blue';
        if (status === 'Shortlisted') color = 'cyan';
        if (status === 'Interview') color = 'orange';
        if (status === 'Offer') color = 'green';
        if (status === 'Rejected') color = 'red';
        return <Tag color={color}>{status.toUpperCase()}</Tag>;
    };

    const jobColumns = [
        { title: 'Job Title', dataIndex: 'title', key: 'title', render: (text) => <Text strong>{text}</Text> },
        { title: 'Type', dataIndex: 'jobType', key: 'jobType', render: (type) => <Tag color="processing">{type}</Tag> },
        { title: 'Location', dataIndex: 'location', key: 'location' },
        { title: 'Applicants', dataIndex: 'applicationCount', key: 'applicationCount', render: (count) => <Badge count={count} color={token.colorPrimary} /> },
        { title: 'Status', dataIndex: 'status', key: 'status', render: (s) => <Tag color={s === 'Open' ? 'success' : 'default'}>{s}</Tag> },
        { title: 'Posted At', dataIndex: 'postedAt', key: 'postedAt', render: (date) => new Date(date).toLocaleDateString() },
    ];

    const applicantColumns = [
        { 
            title: 'Candidate', 
            dataIndex: 'candidateFullName', 
            key: 'candidate', 
            render: (name, record) => (
                <Space>
                    <Avatar icon={<UserOutlined />} />
                    <div>
                        <Text strong>{name}</Text>
                        <div style={{ fontSize: '12px' }}><Text type="secondary">{record.headline}</Text></div>
                    </div>
                </Space>
            ) 
        },
        { 
            title: 'Match Score', 
            dataIndex: 'matchScore', 
            key: 'matchScore', 
            sorter: (a, b) => a.matchScore - b.matchScore,
            render: (score) => <Tag color={score > 80 ? 'green' : score > 50 ? 'orange' : 'red'}>{Math.round(score)}%</Tag> 
        },
        { title: 'Applied For', dataIndex: 'jobTitle', key: 'jobTitle' },
        { title: 'Status', dataIndex: 'status', key: 'status', render: (s) => getStatusTag(s) },
        { 
            title: 'Action', 
            key: 'action', 
            render: (_, record) => (
                <Space>
                    <Tooltip title="View Details">
                        <Button icon={<EyeOutlined />} type="text" />
                    </Tooltip>
                    <Button 
                        type="primary" 
                        size="small" 
                        onClick={() => { setSelectedApp(record); setIsAppStatusModalOpen(true); statusForm.setFieldsValue({ status: record.status }); }}
                    >
                        Update Status
                    </Button>
                </Space>
            )
        }
    ];

    if (loading && !profile) return <div style={{ textAlign: 'center', marginTop: 100 }}><Spin size="large" /></div>;

    const items = [
        {
            key: 'overview',
            label: <span><AppstoreOutlined /> Overview</span>,
            children: (
                <Row gutter={[24, 24]}>
                    <Col xs={24} md={8}>
                        <Card variant="borderless" style={{ background: token.colorInfoBg, borderRadius: 16 }}>
                            <Statistic title="Total Jobs Posted" value={analytics?.totalJobsPosted || jobs.length} prefix={<RocketOutlined />} />
                        </Card>
                    </Col>
                    <Col xs={24} md={8}>
                        <Card variant="borderless" style={{ background: token.colorSuccessBg, borderRadius: 16 }}>
                            <Statistic title="Total Applications Received" value={analytics?.totalApplicationsReceived || applicants.length} prefix={<TeamOutlined />} />
                        </Card>
                    </Col>
                    <Col xs={24} md={8}>
                        <Card variant="borderless" style={{ background: token.colorWarningBg, borderRadius: 16 }}>
                            <Statistic title="Active Openings" value={analytics?.activeJobs || jobs.filter(j => j.status === 'Open').length} prefix={<TrophyOutlined />} />
                        </Card>
                    </Col>
                </Row>
            )
        },
        {
            key: 'jobs',
            label: <span><RocketOutlined /> My Jobs</span>,
            children: (
                <Card variant="borderless" style={{ boxShadow: token.boxShadowTertiary, borderRadius: token.borderRadiusLG }} extra={<Button type="primary" icon={<PlusOutlined />} onClick={() => setIsJobModalOpen(true)}>Post New Job</Button>}>
                    <Table dataSource={jobs} columns={jobColumns} rowKey="id" pagination={{ pageSize: 5 }} />
                </Card>
            )
        },
        {
            key: 'applicants',
            label: <span><TeamOutlined /> Applicants</span>,
            children: (
                <Card variant="borderless" style={{ boxShadow: token.boxShadowTertiary, borderRadius: token.borderRadiusLG }}>
                    <Table dataSource={applicants} columns={applicantColumns} rowKey="applicationId" />
                </Card>
            )
        },
        {
            key: 'analytics',
            label: <span><BarChartOutlined /> Hiring Analytics</span>,
            children: (
                <Row gutter={[24, 24]}>
                    <Col xs={24} md={12}>
                        <Card title="Hiring Pipeline Conversion" variant="borderless" style={{ boxShadow: token.boxShadowTertiary, borderRadius: token.borderRadiusLG }}>
                            <Row align="middle" justify="space-around" style={{ textAlign: 'center', padding: '20px 0' }}>
                                <Col span={12}>
                                    <Progress 
                                        type="circle" 
                                        percent={analytics?.totalApplicationsReceived ? Math.round((analytics.shortlistedCandidates / analytics.totalApplicationsReceived) * 100) : 0} 
                                        format={(percent) => `${percent}%`}
                                        strokeColor={token.colorInfo}
                                    />
                                    <div style={{ marginTop: 12 }}><Text strong>Shortlisted</Text></div>
                                </Col>
                                <Col span={12}>
                                    <Progress 
                                        type="circle" 
                                        percent={analytics?.totalApplicationsReceived ? Math.round((analytics.offeredCandidates / analytics.totalApplicationsReceived) * 100) : 0} 
                                        format={(percent) => `${percent}%`}
                                        strokeColor={token.colorSuccess}
                                    />
                                    <div style={{ marginTop: 12 }}><Text strong>Hired / Offered</Text></div>
                                </Col>
                            </Row>
                            <Divider />
                            <div style={{ textAlign: 'center' }}>
                                <Text type="secondary">Based on {analytics?.totalApplicationsReceived || 0} total applications received.</Text>
                            </div>
                        </Card>
                    </Col>
                    <Col xs={24} md={12}>
                        <Card title="Recruitment Stats Summary" variant="borderless" style={{ boxShadow: token.boxShadowTertiary, borderRadius: token.borderRadiusLG }}>
                            <Space direction="vertical" style={{ width: '100%' }} size="large">
                                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                    <Text>Total Applications</Text>
                                    <Text strong>{analytics?.totalApplicationsReceived}</Text>
                                </div>
                                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                    <Text>Shortlisted Candidates</Text>
                                    <Text strong>{analytics?.shortlistedCandidates}</Text>
                                </div>
                                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                    <Text>Offers Extended</Text>
                                    <Text strong>{analytics?.offeredCandidates}</Text>
                                </div>
                                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                    <Text>Candidate Satisfaction Rate</Text>
                                    <Text strong>94% <Tag color="green">High</Tag></Text>
                                </div>
                            </Space>
                        </Card>
                    </Col>
                </Row>
            )
        }
    ];

    return (
        <div style={{ padding: '24px 0' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
                <div>
                    <Title level={2} className="hawaii-title">Recruiter Dashboard</Title>
                    <Text type="secondary">Welcome back, {profile?.fullName || user.fullName}</Text>
                </div>
                <Button icon={<EditOutlined />} onClick={() => setIsProfileModalOpen(true)}>Edit Profile</Button>
            </div>

            <Tabs defaultActiveKey="overview" items={items} type="card" />

            {/* Recruiter Profile Edit Modal */}
            <Modal
                title="Edit Company Profile"
                open={isProfileModalOpen}
                onCancel={() => setIsProfileModalOpen(false)}
                footer={null}
            >
                <Form form={profileForm} layout="vertical" onFinish={handleUpdateProfile}>
                    <Form.Item name="fullName" label="Recruiter Name" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item name="companyName" label="Company Name" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item name="companyDescription" label="About the Company">
                        <Input.TextArea rows={4} />
                    </Form.Item>
                    <Form.Item name="companyWebsite" label="Company Website">
                        <Input placeholder="https://example.com" />
                    </Form.Item>
                    <Form.Item name="location" label="Location">
                        <Input placeholder="City, Country" />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit" block>Save Changes</Button>
                    </Form.Item>
                </Form>
            </Modal>

            {/* Post Job Modal */}
            <Modal
                title="Post a New Job Opportunity"
                open={isJobModalOpen}
                onCancel={() => setIsJobModalOpen(false)}
                footer={null}
                width={700}
            >
                <Form form={jobForm} layout="vertical" onFinish={handleCreateJob} initialValues={{ jobType: 'FullTime' }}>
                    <Row gutter={16}>
                        <Col span={16}>
                            <Form.Item name="title" label="Job Title" rules={[{ required: true }]}>
                                <Input placeholder="e.g. Senior Backend Engineer" />
                            </Form.Item>
                        </Col>
                        <Col span={8}>
                            <Form.Item name="jobType" label="Job Type" rules={[{ required: true }]}>
                                <Select>
                                    <Option value="FullTime">Full-Time</Option>
                                    <Option value="PartTime">Part-Time</Option>
                                    <Option value="Contract">Contract</Option>
                                    <Option value="Remote">Remote</Option>
                                    <Option value="Internship">Internship</Option>
                                </Select>
                            </Form.Item>
                        </Col>
                    </Row>
                    
                    <Form.Item name="description" label="Description" rules={[{ required: true }]}>
                        <Input.TextArea rows={6} placeholder="Describe the role, responsibilities, and requirements..." />
                    </Form.Item>
                    
                    <Row gutter={16}>
                        <Col span={12}>
                            <Form.Item name="location" label="Location" rules={[{ required: true }]}>
                                <Input placeholder="City, Country or 'Remote'" />
                            </Form.Item>
                        </Col>
                        <Col span={12}>
                            <Form.Item name="experienceLevel" label="Experience Level">
                                <Select placeholder="Expertise level">
                                    <Option value="Junior">Entry Level</Option>
                                    <Option value="Mid">Mid Level</Option>
                                    <Option value="Senior">Senior Level</Option>
                                    <Option value="Lead">Lead Level</Option>
                                </Select>
                            </Form.Item>
                        </Col>
                    </Row>

                    <Row gutter={16}>
                        <Col span={12}>
                            <Form.Item name="salaryMin" label="Min Salary">
                                <InputNumber style={{ width: '100%' }} formatter={(v) => `$ ${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')} />
                            </Form.Item>
                        </Col>
                        <Col span={12}>
                            <Form.Item name="salaryMax" label="Max Salary">
                                <InputNumber style={{ width: '100%' }} formatter={(v) => `$ ${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')} />
                            </Form.Item>
                        </Col>
                    </Row>

                    <Form.Item name="requiredSkillIds" label="Required Skills">
                        <Select mode="multiple" placeholder="Search and select skills">
                            {allSkills.map(s => <Option key={s.id} value={s.id}>{s.name}</Option>)}
                        </Select>
                    </Form.Item>

                    <Form.Item>
                        <Button type="primary" htmlType="submit" size="large" block>Launch Job Posting</Button>
                    </Form.Item>
                </Form>
            </Modal>

            {/* Update Application Status Modal */}
            <Modal
                title={`Update Status: ${selectedApp?.candidateFullName}`}
                open={isAppStatusModalOpen}
                onCancel={() => setIsAppStatusModalOpen(false)}
                footer={null}
            >
                <Form form={statusForm} layout="vertical" onFinish={handleUpdateStatus}>
                    <Form.Item name="status" label="New Status" rules={[{ required: true }]}>
                        <Select>
                            <Option value="Applied">Applied</Option>
                            <Option value="Shortlisted">Shortlisted</Option>
                            <Option value="Interview">Interview</Option>
                            <Option value="Offer">Offer</Option>
                            <Option value="Rejected">Rejected</Option>
                        </Select>
                    </Form.Item>
                    <Form.Item name="notes" label="Notes (Optional)">
                        <Input.TextArea placeholder="Feedback for the candidate or internal notes..." />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit" block>Update Status</Button>
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

const Statistic = ({ title, value, prefix }) => {
    return (
        <div style={{ padding: '20px' }}>
            <Text type="secondary" style={{ fontSize: '14px', display: 'block', marginBottom: 4 }}>{title}</Text>
            <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                <span style={{ fontSize: '24px' }}>{prefix}</span>
                <span style={{ fontSize: '28px', fontWeight: 'bold' }}>{value}</span>
            </div>
        </div>
    );
};

export default RecruiterDashboard;
