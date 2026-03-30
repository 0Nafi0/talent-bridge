import { useState, useEffect } from 'react';
import { Typography, Row, Col, Card, Input, Select, Tag, Button, Empty, Spin, theme, Modal, Form, message, Divider, Space, Descriptions } from 'antd';
import { SearchOutlined, EnvironmentOutlined, DollarOutlined, TrophyOutlined, RocketOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;

const JobBoard = () => {
    const { user } = useAuth();
    const [jobs, setJobs] = useState([]);
    const [loading, setLoading] = useState(true);
    const { token } = theme.useToken();
    
    // Detailed View
    const [selectedJob, setSelectedJob] = useState(null);
    const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
    const [isApplyModalOpen, setIsApplyModalOpen] = useState(false);
    const [applying, setApplying] = useState(false);
    const [applyForm] = Form.useForm();

    // Filters
    const [keyword, setKeyword] = useState('');
    const [jobType, setJobType] = useState('');

    const fetchJobs = async () => {
        setLoading(true);
        try {
            let query = '/jobs?';
            if (keyword) query += `keyword=${keyword}&`;
            if (jobType) query += `jobType=${jobType}&`;
            
            const response = await api.get(query);
            setJobs(response.data.items || response.data);
        } catch (error) {
            console.error('Failed to fetch jobs', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchJobs();
    }, []);

    const onSearch = () => {
        fetchJobs();
    };

    const handleApply = async (values) => {
        setApplying(true);
        try {
            await api.post('/applications', {
                jobId: selectedJob.id,
                coverLetter: values.coverLetter
            });
            message.success(`Application sent for ${selectedJob.title}!`);
            setIsApplyModalOpen(false);
            setIsDetailModalOpen(false);
            applyForm.resetFields();
        } catch (err) {
            message.error(err.response?.data?.message || 'Failed to submit application.');
        } finally {
            setApplying(false);
        }
    };

    const showDetails = (job) => {
        setSelectedJob(job);
        setIsDetailModalOpen(true);
    };

    return (
        <div style={{ padding: '24px 0' }}>
            <div style={{ textAlign: 'center', marginBottom: 40 }}>
                <Title level={2} className="hawaii-title">Explore Opportunities</Title>
                <Text type="secondary" style={{ fontSize: '1.1rem' }}>Browse the latest jobs posted by top recruiters</Text>
            </div>

            <Card variant="borderless" style={{ marginBottom: 32, boxShadow: token.boxShadowTertiary }}>
                <Row gutter={[16, 16]}>
                    <Col xs={24} md={10}>
                        <Input 
                            size="large" 
                            placeholder="Job title, keywords, or company" 
                            prefix={<SearchOutlined />} 
                            value={keyword}
                            onChange={(e) => setKeyword(e.target.value)}
                            onPressEnter={onSearch}
                        />
                    </Col>
                    <Col xs={24} md={8}>
                        <Select 
                            size="large" 
                            style={{ width: '100%' }} 
                            placeholder="Job Type" 
                            allowClear
                            onChange={(val) => setJobType(val)}
                        >
                            <Option value="FullTime">Full-Time</Option>
                            <Option value="PartTime">Part-Time</Option>
                            <Option value="Contract">Contract</Option>
                            <Option value="Remote">Remote</Option>
                        </Select>
                    </Col>
                    <Col xs={24} md={6}>
                        <Button type="primary" size="large" block onClick={onSearch}>Search</Button>
                    </Col>
                </Row>
            </Card>

            {loading ? (
                <div style={{ textAlign: 'center', padding: '50px 0' }}>
                    <Spin size="large" />
                </div>
            ) : jobs.length === 0 ? (
                <Empty description="No jobs found matching your criteria" />
            ) : (
                <Row gutter={[24, 24]}>
                    {jobs.map(job => (
                        <Col xs={24} md={12} lg={8} key={job.id}>
                            <Card 
                                variant="borderless" 
                                hoverable 
                                onClick={() => showDetails(job)}
                                style={{ height: '100%', display: 'flex', flexDirection: 'column', boxShadow: token.boxShadowTertiary, borderRadius: token.borderRadiusLG }}
                                styles={{ body: { flex: 1, display: 'flex', flexDirection: 'column' } }}
                                actions={[
                                    <Button type="primary" style={{ width: '90%' }}>View Details</Button>
                                ]}
                            >
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '16px' }}>
                                    <Title level={4} style={{ margin: 0, fontSize: '1.2rem', color: token.colorPrimary }}>{job.title}</Title>
                                    <Tag color={token.colorPrimary}>{job.jobType}</Tag>
                                </div>
                                <Text strong style={{ display: 'block', marginBottom: 8 }}>{job.companyName}</Text>
                                <Paragraph ellipsis={{ rows: 2 }} type="secondary" style={{ flex: 1 }}>
                                    {job.description}
                                </Paragraph>
                                <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', marginTop: '16px', color: token.colorTextSecondary }}>
                                    <span><EnvironmentOutlined style={{ marginRight: 8 }}/>{job.location}</span>
                                    <span><DollarOutlined style={{ marginRight: 8 }}/>${job.salaryMin?.toLocaleString()} - ${job.salaryMax?.toLocaleString()}</span>
                                    <span><TrophyOutlined style={{ marginRight: 8 }}/>{job.experienceLevel} Level</span>
                                </div>
                            </Card>
                        </Col>
                    ))}
                </Row>
            )}

            {/* Job Details Modal */}
            <Modal
                title={<Title level={3} style={{ margin: 0 }}>{selectedJob?.title}</Title>}
                open={isDetailModalOpen}
                onCancel={() => setIsDetailModalOpen(false)}
                width={800}
                footer={[
                    <Button key="back" onClick={() => setIsDetailModalOpen(false)}>Close</Button>,
                    (!user || user.role === 'Candidate') && (
                        <Button 
                            key="apply" 
                            type="primary" 
                            icon={<RocketOutlined />} 
                            onClick={() => {
                                if (!user) {
                                    message.warning('Please login to apply for jobs!');
                                    return;
                                }
                                setIsApplyModalOpen(true);
                            }}
                        >
                            Apply Now
                        </Button>
                    ),
                    (user?.role === 'Recruiter') && (
                        <Tag color="orange" style={{ padding: '4px 12px' }}>Recruiters cannot apply for jobs</Tag>
                    )
                ]}
            >
                {selectedJob && (
                    <div style={{ padding: '10px 0' }}>
                        <Descriptions column={2}>
                            <Descriptions.Item label="Company"><Text strong>{selectedJob.companyName}</Text></Descriptions.Item>
                            <Descriptions.Item label="Location">{selectedJob.location}</Descriptions.Item>
                            <Descriptions.Item label="Job Type"><Tag color="processing">{selectedJob.jobType}</Tag></Descriptions.Item>
                            <Descriptions.Item label="Salary Range">${selectedJob.salaryMin?.toLocaleString()} - ${selectedJob.salaryMax?.toLocaleString()}</Descriptions.Item>
                            <Descriptions.Item label="Experience">{selectedJob.experienceLevel}</Descriptions.Item>
                            <Descriptions.Item label="Posted On">{new Date(selectedJob.createdAt).toLocaleDateString()}</Descriptions.Item>
                        </Descriptions>
                        <Divider />
                        <Title level={4}>Job Description</Title>
                        <Paragraph style={{ fontSize: '1.1rem', lineHeight: '1.8' }}>
                            {selectedJob.description}
                        </Paragraph>
                        
                        {selectedJob.requiredSkills && selectedJob.requiredSkills.length > 0 && (
                            <>
                                <Divider />
                                <Title level={4}>Required Skills</Title>
                                <Space wrap>
                                    {selectedJob.requiredSkills.map(s => (
                                        <Tag key={s.id} color="volcano">{s.name}</Tag>
                                    ))}
                                </Space>
                            </>
                        )}
                    </div>
                )}
            </Modal>

            {/* Apply Form Modal */}
            <Modal
                title={<span><ArrowLeftOutlined style={{ marginRight: 12, cursor: 'pointer' }} onClick={() => setIsApplyModalOpen(false)} /> Apply for {selectedJob?.title}</span>}
                open={isApplyModalOpen}
                footer={null}
                onCancel={() => setIsApplyModalOpen(false)}
            >
                <Form form={applyForm} layout="vertical" onFinish={handleApply}>
                    <div style={{ marginBottom: 20 }}>
                        <Text type="secondary">Tell the recruiter why you're a great fit for this role at {selectedJob?.companyName}.</Text>
                    </div>
                    <Form.Item 
                        name="coverLetter" 
                        label="Cover Letter / Introduction" 
                        rules={[{ required: true, message: 'Please introduce yourself to the recruiter!' }]}
                    >
                        <Input.TextArea rows={6} placeholder="Write a brief cover letter..." />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit" size="large" block loading={applying}>Submit Application</Button>
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default JobBoard;
